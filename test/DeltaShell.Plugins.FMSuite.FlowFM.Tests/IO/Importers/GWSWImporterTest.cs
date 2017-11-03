using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GWSWImporterTest
    {
        [Test]
        public void CsvFilesCanBeReadJustProvidingTheFileColumnInfo()
        {
            //check csv file exists

            //create column info
            //check column info is valid

            //import csv file

            //check csv is correct

        }

        private char CsvDelimter = ';';

        [Test]
        public void ReadDebietCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Debiet.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        public void ReadKnooppuntCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                //UNI_IDE	RST_IDE	PUT_IDE	KNP_XCO	KNP_YCO	CMP_IDE	MVD_NIV	MVD_SCH	WOS_OPP	KNP_MAT	KNP_VRM	KNP_BOK	KNP_BRE	KNP_LEN	KNP_TYP	INZ_TYP	INI_NIV	STA_OBJ	AAN_MVD	ITO_IDE	ALG_TOE
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        public void ReadKunstwerkCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                //UNI_IDE	KWK_TYP	BWS_NIV	PRO_BOK	DRL_COE	DRL_CAP	OVS_BRE	OVS_NIV	OVS_COE	PMP_CAP	PMP_AN1	PMP_AF1	PMP_AN2	PMP_AF2	QDH_NIV	QDH_DEB	AAN_OVN	AAN_OVB	AAN_CAP	AAN_ANS	AAN_AFS	ALG_TOE
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        public void ReadMetaCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Meta.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                //ALG_ATL	ALG_VRS	ALG_DAT	ALG_OPD	ALG_UIT	ALG_OMS	ALG_EXP	ALG_TOE
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        public void ReadNwrwCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Nwrw.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        public void ReadOppervlakCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Oppervlak.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                //UNI_IDE	NSL_STA	AFV_DEF	AFV_IDE	AFV_OPP	ALG_TOE
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        public void ReadProfielCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                //IDE_PRO PRO_MAT PRO_VRM PRO_BRE PRO_HGT OPL_HL1 OPL_HL2 VNR_REG PRO_NIV PRO_NOP PRO_NOM PRO_BRE AAN_PBR TOE_OBJ
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
                        new CsvRequiredField("PRO_BRE2", typeof (string)),
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        public void ReadVerbindingCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void ReadVerloopCsvFileIntoDataTable()
        {
            var filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verloop.csv");
/*            var csvImporter = new CsvImporter();
            Assert.IsNotNull(csvImporter);

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = CsvDelimter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                //IDE_VER	VER_TYP	VER_DAG	VER_VOL	U00_DAG	U01_DAG	U02_DAG	U03_DAG	U04_DAG	U05_DAG	U06_DAG	U07_DAG	U08_DAG	U09_DAG	U10_DAG	U11_DAG	U12_DAG	U13_DAG	U14_DAG	U15_DAG	U16_DAG	U17_DAG	U18_DAG	U19_DAG	U20_DAG	U21_DAG	U22_DAG	U23_DAG	TOE_OBJ
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

            var importedCsv = csvImporter.ImportCsv(filePath, mappingData);
            Assert.IsNotNull(importedCsv);

            Assert.IsTrue(importedCsv.Rows.Count > 0);*/
        }

        private string GetFileAndCreateLocalCopy(string path)
        {
            var filePath = TestHelper.GetTestFilePath(path);
            Assert.IsTrue(File.Exists(filePath), filePath);

            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath), filePath);

            return filePath;
        }
    }
}