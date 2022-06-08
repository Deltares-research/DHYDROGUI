using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class GwswFileImportActivity : Activity
    {
        private readonly GwswFileImporter importer;
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImportActivity));
        private DataTable importedDataTable;
        private string elementTypeName;
        private string File { get; }
        private IEventedList<GwswAttributeType> GwswAttributesDefinition { get; }
        private char CsvDelimeter { get;  }
        private CsvSettings CsvSettingsSemiColonDelimeted { get; }
        public ConcurrentQueue<GwswElement> Elements { get; } = new ConcurrentQueue<GwswElement>();
        
        public GwswFileImportActivity(string file, IEventedList<GwswAttributeType> gwswAttributesDefinition, char csvDelimeter, CsvSettings csvSettingsSemiColonDelimeted, GwswFileImporter importer)
        {
            this.importer = importer;
            Name = $"Import  {Path.GetFileName(file) ?? "<unknown_file>"}";
            File = file;
            GwswAttributesDefinition = gwswAttributesDefinition;
            CsvDelimeter = csvDelimeter;
            CsvSettingsSemiColonDelimeted = csvSettingsSemiColonDelimeted;
        }
        protected override void OnInitialize()
        {
            SetProgressText($"Initializing...");
            importedDataTable = ImportFileAsDataTable(File);

            if (importedDataTable == null)
                Cancel();

            if(importer.ShouldCancel)
                Cancel();

            var elementTypeFound =
                GwswAttributesDefinition.FirstOrDefault(at => at.FileName.Equals(System.IO.Path.GetFileName(File)));
            elementTypeName = string.Empty;
            if (elementTypeFound != null)
            {
                elementTypeName = elementTypeFound.ElementName;
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Mapping_file__0__as_element__1_, File,
                    elementTypeName);
            }
            else
            {
                Log.InfoFormat(
                    Resources
                        .GwswFileImporterBase_ImportItem_Occurrences_on_file__0__will_not_be_mapped_to_any_element_,
                    File);
                Cancel();
            }

            if (!IsColumnMappingCorrect(File, importedDataTable))
            {
                Cancel();
            }
        }

        protected override void OnExecute()
        {
            var rows = importedDataTable.Rows.Cast<DataRow>().ToArray();
            var nrOfRows = rows.Length;
            var stepSize = nrOfRows / 20;
            var current = 0;
            CancellationTokenSource cts = new CancellationTokenSource();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = Environment.ProcessorCount * 2;
            
            
            OrderablePartitioner<DataRow> partitioner = Partitioner.Create(rows, EnumerablePartitionerOptions.NoBuffering);
            Task t = new Task(() => Parallel.ForEach(partitioner, po, (dataRow, s, l) =>
            {
                try
                {
                    if (po.CancellationToken.IsCancellationRequested)
                        s.Break();
                    var lineNumber = importedDataTable.Rows.IndexOf(dataRow) ;
                    //index 0 is always id, if id is empty do not read.
                    if (dataRow.ItemArray.Length > 0 && string.IsNullOrEmpty(dataRow.ItemArray[0].ToString())) return;
                    var element = new GwswElement { ElementTypeName = elementTypeName };
                    for (var i = 0; i < dataRow.ItemArray.Length; i++)
                    {
                        if (importer.ShouldCancel)
                            Cancel();

                        var cell = dataRow.ItemArray[i];
                        var columnName = importedDataTable.Columns[i].ColumnName;
                        var attribute = new GwswAttribute
                        {
                            LineNumber = lineNumber,
                            ValueAsString = cell.ToString()
                        };
                        if (GwswAttributesDefinition != null)
                        {
                            var foundAttributeType = GwswAttributesDefinition.FirstOrDefault(attr =>
                                attr.ElementName.Equals(elementTypeName) && attr.Key.Equals(columnName));
                            attribute.GwswAttributeType = foundAttributeType;
                        }

                        element.GwswAttributeList.Add(attribute);
                    }

                    Elements.Enqueue(element);
                }
                finally
                {
                    Interlocked.Increment(ref current);
                }

            }));

            t.Start();
            int step = 0;
            while (!t.IsCompleted)
            {
                if (stepSize != 0 && current / stepSize > step)
                {
                    step = current / stepSize;

                    EventingHelper.DoWithEvents(() =>
                    {
                        SetProgressText($"Importing {current} / {nrOfRows}");
                    });
                }
                Thread.Sleep(100);
                if (importer != null && importer.ShouldCancel)
                    cts.Cancel();
            }
            
            
            Status = ActivityStatus.Done;
        }

        protected override void OnCancel()
        {

        }

        protected override void OnCleanUp()
        {
            
        }

        protected override void OnFinish()
        {
            
        }
        private bool IsColumnMappingCorrect(string path, DataTable importedDataTable)
        {
            var result = true;
            string headerLineFile;
            using (var reader = new StreamReader(path))
            {
                headerLineFile = reader.ReadLine() ?? string.Empty;
            }

            var headersFile = headerLineFile.Split(CsvDelimeter).Distinct().ToArray();
            var fileAttributes = GwswAttributesDefinition
                .Where(at => at.FileName.Equals(System.IO.Path.GetFileName(System.IO.Path.GetFileName(path)))).ToList();
            for (var columnIndex = 0; columnIndex < importedDataTable.Columns.Count; columnIndex++)
            {
                var columnName = importedDataTable.Columns[columnIndex].ColumnName;
                var fileAttribute = fileAttributes.First(a => a.Key.Equals(columnName));
                var expectedHeader = fileAttribute.LocalKey;
                var headerName = headersFile[columnIndex];
                if (!expectedHeader.ToLower().Equals(headerName.ToLower().Trim()))
                {
                    Log.ErrorFormat(
                        Resources
                            .GwswFileImporterBase_ImportItem_column__0__expectedcolumn__1__of_file__2__was_not_mapped_correctly__,
                        headerName, expectedHeader, path);
                    result = false;
                }
            }

            return result;
        }
        /// <summary>
        /// Transforms a CSV data file, into tables that we can handle internally
        /// </summary>
        /// <param name="path">Location of the CSV file to import.</param>
        /// <param name="mappingData">Delimeters and properties for handling the CSV file.</param>
        /// <returns>DataTable with the content of the CSV file of <param name="path"/>.</returns>
        private DataTable ImportFileAsDataTable(string path, CsvMappingData mappingData = null)
        {
            if (mappingData == null)
                mappingData = GetCsvMappingDataForFileFromDefinition(path);

            if (mappingData == null)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__,
                    path);
                return null;
            }

            var csvImporter = new CsvImporter { AllowEmptyCells = true };
            var importedCsv = new DataTable();
            try
            {
                importedCsv =
                    csvImporter.ImportCsv(path, mappingData); // TODO Sil -> Invalid cast exception from this method
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_, path,
                    e.Message);
            }

            return importedCsv;
        }
        private CsvMappingData GetCsvMappingDataForFileFromDefinition(string fileName)
        {
            //Import file elements based on their attributes
            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__,
                    fileName);
                return null;
            }

            var fileAttributes = GwswAttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(fileName)))
                .ToList();
            var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
            //Create column mapping
            fileAttributes.ForEach(
                attr =>
                    fileColumnMapping.Add(
                        new CsvRequiredField(attr.Key, attr.AttributeType),
                        attr.AttributeType == typeof(DateTime) ? new CsvColumnInfo(fileAttributes.IndexOf(attr), new DateTimeFormatInfo()
                        {
                            FullDateTimePattern = "yyyyMMdd"
                        }) : new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

            var mapping = new CsvMappingData
            {
                Settings = CsvSettingsSemiColonDelimeted,
                FieldToColumnMapping = fileColumnMapping,
            };
            return mapping;
        }
    }
}