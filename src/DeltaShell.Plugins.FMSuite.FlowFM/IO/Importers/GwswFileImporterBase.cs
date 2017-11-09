using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    class GwswFileImporterBase : IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImporterBase));
        private char CsvDelimeterComma = ',';
        private char CsvDelimeterSemiColon = ';';
        private CsvSettings _csvSettings;

        private CsvSettings CsvSettingsSemiColonDelimeted
        {
            get
            {
                if (_csvSettings == null)
                {
                    _csvSettings = new CsvSettings
                    {
                        Delimiter = CsvDelimeterSemiColon,
                        FirstRowIsHeader = true,
                        SkipEmptyLines = true
                    };
                }
                
                return _csvSettings;
            }
        }

        public Dictionary<string, SewerFeatureType> stringToSewerTypeConverter = new Dictionary<string, SewerFeatureType>
        {
            { "Node", SewerFeatureType.Node },
            { "Pipe", SewerFeatureType.Pipe },
            { "Structure", SewerFeatureType.Structure },
            { "Surface", SewerFeatureType.Surface },
            { "Runoff", SewerFeatureType.Runoff },
            { "Discharge", SewerFeatureType.Discharge },
            { "Distribution", SewerFeatureType.Distribution },
            { "Meta", SewerFeatureType.Meta }
        };

        public List<GwswAttributeType> AttributesDefinition { get; private set; }

        #region IFileImporter
        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            var importedDataTable = ImportFileAsDataTable(path, target);
            if (importedDataTable == null)
                return null;

            var elementList = new List<GwswElement>();
            var elementTypeFound = AttributesDefinition.FirstOrDefault(at => at.FileName.Equals(Path.GetFileName(path)));
            string elementTypeName = string.Empty;
            if (elementTypeFound != null)
            {
                elementTypeName = elementTypeFound.ElementName;
            }
            foreach (DataRow dataRow in importedDataTable.Rows)
            {
                var element = new GwswElement();
                element.ElementType = stringToSewerTypeConverter[elementTypeName];
                var rowValues = dataRow.ItemArray.ToList();
                foreach (var column in rowValues)
                {
                    var attribute = new GwswAttribute();
                    attribute.ValueAsString = column.ToString();
                    attribute.ValueType = column.GetType();
                    var columnIndex = rowValues.IndexOf(column);
                    var columnName = importedDataTable.Columns[columnIndex].ColumnName;
                    if (AttributesDefinition != null)
                    {
                        var foundAttr = AttributesDefinition.FirstOrDefault(attr => attr.Key.Equals(columnName));
                        if (foundAttr == null)
                        {
                            Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Row__0__column__1__of_file__2__was_not_mapped_correctly_, importedDataTable.Rows.IndexOf(dataRow), columnName, path);
                            continue;
                        }
                        attribute.GwswAttributeType = foundAttr;
                    }
                    
                    element.GwswAttributeList.Add( attribute );
                }
                
                elementList.Add(element);
            }
            return elementList;
        }

        public string Name
        {
            get { return "GWSW File importer"; }
        }

        public string Category
        {
            get { return "1D / 2D"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.StructureFeatureSmall; }
        }
        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IWaterFlowFMModel);
            }
        }
        public bool CanImportOnRootLevel { get { return false;  } }
        public string FileFilter { get { return "GWSW Csv Files (*.csv)|*.csv"; } }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get { return false; } }
        #endregion

        public DataTable ImportFileAsDataTable(string path, object target = null)
        {
            var mappingData = target as CsvMappingData;
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

        public List<GwswElement> ImportFilesFromDefinitionFile(string path)
        {
            ImportDefinitionFile(path);
            Log.InfoFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Attributes_mapped__0_, AttributesDefinition.Count);

            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);

            var uniqueFileList = AttributesDefinition.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
            var importedElements = new List<GwswElement>();

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

                var mapping = CreateCsvMappingDataForFile(fileName);

                var importedElement = ImportItem(elementFilePath, mapping);
                var importedElementTable = importedElement as List<GwswElement>;
                if (importedElementTable == null)
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_File__0__was_not_imported_correctly_, fileName);
                }

                importedElements.AddRange(importedElementTable);
            }

            return importedElements;
        }

        public DataTable ImportDefinitionFile(string path)
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
                        new CsvRequiredField("Opmerking", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
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
            var attributeList = new List<GwswAttributeType>();

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

                var attribute = new GwswAttributeType()
                {
                    Name = attributeName,
                    ElementName = attributeElement,
                    Definition = attributeDefinition,
                    FileName = attributeFile,
                    Key = attributeCodeInt,
                    LocalKey = attributeCode,
                    AttributeType = FMParser.GetClrType(attributeName, attributeType, ref attributeDefinition,
                        attributeFile, importedTable.Rows.IndexOf(row)),
                };

                attributeList.Add(attribute);
            }

            AttributesDefinition = attributeList;

            return importedTable;
        }

        public CsvMappingData CreateCsvMappingDataForFile(string fileName)
        {
            //Import file elements based on their attributes
            var fileAttributes = AttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(fileName))).ToList();
            var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
            //Create column mapping
            fileAttributes.ForEach(
                attr =>
                    fileColumnMapping.Add(
                        new CsvRequiredField(attr.Key, attr.AttributeType),
                        new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

            var mapping = new CsvMappingData()
            {
                Settings = CsvSettingsSemiColonDelimeted,
                FieldToColumnMapping = fileColumnMapping
            };
            return mapping;
        }
    }

    public class GwswElement
    {
        public List<GwswAttribute> GwswAttributeList { get; set; }
        public SewerFeatureType ElementType { get; set; }

        public GwswElement()
        {
            GwswAttributeList = new List<GwswAttribute>();
        }
    }

    public class GwswAttribute
    {
        public GwswAttributeType GwswAttributeType { get; set; }
        public string ValueAsString { get; set; } 
        public Type ValueType { get; set; }
    }

    public class GwswAttributeType
    {
        public string Name { get; set; }

        private string _elementName;
        public string ElementName
        {
            get
            {
                if (_elementName == null)
                {
                    return _elementName;
                }
                return Path.GetFileNameWithoutExtension(_elementName); /*The element names might contain extensions*/
            }
            set { _elementName = value; }
        }

        public string Key { get; set; }
        public string LocalKey { get; set; }
        public string Definition { get; set; }
        public string Mandatory { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public Type AttributeType { get; set; }

        public GwswAttributeType(){}

        public GwswAttributeType(string fileName, int lineNumber, string columnName, string typeFiled, string codeName, string definition, string mandatory, string remarks)
        {
            Name = columnName;
            Key = codeName;
            Definition = definition;
            Mandatory = mandatory;
            Remarks = remarks;
            FileName = fileName;
            AttributeType = FMParser.GetClrType(Name, typeFiled, ref definition, fileName, lineNumber);
        }
    }
}
