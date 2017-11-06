using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GwswFileImporterTest
    {
        private static char csvDelimeter = ';';

        [Test]
        public void CsvFilesCanBeReadJustProvidingTheFileColumnInfo()
        {
            //check csv file exists

            //create column info
            //check column info is valid

            //import csv file

            //check csv is correct

        }

        [Test]
        public void ReadGwswDefinitionFile()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
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

            CheckCsvIsImportedCorrectly(filePath, mappingData);
        }

        [Test]
        public void CreateGwswDictionaryFromDefinitionFile()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie.csv");
            // Import Csv Definition.
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);
            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
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

            var importedTable = CheckGwswFileImporterWorksCorrectly(filePath, mappingData);

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

            Assert.IsTrue(attributeList.Count > 0, string.Format("Attributes found {0}", attributeList.Count));

            var uniqueFileList = attributeList.GroupBy( i => i.FileName).Select( grp => grp.Key).ToList();
            Assert.AreEqual(uniqueFileList.Count, 12, "Mismatch on found filenames.");

            var csvSettings = new CsvSettings
            {
                Delimiter = csvDelimeter,
                FirstRowIsHeader = true,
                SkipEmptyLines = true
            };

            var importedTables = new List<DataTable>();

            //Read each one of the files.
            foreach (var fileName in uniqueFileList)
            {
                var directoryName = Path.GetDirectoryName(filePath);
                var elementFilePath = Path.Combine(directoryName, fileName);
                Assert.IsTrue(File.Exists(elementFilePath), string.Format("Could not find file {0}", elementFilePath));

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

                var importedElementTable = CheckGwswFileImporterWorksCorrectly(elementFilePath, mapping);
                importedTables.Add(importedElementTable);
            }
        }

        [Test]
        public void ImportGwswFilesFromDefinitionFile()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie.csv");
            var gwswImporter = new GwswFileImporterBase();
            Assert.IsNotNull(gwswImporter);

            var dataTables = gwswImporter.ImportFromDefinitionFile(filePath);
            Assert.IsNotNull(dataTables);

            Assert.IsTrue(dataTables.Count > 0);
        }


        [Test]
        public void ImportCsvDebietFileUsingGwswFileImporter()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Debiet.csv");
            
            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("UNI_IDE", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("DEB_TYP", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VER_IDE", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AVV_ENH", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_OPP", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckGwswFileImporterWorksCorrectly(filePath, mappingData);
        }

        #region Proof of concept - Import Csv

        [Test]
        public void ImportCsvDebietFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Debiet.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("UNI_IDE", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("DEB_TYP", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VER_IDE", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AVV_ENH", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_OPP", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly(filePath, mappingData);
        }

        [Test]
        public void ReadKnooppuntCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("UNI_IDE", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("RST_IDE", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PUT_IDE", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_XCO", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_YCO", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("CMP_IDE", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("MVD_NIV", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("MVD_SCH", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("WOS_OPP", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_MAT", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_VRM", typeof (string)),
                        new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_BOK", typeof (string)),
                        new CsvColumnInfo(11, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_BRE", typeof (string)),
                        new CsvColumnInfo(12, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_LEN", typeof (string)),
                        new CsvColumnInfo(13, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KNP_TYP", typeof (string)),
                        new CsvColumnInfo(14, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("INZ_TYP", typeof (string)),
                        new CsvColumnInfo(15, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("INI_NIV", typeof (string)),
                        new CsvColumnInfo(16, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("STA_OBJ", typeof (string)),
                        new CsvColumnInfo(17, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_MVD", typeof (string)),
                        new CsvColumnInfo(18, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ITO_IDE", typeof (string)),
                        new CsvColumnInfo(19, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(20, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        [Test]
        public void ReadKunstwerkCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("UNI_IDE", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KWK_TYP", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("BWS_NIV", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_BOK", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("DRL_COE", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("DRL_CAP", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("OVS_BRE", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("OVS_NIV", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("OVS_COE", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PMP_CAP", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PMP_AN1", typeof (string)),
                        new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PMP_AF1", typeof (string)),
                        new CsvColumnInfo(11, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PMP_AN2", typeof (string)),
                        new CsvColumnInfo(12, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PMP_AF2", typeof (string)),
                        new CsvColumnInfo(13, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("QDH_NIV", typeof (string)),
                        new CsvColumnInfo(14, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("QDH_DEB", typeof (string)),
                        new CsvColumnInfo(15, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_OVN", typeof (string)),
                        new CsvColumnInfo(16, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_OVB", typeof (string)),
                        new CsvColumnInfo(17, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_CAP", typeof (string)),
                        new CsvColumnInfo(18, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_ANS", typeof (string)),
                        new CsvColumnInfo(19, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_AFS", typeof (string)),
                        new CsvColumnInfo(20, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(21, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        [Test]
        public void ReadMetaCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Meta.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("ALG_ATL", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_VRS", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_DAT", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_OPD", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_UIT", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_OMS", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_EXP", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        [Test]
        public void ReadNwrwCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Nwrw.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                //IDE_AFW	AFV_BRG	AFV_IFX 	AFV_IFN 	AFV_IFA 	AFV_IFH 	AFV_AFS 	AFV_LEN 	AFV_HEL 	AFV_RUW	TOE_OBJ
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("IDE_AFW", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_BRG", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_IFX", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_IFN", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_IFA", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_IFH", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_AFS", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_LEN", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_HEL", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_RUW", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("TOE_OBJ", typeof (string)),
                        new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        [Test]
        public void ReadOppervlakCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Oppervlak.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("UNI_IDE", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("NSL_STA", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_DEF", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_IDE", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AFV_OPP", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void ReadProfielCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");


            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("IDE_PRO", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("DEB_TYP", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_MAT", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_VRM", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_BRE", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_HGT", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("OPL_HL1", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("OPL_HL2", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VNR_REG", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_NIV", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_NOP", typeof (string)),
                        new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_NOM", typeof (string)),
                        new CsvColumnInfo(11, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_BRE", typeof (string)),
                        new CsvColumnInfo(12, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_PBR", typeof (string)),
                        new CsvColumnInfo(13, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("TOE_OBJ", typeof (string)),
                        new CsvColumnInfo(14, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        [Test]
        public void ReadVerbindingCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("UNI_IDE", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KN1_IDE", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("KN2_IDE", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VRB_TYP", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("LEI_IDE", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("BOB_KN1", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("BOB_KN2", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("STR_RCH", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VRB_LEN", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("INZ_TYP", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("INV_KN1", typeof (string)),
                        new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("UTV_KN1", typeof (string)),
                        new CsvColumnInfo(11, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("INV_KN2", typeof (string)),
                        new CsvColumnInfo(12, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("UTV_KN2", typeof (string)),
                        new CsvColumnInfo(13, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ITO_IDE", typeof (string)),
                        new CsvColumnInfo(14, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("PRO_IDE", typeof (string)),
                        new CsvColumnInfo(15, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("STA_OBI", typeof (string)),
                        new CsvColumnInfo(16, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_BB1", typeof (string)),
                        new CsvColumnInfo(17, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("AAN_BB2", typeof (string)),
                        new CsvColumnInfo(18, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("INI_NIV", typeof (string)),
                        new CsvColumnInfo(19, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("ALG_TOE", typeof (string)),
                        new CsvColumnInfo(20, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        [Test]
        public void ReadVerloopCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verloop.csv");
           
            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
																					
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {
                        new CsvRequiredField("IDE_VER", typeof (string)),
                        new CsvColumnInfo(0, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VER_TYP", typeof (string)),
                        new CsvColumnInfo(1, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VER_DAG", typeof (string)),
                        new CsvColumnInfo(2, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("VER_VOL", typeof (string)),
                        new CsvColumnInfo(3, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U00_DAG", typeof (string)),
                        new CsvColumnInfo(4, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U01_DAG", typeof (string)),
                        new CsvColumnInfo(5, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U02_DAG", typeof (string)),
                        new CsvColumnInfo(6, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U03_DAG", typeof (string)),
                        new CsvColumnInfo(7, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U04_DAG", typeof (string)),
                        new CsvColumnInfo(8, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U05_DAG", typeof (string)),
                        new CsvColumnInfo(9, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U06_DAG", typeof (string)),
                        new CsvColumnInfo(10, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U07_DAG", typeof (string)),
                        new CsvColumnInfo(11, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U08_DAG", typeof (string)),
                        new CsvColumnInfo(12, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U09_DAG", typeof (string)),
                        new CsvColumnInfo(13, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U10_DAG", typeof (string)),
                        new CsvColumnInfo(14, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U11_DAG", typeof (string)),
                        new CsvColumnInfo(15, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U12_DAG", typeof (string)),
                        new CsvColumnInfo(16, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U13_DAG", typeof (string)),
                        new CsvColumnInfo(17, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U14_DAG", typeof (string)),
                        new CsvColumnInfo(18, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U15_DAG", typeof (string)),
                        new CsvColumnInfo(19, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U16_DAG", typeof (string)),
                        new CsvColumnInfo(20, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U17_DAG", typeof (string)),
                        new CsvColumnInfo(21, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U18_DAG", typeof (string)),
                        new CsvColumnInfo(22, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U19_DAG", typeof (string)),
                        new CsvColumnInfo(23, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U20_DAG", typeof (string)),
                        new CsvColumnInfo(24, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U21_DAG", typeof (string)),
                        new CsvColumnInfo(25, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U22_DAG", typeof (string)),
                        new CsvColumnInfo(26, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("U23_DAG", typeof (string)),
                        new CsvColumnInfo(27, CultureInfo.InvariantCulture)
                    },
                    {
                        new CsvRequiredField("TOE_OBJ", typeof (string)),
                        new CsvColumnInfo(28, CultureInfo.InvariantCulture)
                    },
                }
            };

            CheckCsvIsImportedCorrectly( filePath, mappingData);
        }

        #endregion

        #region Helpers

        private static DataTable CheckGwswFileImporterWorksCorrectly(string filePath, CsvMappingData mappingData)
        {
            var importer = new GwswFileImporterBase();
            Assert.IsNotNull(importer);

            var importedObject = importer.ImportItem(filePath, mappingData);
            var importedTable = importedObject as DataTable;
            Assert.IsNotNull(importedTable, string.Format("The .csv file {0}, could not be imported.", filePath));

            var fileAsStringList = File.ReadAllLines(filePath);
            var numberOfLines = fileAsStringList.Length - 1; // we should not include the header
            var rowsCount = importedTable.Rows.Count;
            if (rowsCount != numberOfLines)
            {
                //Check there are no repeated columns in the .CSV
                var repeatedElements = fileAsStringList[0].Split(csvDelimeter).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key)
                    .ToList();
                
                /* If there were repeated columns, that table will simply not be imported, and the user will receive a log message saying so. 
                  as for this test, we are interested onto continuing, so we can ignore when a column is repeated. Its corresponding test will fail. */
                if (repeatedElements.Count == 0)
                {
                    Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
                }
            }

            return importedTable;
        }


        private static void CheckCsvIsImportedCorrectly(string filePath, CsvMappingData mappingData)
        {
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            try
            {
                var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
                Assert.IsNotNull(importedCsv, string.Format("The .csv file {0}, could not be imported.", filePath));

                var numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
                var rowsCount = importedCsv.Rows.Count;
                Assert.AreEqual(numberOfLines, rowsCount, string.Format("The imported .csv file {0}, contains {1} rows but {2} were imported.", filePath, numberOfLines, rowsCount));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private string GetFileAndCreateLocalCopy(string path)
        {
            var filePath = TestHelper.GetTestFilePath(path);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath), string.Format("File {0} could not be located", filePath));

            return filePath;
        }

        #endregion
    }
}