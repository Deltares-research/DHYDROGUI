using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Shell.Core;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.Common.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    class GwswFileImporterBase : IFileImporter
    {
        private char CsvDelimeter = ';';
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImporterBase));

        #region IFileImporter
        public bool CanImportOn(object targetObject)
        {
            throw new NotImplementedException();
        }

        public object ImportItem(string path, object target = null)
        {
            var mappingData = target as CsvMappingData;
            if (mappingData == null)
            {
                Log.ErrorFormat("No mapping was found to import File {0}.", path);
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
                Log.ErrorFormat("Could not import file {0}. Reason: {1}", path, e.Message);
            }

            return importedCsv;
        }

        public string Name { get; }
        public string Category { get; }
        public Bitmap Image { get; }
        public IEnumerable<Type> SupportedItemTypes { get; }
        public bool CanImportOnRootLevel { get; }
        public string FileFilter { get; }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; }
        #endregion

        public List<DataTable> ImportFromDefinitionFile(string path)
        {
            // Import definition file with predefined CSV columns.
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
                        new CsvRequiredField("Nr", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Bestandsnaam", typeof (string)),
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
                        new CsvRequiredField("Definitie", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Eenheid", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Verplicht", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("Opmerking", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                }
            };

            var importedObject = ImportItem(path, mappingData);
            var importedTable = importedObject as DataTable;
            if (importedTable == null)
            {
                Log.ErrorFormat("Not possible to import {0}", path);
                return null;
            }

            //Load the related tables referred in the definition file.
            var attributeList = new List<GwswsAttribute>();

            // Create new attributes for each occurrence.
            // Retreive the files that need to be read.
            foreach (DataRow row in importedTable.Rows)
            {
                var attributeFile = row.ItemArray[0].ToString();
                var attributeName = row.ItemArray[1].ToString();
                var attributeCode = row.ItemArray[2].ToString();
                var attributeDefinition = row.ItemArray[3].ToString();
                var attributeType = row.ItemArray[4].ToString();

                var attribute = new GwswsAttribute()
                {
                    Name = attributeName,
                    Definition = attributeDefinition,
                    FileName = attributeFile,
                    Key = attributeCode,
                    AttributeType = FMParser.GetClrType(attributeName, attributeType, ref attributeDefinition,
                        attributeFile, importedTable.Rows.IndexOf(row)),
                };

                attributeList.Add(attribute);
            }
            Log.InfoFormat("Attributes imported {0}", attributeList.Count);

            Log.Info("Importing sub files.");

            var uniqueFileList = attributeList.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
            var csvSettings = new CsvSettings
            {
                Delimiter = CsvDelimeter,
                FirstRowIsHeader = true,
                SkipEmptyLines = true
            };

            var importedTables = new List<DataTable>();

            //Read each one of the files.
            foreach (var fileName in uniqueFileList)
            {
                var directoryName = Path.GetDirectoryName(path);
                var elementFilePath = Path.Combine(directoryName, fileName);
                if (!File.Exists(elementFilePath))
                {
                    Log.ErrorFormat("Could not find file {0}.", elementFilePath);
                    continue;
                }

                //Import file elements based on their attributes
                var fileAttributes = attributeList.Where(at => at.FileName.Equals(fileName)).ToList();
                var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
                //Create column mapping
                fileAttributes.ForEach(
                    attr =>
                        fileColumnMapping.Add(
                            new CsvRequiredField(attr.Key, attr.AttributeType),
                            new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

                var mapping = new CsvMappingData()
                {
                    Settings = csvSettings,
                    FieldToColumnMapping = fileColumnMapping
                };

                var importedElement = ImportItem(elementFilePath, mapping);
                var importedElementTable = importedElement as DataTable;
                if (importedElementTable == null)
                {
                    Log.ErrorFormat("File {0} was not imported correctly.", fileName);
                }
                importedTables.Add(importedElementTable);
            }

            return importedTables;
        }

    }

    public class GwswsAttribute
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Definition { get; set; }
        public string Mandatory { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public Type AttributeType { get; set; }

        public GwswsAttribute(){}

        public GwswsAttribute(string fileName, int lineNumber, string columnName, string typeFiled, string codeName, string definition, string mandatory, string remarks)
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
