using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GwswDefinitionImporter: GwswBaseImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswDefinitionImporter));
        public bool ImportWithFiles;
        #region Gwsw Definition Importer

        /// <summary>
        /// First imports a Gwsw Definition file and all the files mentioned on it. When the definition file is imported, 
        /// a mapping per file is created. Each of the mappings contains a list of attributes, their types, and their default values (when applicable).
        /// After the mappings are created, the importer will look for all the files mentioned on it and will try to import
        /// them, using said mappings, into Network Features.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        private List<INetworkFeature> ImportFilesFromDefinitionFile(string path, IHydroNetwork network = null)
        {
            ImportDefinitionFile(path);
            Log.InfoFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Attributes_mapped__0_, GwswAttributesDefinition.Count);

            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);

            var uniqueFileList = GwswAttributesDefinition.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
            var importedFeatureElements = new List<INetworkFeature>();

            //Read each one of the files.
            foreach (var fileName in uniqueFileList)
            {
                var directoryName = Path.GetDirectoryName(path);
                var elementFilePath = Path.Combine(directoryName, fileName);
                if (!File.Exists(elementFilePath))
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__, elementFilePath);
                    continue;
                }
                
                //Call the base importer.
                var importedElement = base.ImportItem(elementFilePath, network);
                var elementAsFeature = importedElement as List<INetworkFeature>;
                if (elementAsFeature == null)
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_File__0__was_not_imported_correctly_, fileName);
                    continue;
                }

                importedFeatureElements.AddRange(elementAsFeature);
            }

            return importedFeatureElements;
        }

        private DataTable ImportDefinitionFile(string path)
        {
            // Import definition file with predefined CSV columns.
            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimeterComma,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("Bestandsnaam", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ElementName", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Kolomnaam", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Code", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Code_International", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Definitie", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Type", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Eenheid", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Verplicht", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Standaardwaarde", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Opmerking", typeof (string)),
                        new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                    },
                }
            };

            var importedTable = ImportFileAsDataTable(path, mappingData);
            if (importedTable == null)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, path);
                return null;
            }

            //Load the related tables referred in the definition file.
            var attributeList = new EventedList<GwswAttributeType>();

            // Create new attributes for each occurrence.
            // Retreive the files that need to be read.
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

                var attribute = new GwswAttributeType()
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

            return importedTable;
        }

        #endregion

        #region IFileImporter

        public override object ImportItem(string path, object target = null)
        {
            var fmModel = target as IWaterFlowFMModel;
            if (ImportWithFiles)
                return ImportFilesFromDefinitionFile(path, fmModel?.Network);

            return ImportDefinitionFile(path);
        }

        public override string Name
        {
            get { return "GWSW Definition and Features importer"; }
        }

        #endregion
    }
}