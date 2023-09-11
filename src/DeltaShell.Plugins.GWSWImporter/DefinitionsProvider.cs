using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Class which provides definitions.
    /// </summary>
    public class DefinitionsProvider : IDefinitionsProvider
    {
        public DefinitionsProvider():this(new LogHandler("Definitions Provider"))
        {
        }
        public DefinitionsProvider(ILogHandler logHandler)
        {
            LogHandler = logHandler;
        }

        private static readonly CsvMappingData csvMappingData = new CsvMappingData
        {
            Settings = new CsvSettings
            {
                Delimiter = ',',
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
                }
            }
        };
        
        public IEventedList<GwswAttributeType> GetDefinitions(string gwswDirectory)
        {
            var definitionsVersionProvider = new DefinitionsVersionProvider(LogHandler);

            string definitionVersionName = definitionsVersionProvider.GetDefinitionVersionName(gwswDirectory);
            var definitionFilePath = $"{definitionVersionName}.csv";

            return LoadDefinitions(definitionFilePath);
        }

        public ILogHandler LogHandler { get; set; }

        private IEventedList<GwswAttributeType> LoadDefinitions(string gwswFileDefinitionPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = @"DeltaShell.Plugins.ImportExport.GWSW.Resources." + gwswFileDefinitionPath;
            DataTable importedTable;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    LogHandler?.ReportErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_,
                        resourceName);
                    return null;
                }

                var mappingData = csvMappingData;
                using (var streamReader = new StreamReader(stream))
                {
                    importedTable = ImportFileAsDataTable(streamReader, mappingData);
                    if (importedTable == null || importedTable.Rows.Count == 0)
                    {
                        LogHandler?.ReportErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_,
                                                     resourceName);
                        return null;
                    }
                }
            }

            //Load the related tables referred in the definition file.
            var attributeCollection = new EventedList<GwswAttributeType>();

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

                var attribute = new GwswAttributeType(LogHandler)
                {
                    Name = attributeName,
                    ElementName = attributeElement,
                    Definition = attributeDefinition,
                    FileName = attributeFile,
                    Key = attributeCodeInt,
                    LocalKey = attributeCode,
                    DefaultValue = attributeDefaultValue
                };
                attribute.AttributeType = attribute.TryGetParsedValueType(attributeName, attributeType, attributeDefinition, attributeFile, importedTable.Rows.IndexOf(row));

                attributeCollection.Add(attribute);
            }

            //If some attributes have a different element from which they should, then we will show an error informing of such a difference.
            attributeCollection.GroupBy(el => el.FileName).ForEach(gr =>
            {
                var mismatchedElementNames = gr.Select(el => el.ElementName).Distinct().ToList();
                if (mismatchedElementNames.Count > 1)
                {
                    LogHandler?.ReportErrorFormat(
                        Resources
                            .GwswFileImporterBase_ImportDefinitionFile_There_is_a_mismatch_for_File_Name__0___currently_mapped_to_different_element_names__1__,
                        gr.Key, string.Concat(mismatchedElementNames));
                }
            });

            LogHandler?.ReportInfoFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Attributes_mapped__0_,
                attributeCollection.Count);

            return attributeCollection;
        }

        private DataTable ImportFileAsDataTable(StreamReader streamReader, CsvMappingData mappingData)
        {
            if (mappingData == null)
            {
                LogHandler?.ReportErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__,
                    streamReader);
                return null;
            }

            var csvImporter = new CsvImporter {AllowEmptyCells = true};
            var importedCsv = new DataTable();
            try
            {
                importedCsv = csvImporter.Extract(csvImporter.SplitToTable(streamReader, mappingData.Settings),
                    mappingData.FieldToColumnMapping, mappingData.Filters);
            }
            catch (Exception e)
            {
                LogHandler?.ReportErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_,
                    streamReader, e.Message);
            }

            return importedCsv;
        }
    }
}