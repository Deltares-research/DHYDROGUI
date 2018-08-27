using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{

    /// <summary>
    /// Importer for GWSW files
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Core.IFileImporter" />
    public class GwswFileImporter: IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImporter));

        private CsvSettings csvSettings;

        private class GwswImportManager
        {
            private int totalAmountOfImportSteps;
            private int currentImportStep;
            public int AmountOfImportStepsPerFile;

            public void CalculateTotalAmountOfImportSteps(IList<string> filesToImport)
            {
                var amountOfFiles = filesToImport.Count;
                totalAmountOfImportSteps = amountOfFiles * AmountOfImportStepsPerFile + 1;
            }

            public void ReportProgress(string message)
            {
                currentImportStep++;
                new GwswFileImporter().SetProgress(message, currentImportStep, totalAmountOfImportSteps);
            }

            public void JumpImportStepsForNextFile()
            {
                currentImportStep += AmountOfImportStepsPerFile;
            }
        }

        public GwswFileImporter()
        {
            FilesToImport = new List<string>();
            GwswAttributesDefinition = new EventedList<GwswAttributeType>();
            GwswDefaultFeatures = new Dictionary<string, List<string>>();
            ImportManager = new GwswImportManager();
            CsvDelimeter = ';'; //Default value, can be changed.
            LoadDefinitionFile();
        }

        /// <summary>
        /// Imports the given file as path. If it is null, then the list of files (FilesToImport) will be imported instead. 
        /// A Gwsw Definition file needs to be loaded beforehand with method LoadDefinitionFile.
        /// By default, all files referenced in the GwswDefinitionFile are selected to import.
        /// </summary>
        /// <param name="path">File to import. If this argument is missing then FilesToImport will be taken instead.</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public object ImportItem(string path, object target = null)
        {
            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporter_ImportItem_No_mapping_was_found_to_import_Gwsw_Files_);
                return null;
            }

            if( !string.IsNullOrEmpty(path) ) FilesToImport = new EventedList<string>{path};

            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);

            var fmModel = target as IWaterFlowFMModel;
            var network = fmModel?.Network;

            network?.BeginEdit(new DefaultEditAction("Importing GWSW database"));

            try
            {
                InitializeImportManager();
                var importedFeatureElements = ImportGwswDatabaseToNetwork(network);
                if (network != null)
                {
                    ReportProgress("Adding features to network");
                    AddSewerFeaturesToNetwork(importedFeatureElements, network);
                }
                return importedFeatureElements;
            }
            finally
            {
                network?.EndEdit();
            }
        }

        private GwswImportManager ImportManager { get; }
        
        private IList<ISewerFeature> ImportGwswDatabaseToNetwork(IHydroNetwork network)
        {
            var importedFeatureElements = new List<ISewerFeature>();
            foreach (var filePath in FilesToImport)
            {
                if (!File.Exists(filePath))
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__, filePath);
                    ImportManager.JumpImportStepsForNextFile();
                    continue;
                }

                ReportProgress($"Importing file {filePath}");
                var gwswElements = ImportGwswElementList(filePath);

                ReportProgress("Generating network features");
                var createdSewerEntities = SewerFeatureFactory.CreateSewerEntities(gwswElements, network);
                
                importedFeatureElements.AddRange(createdSewerEntities);
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_File__0__imported__1__features_, filePath, createdSewerEntities.Count);
            }

            return importedFeatureElements;
        }

        private static void AddSewerFeaturesToNetwork(IList<ISewerFeature> importedFeatureElements, IHydroNetwork network)
        {
            importedFeatureElements.ForEach(e => e.AddToHydroNetwork(network));
        }

        private void InitializeImportManager()
        {
            ImportManager.AmountOfImportStepsPerFile = 2;
            ImportManager.CalculateTotalAmountOfImportSteps(FilesToImport);
        }

        private void ReportProgress(string message)
        {
            ImportManager.ReportProgress(message);
        }

        /// <summary>
        /// Loads the feature files from a directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        public void LoadFeatureFiles(string directoryPath)
        {
            Log.InfoFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Attributes_mapped__0_, GwswAttributesDefinition.Count);

            try
            {
                GwswDefaultFeatures = GetDefinitionFeatureFiles(directoryPath);
            }
            catch (Exception)
            {
                GwswAttributesDefinition = new EventedList<GwswAttributeType>();
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, directoryPath);
            }

            FilesToImport = new EventedList<string>(GwswDefaultFeatures?.Select(f => f.Value[2]));
        }

        /// <summary>
        /// Given a file path, it tries to import a CSV file and generate Gwsw elements out of the data on it.
        /// </summary>
        /// <param name="path">The location of the CSV file we want to transform into Gwsw elements.</param>
        /// <returns>List of GwswElements or null</returns>
        public IList<GwswElement> ImportGwswElementList(string path)
        {
            var mapping = GetCsvMappingDataForFileFromDefinition(path);
            var importedDataTable = ImportFileAsDataTable(path, mapping); // TODO Sil -> invalid cast exception from this method
            if (importedDataTable == null)
                return null;

            var elementList = new List<GwswElement>();
            var elementTypeFound = GwswAttributesDefinition.FirstOrDefault(at => at.FileName.Equals(Path.GetFileName(path)));
            var elementTypeName = string.Empty;
            if (elementTypeFound != null)
            {
                elementTypeName = elementTypeFound.ElementName;
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Mapping_file__0__as_element__1_, path, elementTypeName);
            }
            else
            {
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Occurrences_on_file__0__will_not_be_mapped_to_any_element_, path);
                return elementList;
            }

            if (!IsColumnMappingCorrect(path, importedDataTable))
            {
                return elementList;
            }

            foreach (DataRow dataRow in importedDataTable.Rows)
            {
                var element = new GwswElement { ElementTypeName = elementTypeName };
                for (var i = 0; i < dataRow.ItemArray.Length; i++)
                {
                    var cell = dataRow.ItemArray[i];
                    var columnName = importedDataTable.Columns[i].ColumnName;
                    var attribute = new GwswAttribute
                    {
                        LineNumber = importedDataTable.Rows.IndexOf(dataRow),
                        ValueAsString = cell.ToString()
                    };
                    if (GwswAttributesDefinition != null)
                    {
                        var foundAttributeType = GwswAttributesDefinition.FirstOrDefault(attr => attr.ElementName.Equals(elementTypeName) && attr.Key.Equals(columnName));
                        attribute.GwswAttributeType = foundAttributeType;
                    }
                    element.GwswAttributeList.Add(attribute);
                }

                elementList.Add(element);
            }

            return elementList;
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
            var fileAttributes = GwswAttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(Path.GetFileName(path)))).ToList();
            for (var columnIndex = 0; columnIndex < importedDataTable.Columns.Count; columnIndex++)
            {
                var columnName = importedDataTable.Columns[columnIndex].ColumnName;
                var fileAttribute = fileAttributes.First(a => a.Key.Equals(columnName));
                var expectedHeader = fileAttribute.LocalKey;
                var headerName = headersFile[columnIndex];
                if (!expectedHeader.ToLower().Equals(headerName.ToLower().Trim()))
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_column__0__expectedcolumn__1__of_file__2__was_not_mapped_correctly__,
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
        public DataTable ImportFileAsDataTable(string path, CsvMappingData mappingData)
        {
            if (mappingData == null)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, path);
                return null;
            }

            var csvImporter = new CsvImporter();
            var importedCsv = new DataTable();
            try
            {
                importedCsv = csvImporter.ImportCsv(path, mappingData); // TODO Sil -> Invalid cast exception from this method
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_, path, e.Message);
            }

            return importedCsv;
        }

        /// <summary>
        /// Gets or sets the CSV delimeter to split a line in a csv file.
        /// </summary>
        /// <value>
        /// The CSV delimeter.
        /// </value>
        public char CsvDelimeter { get; set; }

        /// <summary>
        /// Gets or sets the files to import.
        /// </summary>
        /// <value>
        /// The files to import.
        /// </value>
        public IList<string> FilesToImport{ get; set; }

    private CsvSettings CsvSettingsSemiColonDelimeted
        {
            get
            {
                return csvSettings ?? (csvSettings = new CsvSettings
                {
                    Delimiter = CsvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                });
            }
        }
        public IEventedList<GwswAttributeType> GwswAttributesDefinition { get; private set; }

        /// <summary>
        /// Dictionary content:
        /// Key = Feature FileName.
        /// Value = List containing 3 strings:
        ///     [0] <string>ElementName</string>
        ///     [1] <string>SewerFeatureType (mapped value)</string>
        ///     [2] <string>Full path</string>
        /// </summary>
        public IDictionary<string, List<string>> GwswDefaultFeatures { get; private set; }

        private CsvMappingData CsvMappingData
        {
            get
            {
                var mappingData = new CsvMappingData
                {
                    Settings = new CsvSettings
                    {
                        Delimiter = CsvDelimeter,
                        FirstRowIsHeader = true,
                        SkipEmptyLines = true
                    },
                    FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                    {
                        {
                            new CsvRequiredField("Bestandsnaam", typeof(string)),
                            new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("ElementName", typeof(string)),
                            new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Kolomnaam", typeof(string)),
                            new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Code", typeof(string)),
                            new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Code_International", typeof(string)),
                            new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Definitie", typeof(string)),
                            new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Type", typeof(string)),
                            new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Eenheid", typeof(string)),
                            new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Verplicht", typeof(string)),
                            new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Standaardwaarde", typeof(string)),
                            new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                        },
                        {
                            new CsvRequiredField("Opmerking", typeof(string)),
                            new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                        },
                    }
                };
                return mappingData;
            }
        }

        #region IFileImporter

        public string Name
        {
            get { return "GWSW Feature File importer"; }
        }

        public string Category
        {
            get { return "1D / 2D"; }
        }

        public Bitmap Image
        {
            get { return Resources.StructureFeatureSmall; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IWaterFlowFMModel);
                yield return typeof(INetwork);
            }
        }

        public bool CanImportOnRootLevel { get { return false; } }
        public string FileFilter { get { return "GWSW Csv Files (*.csv)|*.csv"; } }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get { return false; } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        #endregion

        private void SetProgress(string currentStepName, int currentStep, int totalSteps)
        {
            ProgressChanged?.Invoke(currentStepName, currentStep, totalSteps);
        }

        /// <summary>
        /// It loads a definition file into the dictionary GwswAttributeDefinition
        /// It also sets the initial FilesToImport
        /// </summary>
        /// <returns>DataTable describing contents of the CSV file</returns>
        private void LoadDefinitionFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = @"DeltaShell.Plugins.FMSuite.FlowFM.Resources.GWSWDefinition.csv";
            var csvPreviousDelimeter = CsvDelimeter;
            CsvDelimeter = ',';
            DataTable importedTable;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, resourceName);
                    CsvDelimeter = csvPreviousDelimeter;
                    return;
                }
                var mappingData = CsvMappingData;
                using (var streamReader = new StreamReader(stream))
                {
                    importedTable = ImportFileAsDataTable(streamReader, mappingData);
                    if (importedTable == null || importedTable.Rows.Count == 0)
                    {
                        Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, resourceName);
                        CsvDelimeter = csvPreviousDelimeter;
                        return;
                    }
                }
            }

            //Load the related tables referred in the definition file.
            var attributeList = new EventedList<GwswAttributeType>();

            // Create new attributes for each occurrence.
            // Retrieve the files that need to be read.
            foreach (DataRow row in importedTable.Rows)
            {
                var attributeFile = row.ItemArray[0].ToString();
                var attributeElement = row.ItemArray[1].ToString();
                var attributeName = row.ItemArray[2].ToString();
                var attributeCode = row.ItemArray[3].ToString();
                var attributeCodeInt = row.ItemArray[4].ToString();
                var attributeDefinition = row.ItemArray[5].ToString();
                var attributeType = row.ItemArray[6].ToString();
                var attributeDefaultValue = row.ItemArray[9].ToString();

                var attribute = new GwswAttributeType
                {
                    Name = attributeName,
                    ElementName = attributeElement,
                    Definition = attributeDefinition,
                    FileName = attributeFile,
                    Key = attributeCodeInt,
                    LocalKey = attributeCode,
                    AttributeType = GwswAttributeType.TryGetParsedValueType(attributeName, attributeType, attributeDefinition, attributeFile, importedTable.Rows.IndexOf(row)),
                    DefaultValue = attributeDefaultValue
                };

                attributeList.Add(attribute);
            }

            //If some attributes have a different element from which they should, then we will show an error informing of such a difference.
            attributeList.GroupBy(el => el.FileName).ForEach(gr =>
            {
                var mismatchedElementNames = gr.Select(el => el.ElementName).Distinct().ToList();
                if (mismatchedElementNames.Count > 1)
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_There_is_a_mismatch_for_File_Name__0___currently_mapped_to_different_element_names__1__, gr.Key, string.Concat(mismatchedElementNames));
                }
            });

            GwswAttributesDefinition = attributeList;
            CsvDelimeter = csvPreviousDelimeter;
        }

        /// <summary>
        /// Transforms a CSV data file, into tables that we can handle internally
        /// </summary>
        /// <param name="streamReader">Stream of the CSV file to import.</param>
        /// <param name="mappingData">Delimeters and properties for handling the CSV file.</param>
        /// <returns>DataTable with the content of the CSV file of <param name="streamReader"/>.</returns>
        private DataTable ImportFileAsDataTable(StreamReader streamReader, CsvMappingData mappingData)
        {
            if (mappingData == null)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, streamReader);
                return null;
            }

            var csvImporter = new CsvImporter();
            var importedCsv = new DataTable();
            try
            {
                importedCsv = csvImporter.Extract(csvImporter.SplitToTable(streamReader, mappingData.Settings), mappingData.FieldToColumnMapping, mappingData.Filters);
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_, streamReader, e.Message);
            }

            return importedCsv;
        }

        private IDictionary<string, List<string>> GetDefinitionFeatureFiles(string directoryPath)
        {
            //Get the items to import
            var dictionary = GwswAttributesDefinition?.GroupBy(i => i.FileName)
                .ToDictionary(
                    grp => grp.Key,
                    grp => {
                        var valueList = new List<string>();
                        var elementName = grp.FirstOrDefault( g => !String.IsNullOrEmpty(g.ElementName))?.ElementName;
                        SewerFeatureType featureName;
                        Enum.TryParse(elementName, out featureName);
                        valueList.Add(elementName);
                        valueList.Add(featureName.ToString());
                        valueList.Add(Path.Combine(directoryPath, grp.Key));
                        return valueList;
                    });

            return dictionary;
        }

        private CsvMappingData GetCsvMappingDataForFileFromDefinition(string fileName)
        {
            //Import file elements based on their attributes
            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__, fileName);
                return null;
            }

            var fileAttributes = GwswAttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(fileName))).ToList();
            var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
            //Create column mapping
            fileAttributes.ForEach(
                attr =>
                    fileColumnMapping.Add(
                        new CsvRequiredField(attr.Key, attr.AttributeType),
                        new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

            var mapping = new CsvMappingData
            {
                Settings = CsvSettingsSemiColonDelimeted,
                FieldToColumnMapping = fileColumnMapping,
            };
            return mapping;
        }
    }
}
