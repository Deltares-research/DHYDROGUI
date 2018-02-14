using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GwswFileImporter: IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImporter));
//        private const char CsvDelimeterComma = ',';
//        private const char CsvDelimeterSemiColon = ';';
        private CsvSettings csvSettings;

        public char CsvDelimeter { get; set; }

        public IList<string> FilesToImport;

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

        private void SetProgress(string currentStepName, int currentStep, int totalSteps)
        {
            ProgressChanged?.Invoke(currentStepName, currentStep, totalSteps);
        }

        public GwswFileImporter()
        {
            FilesToImport = new List<string>();
            GwswAttributesDefinition = new EventedList<GwswAttributeType>();
            GwswDefaultFeatures = new Dictionary<string, List<string>>();
            CsvDelimeter = ';'; //Default value, can be changed.
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

            if( !String.IsNullOrEmpty(path) ) FilesToImport = new EventedList<string>{path};

            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);

            var fmModel = target as IWaterFlowFMModel;
            var network = fmModel?.Network;
            var importedFeatureElements = new List<INetworkFeature>();
            var subSteps = network != null ? 3 : 2;
            var totalSteps = FilesToImport.Count * subSteps; /*1. Import Gwsw Element, 2. Import INetworkFeature, 3.Add to Network.*/
            foreach (var filePath in FilesToImport)
            {
                var fileStep = FilesToImport.IndexOf(filePath) * subSteps;
                if (!File.Exists(filePath))
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__, filePath);
                    continue;
                }
                
                //Get the file content as a list of Gwsw Elements
                SetProgress($"Importing file {filePath}", fileStep, totalSteps);
                var elementList = ImportGwswElementList(filePath); // TODO Sil deze stap duurt enkele seconden
                if (!elementList.Any()) continue;

                fileStep++;
                SetProgress($"Importing file {Path.GetFileName(filePath)}", fileStep, totalSteps);
                var elementsCreated = SewerFeatureFactory.CreateMultipleInstances(elementList, network).ToList();
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_File__0__imported__1__features_, filePath, elementsCreated.Count);
                
                SewerFeatureType elementType;
                var elementTypeName = elementList.FirstOrDefault()?.ElementTypeName;
                if (Enum.TryParse(elementTypeName, out elementType))
                {
                    fileStep++;
                    SetProgress($"Importing file {filePath}", fileStep, totalSteps);
                    InsertFeatures(elementsCreated, network, elementType);
                }

                if (elementsCreated.Any())
                    importedFeatureElements.AddRange(elementsCreated);
            }

            return importedFeatureElements;
        }

        /// <summary>
        /// It loads a definition file into the dictionary GwswAttributeDefinition
        /// It also sets the initial FilesToImport
        /// </summary>
        /// <param name="path">Path to the definition file</param>
        /// <returns>DataTable describing contents of the CSV file</returns>
        public DataTable LoadDefinitionFile(string path)
        {
            // Import definition file with predefined CSV columns.
            var mappingData = CsvMappingData;
            var importedTable = ImportFileAsDataTable(path, mappingData);
            if (importedTable == null || importedTable.Rows.Count == 0)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, path);
                return null;
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
            Log.InfoFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Attributes_mapped__0_, GwswAttributesDefinition.Count);

            try
            {
                GwswDefaultFeatures = GetDefinitionFeatureFiles(path);
            }
            catch (Exception)
            {
                GwswAttributesDefinition = new EventedList<GwswAttributeType>();
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_, path);
                return null;
            }

            FilesToImport = new EventedList<string>(GwswDefaultFeatures?.Select( f => f.Value[2]));

            return importedTable;
        }

        /// <summary>
        /// Given a file path, it tries to import a CSV file and generate Gwsw elements out of the data on it.
        /// </summary>
        /// <param name="path">The location of the CSV file we want to transform into Gwsw elements.</param>
        /// <returns>List of GwswElements or null</returns>
        public IList<GwswElement> ImportGwswElementList(string path)
        {
            var mapping = CreateCsvMappingDataForFile(path);
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
            
            foreach (DataRow dataRow in importedDataTable.Rows)
            {
                var element = new GwswElement { ElementTypeName = elementTypeName };
                var rowValues = dataRow.ItemArray.ToList();
                var columnIndex = 0;
                foreach (var column in rowValues)
                {
                    var attribute = new GwswAttribute
                    {
                        LineNumber = importedDataTable.Rows.IndexOf(dataRow),
                        ValueAsString = column.ToString()
                    };
                    var columnName = importedDataTable.Columns[columnIndex].ColumnName;
                    columnIndex++;
                    if (GwswAttributesDefinition != null)
                    {
                        var foundAttributeType = GwswAttributesDefinition.FirstOrDefault(attr => attr.ElementName.Equals(elementTypeName) && attr.Key.Equals(columnName));
                        if (foundAttributeType == null)
                        {
                            Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Row__0__column__1__of_file__2__was_not_mapped_correctly_, importedDataTable.Rows.IndexOf(dataRow), columnName, path);
                            continue;
                        }
                        attribute.GwswAttributeType = foundAttributeType;
                    }

                    element.GwswAttributeList.Add(attribute);
                }

                elementList.Add(element);
            }

            return elementList;
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

        private IDictionary<string, List<string>> GetDefinitionFeatureFiles(string definitionPath)
        {
            var directoryName = Path.GetDirectoryName(definitionPath);

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
                        valueList.Add(Path.Combine(directoryName, grp.Key));
                        return valueList;
                    });

            return dictionary;
        }

        private CsvMappingData CreateCsvMappingDataForFile(string fileName)
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
                FieldToColumnMapping = fileColumnMapping
            };
            return mapping;
        }

        private void InsertFeatures(IEnumerable<INetworkFeature> features, IHydroNetwork network, SewerFeatureType type)
        {
            if (network == null) return;
            switch (type)
            {
                case SewerFeatureType.Connection:
                    var branches = network.Branches;
                    InsertStructures(features, branches);
                    break;
                case SewerFeatureType.Node:
                    var nodes = network.Nodes;
                    InsertStructures(features, nodes);
                    break;
            }
        }

        [InvokeRequired]
        private static void InsertStructures<TFeat>(IEnumerable<INetworkFeature> features, IEventedList<TFeat> list) where TFeat : INameable
        {
            foreach (var feature in features.Where(s => s is TFeat))
            {
                var replaced = false;
                for (var i = 0; i < list.Count; ++i)
                {
                    if (list[i].Name == feature.Name)
                    {
                        list[i] = (TFeat)feature;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                {
                    list.Add((TFeat)feature);
                }
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
    }

    #region Gwsw Types


    // TODO Sil move these classes to seperate files
    public static class GwswElementExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswElementExtensions));
        private const string UniqueId = "UNIQUE_ID";

        public static bool IsNumerical(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute.GwswAttributeType != null && gwswAttribute.GwswAttributeType.AttributeType != null)
                return gwswAttribute.GwswAttributeType.AttributeType.IsNumericalType();

            return false;
        }

        public static bool IsTypeOf(this GwswAttribute gwswAttribute, Type compareType)
        {
            if (gwswAttribute.GwswAttributeType != null && gwswAttribute.GwswAttributeType.AttributeType != null)
                return gwswAttribute.GwswAttributeType.AttributeType == compareType;

            return false;
        }

        public static bool IsValidAttribute(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute == null) return false;
            
            if (gwswAttribute.ValueAsString != null &&
                gwswAttribute.GwswAttributeType != null &&
                gwswAttribute.GwswAttributeType.AttributeType != null)
            {
                return true;
            }

            gwswAttribute.LogInvalidAttribute();
            return false;
        }

        public static int GetElementLine(this GwswElement gwswElement)
        {
            var line = 0;
            /* It should always have attributes, but just in case (mostly testing) we include this check. */
            if( gwswElement.GwswAttributeList.Any())
                return gwswElement.GwswAttributeList.Select(attr => attr?.LineNumber).First() ?? line;

            return line;
        }
        
        public static GwswAttribute GetAttributeFromList(this GwswElement element, string attributeName)
        {
            var attribute = element?.GwswAttributeList?.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(attributeName));
            if (attribute != null)
                return attribute;

            var uniqueIdAttribute = element?.GwswAttributeList?.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(UniqueId));
            Log.WarnFormat(Resources.GwswElementExtensions_GetAttributeFromList_Attribute__0__was_not_found_for_element__1__of_type__2__, attributeName, uniqueIdAttribute?.ValueAsString, element?.ElementTypeName);
            return null;
        }

        public static string GetValidStringValue(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute.IsValidAttribute() && gwswAttribute.IsTypeOf(typeof(string)))
            {
                return gwswAttribute.ValueAsString;
            }

            return null;
        }

        public static bool TryGetValueAsDouble(this GwswAttribute gwswAttribute, out double value)
        {
            value = default(double);
            if (!gwswAttribute.IsValidAttribute() || gwswAttribute.ValueAsString == string.Empty) return false;
            if( !gwswAttribute.IsNumerical())
            {
                gwswAttribute.LogErrorParseType(typeof(double));
                return false;
            }

            try
            {
                value = Convert.ToDouble(gwswAttribute.ValueAsString, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                gwswAttribute.LogErrorParseType(typeof(double));
            }
            return false;
        }

        public static T GetValueFromDescription<T>(this GwswAttribute gwswAttribute)
        {
            var description = gwswAttribute.GetValidStringValue();
            try
            {
                return (T)EnumDescriptionAttributeTypeConverter.GetEnumValue<T>(description);
            }
            catch (Exception)
            {
                Log.WarnFormat(Resources.SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax, description);
            }

            return default(T);
        }

        private static void LogInvalidAttribute(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute.GwswAttributeType == null) return;

            var attributeType = gwswAttribute.GwswAttributeType;
            Log.ErrorFormat(Resources.GwswElementExtensions_LogInvalidAttribute_File__0___line__1___Column__2____3___contains_invalid_value___4___and_will_not_be_imported_
                , attributeType.FileName, gwswAttribute.LineNumber, attributeType.LocalKey, attributeType.Key, gwswAttribute.ValueAsString);
        }

        private static void LogErrorParseType(this GwswAttribute gwswAttribute, Type toType)
        {
            var attr = gwswAttribute.GwswAttributeType;
            Log.ErrorFormat(Resources.GwswElementExtensions_LogErrorParseType_File__0___line__1___element__2___It_was_not_possible_to_parse_attribute__3__from_type__4__to_type__5__
                , attr.FileName, gwswAttribute.LineNumber, attr.ElementName, attr.Name, gwswAttribute.ValueAsString, attr.AttributeType, toType);
        }
    }

    public class GwswElement
    {
        public List<GwswAttribute> GwswAttributeList { get; set; }
        public string ElementTypeName { get; set; }

        public GwswElement()
        {
            GwswAttributeList = new List<GwswAttribute>();
        }
    }

    public class GwswAttribute
    {
        private string valueAsString;
        public GwswAttributeType GwswAttributeType { get; set; }
        public int LineNumber { get; set; }

        public string ValueAsString
        {
            get
            {
                return this.IsTypeOf(typeof(double)) ? ReplaceCommaWithPoint(valueAsString) : valueAsString;
            }
            set { valueAsString = value; }
        }

        private static string ReplaceCommaWithPoint(string doubleString)
        {
            return doubleString.Replace(',', '.');
        }
    }

    public class GwswAttributeType
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswAttributeType));
        private string elementName;

        public string Name { get; set; }
        
        public string ElementName
        {
            get
            {
                if (elementName == null)
                {
                    return elementName;
                }
                return Path.GetFileNameWithoutExtension(elementName); /*The element names might contain extensions*/
            }
            set { elementName = value; }
        }
        
        public string Key { get; set; }
        public string LocalKey { get; set; }
        public string Definition { get; set; }
        public string Mandatory { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public Type AttributeType { get; set; }
        public string DefaultValue { get; set; }
        
        public static Type TryGetParsedValueType(string name, string typeField, string definition, string fileName, int lineNumber)
        {
            try
            {
                return FMParser.GetClrType(name, typeField, ref definition, fileName, lineNumber);
            }
            catch (Exception)
            {
                Log.ErrorFormat(
                    Resources
                        .GwswAttributeType_TryGetParsedValueType_The_type_value__0__on_line__1__file__2___could_not_be_parsed__Please_check_it_is_correctly_written_,
                    name, lineNumber, fileName);
            }

            return null;
        }
    }

    #endregion
}
