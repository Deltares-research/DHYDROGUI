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
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GwswBaseImporter: IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswBaseImporter));
        protected const char CsvDelimeterComma = ',';
        private const char CsvDelimeterSemiColon = ';';
        private CsvSettings csvSettings;
        private CsvSettings CsvSettingsSemiColonDelimeted
        {
            get
            {
                return csvSettings ?? (csvSettings = new CsvSettings
                {
                    Delimiter = CsvDelimeterSemiColon,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                });
            }
        }
        public IEventedList<GwswAttributeType> GwswAttributesDefinition { get; set; }

        #region IFileImporter

        public virtual string Name
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

        /// <summary>
        /// Given a file path, it tries to import a CSV file and generate Gwsw elements out of the data on it.
        /// </summary>
        /// <param name="path">The locatino of the CSV file we want to transform into Gwsw elements.</param>
        /// <param name="target">HydroNetwork where we want to store (later) the imported data.</param>
        /// <returns>List of GwswElements or null</returns>
        public List<GwswElement> ImportGwswElementList(string path)
        {
            var mapping = CreateCsvMappingDataForFile(path);
            var importedDataTable = ImportFileAsDataTable(path, mapping);
            if (importedDataTable == null)
                return null;

            var elementList = new List<GwswElement>();
            var elementTypeFound = GwswAttributesDefinition.FirstOrDefault(at => at.FileName.Equals(Path.GetFileName(path)));
            var elementTypeName = String.Empty;
            if (elementTypeFound != null)
            {
                elementTypeName = elementTypeFound.ElementName;
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Mapping_file__0__as_element__1_, path, elementTypeName);
            }
            else
            {
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Occurrences_on_file__0__will_not_be_mapped_to_any_element_, path);
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
                        ValueAsString = column.ToString()
                    };
                    var columnName = importedDataTable.Columns[columnIndex].ColumnName;
                    if (GwswAttributesDefinition != null)
                    {
                        var foundAttr = GwswAttributesDefinition.FirstOrDefault(attr => attr.Key.Equals(columnName));
                        if (foundAttr == null)
                        {
                            Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Row__0__column__1__of_file__2__was_not_mapped_correctly_, importedDataTable.Rows.IndexOf(dataRow), columnName, path);
                            continue;
                        }
                        attribute.GwswAttributeType = foundAttr;
                    }

                    element.GwswAttributeList.Add(attribute);
                    columnIndex++;
                }

                elementList.Add(element);
            }

            return elementList;
        }

        public virtual object ImportItem(string path, object target = null)
        {
            var elementList = ImportGwswElementList(path);
            
            if (!(target is IHydroNetwork) || !elementList.Any()) return null;
            
            //If the target is a network means we are going to import into it.
            var network = target as IHydroNetwork;
            var elementsCreated = SewerFeatureFactory.CreateMultipleInstances(elementList, network);

            SewerFeatureType elementType;
            var elementTypeName = elementList.FirstOrDefault()?.ElementTypeName;
            if (Enum.TryParse(elementTypeName, out elementType))
            {
                InsertFeatures(elementsCreated, network, elementType);
            }
            return elementsCreated;
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
                importedCsv = csvImporter.ImportCsv(path, mappingData);
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_, path, e.Message);
            }

            return importedCsv;
        }

        protected void InsertFeatures(IEnumerable<INetworkFeature> features, IHydroNetwork network, SewerFeatureType type)
        {
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
    }

    #region Gwsw Types

    public static class GwswElementExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswElementExtensions));

        public static bool IsTypeOf(this GwswAttribute gwswAttribute, Type compareType)
        {
            if (gwswAttribute.GwswAttributeType != null && gwswAttribute.GwswAttributeType.AttributeType != null)
                return gwswAttribute.GwswAttributeType.AttributeType == compareType;

            return false;
        }

        public static bool IsValidAttribute(this GwswAttribute gwswAttribute)
        {
            if (gwswAttribute == null) return false;

            if (!String.IsNullOrEmpty(gwswAttribute.ValueAsString) &&
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
                return gwswElement.GwswAttributeList.Select(attr => attr.GwswAttributeType?.LineNumber).First() ?? line;

            return line;
        }

        public static GwswAttribute GetAttributeFromList(this GwswElement element, string attributeName)
        {
            var attribute = element?.GwswAttributeList?.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(attributeName));
            if (attribute != null)
                return attribute;

            Log.WarnFormat(Resources.SewerFeatureFactory_GetAttributeFromList_Attribute__0__was_not_found_for_element__1_, attributeName, element?.ElementTypeName);
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
            if (!gwswAttribute.IsValidAttribute()) return false;
            if( !gwswAttribute.IsTypeOf(typeof(double)) && !gwswAttribute.IsTypeOf(typeof(int)))
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

            var attr = gwswAttribute.GwswAttributeType;
            Log.ErrorFormat(Resources.GwswElementExtensions_LogInvalidAttribute_File__0___line__1___Attribute__2__is_not_valid_and_will_not_be_imported_, attr.FileName, attr.LineNumber, attr.Name);
        }

        private static void LogErrorParseType(this GwswAttribute gwswAttribute, Type toType)
        {
            var attr = gwswAttribute.GwswAttributeType;
            Log.ErrorFormat(Resources.GwswElementExtensions_LogErrorParseType_File__0___line__1___element__2___It_was_not_possible_to_parse_attribute__3__from_type__4__to_type__5__, attr.FileName, attr.LineNumber, attr.ElementName, attr.Name, gwswAttribute.ValueAsString, attr.AttributeType, toType);
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
        public GwswAttributeType GwswAttributeType { get; set; }

        private string valueAsString;

        public string ValueAsString
        {
            get
            {
                return this.IsTypeOf(typeof(double)) ? ReplaceCommaWithPoint(valueAsString) : valueAsString;
            }
            set { valueAsString = value; }
        }

        public string DefaultValueAsString
        {
            get { return GwswAttributeType.DefaultValue; }
        }

        private static string ReplaceCommaWithPoint(string doubleString)
        {
            return doubleString.Replace(',', '.');
        }
    }

    public class GwswAttributeType
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswAttributeType));

        public string Name { get; set; }

        private string elementName;

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

        public int LineNumber { get; set; }

        public string Key { get; set; }
        public string LocalKey { get; set; }
        public string Definition { get; set; }
        public string Mandatory { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public Type AttributeType { get; set; }
        public string DefaultValue { get; set; }

        public GwswAttributeType()
        {
        }

        public GwswAttributeType(string fileName, int lineNumber, string columnName, string typeField, string codeName, string definition, string mandatory, string defaultValue, string remarks)
        {
            Name = columnName;
            LineNumber = lineNumber;
            Key = codeName;
            Definition = definition;
            Mandatory = mandatory;
            Remarks = remarks;
            FileName = fileName;
            AttributeType = TryGetParsedValueType(Name, typeField, definition, fileName, lineNumber);
            DefaultValue = defaultValue;
        }

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
