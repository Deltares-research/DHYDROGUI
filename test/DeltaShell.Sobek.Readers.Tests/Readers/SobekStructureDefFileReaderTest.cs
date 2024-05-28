using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekStructureDefFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSobekWeirs()
        {
            //def file contains 6 different weirs
            string structureDefinitionFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Weir\struct.def");
            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            //regular sobek weir
            IEnumerable<SobekStructureDefinition> definitions = reader.Read(structureDefinitionFile);
            
            SobekWeir weir = (SobekWeir) definitions.Where(def=>def.Definition is SobekWeir).Select(def=>def.Definition).FirstOrDefault();
                
            Assert.AreEqual(0.7f,weir.CrestLevel);

            //river weir
            SobekRiverWeir riverWeir = (SobekRiverWeir) definitions.Where(def=>def.Definition is SobekRiverWeir).Select(def=>def.Definition).FirstOrDefault();
            Assert.AreEqual(7, riverWeir.NegativeReductionTable.Rows.Count);

            //advanced river weir (WeirWithPiers)
            SobekRiverAdvancedWeir advancedRiverWeir = (SobekRiverAdvancedWeir) definitions.Where(def=>def.Definition is SobekRiverAdvancedWeir).Select(def=>def.Definition).FirstOrDefault();
            Assert.AreEqual(2, advancedRiverWeir.NumberOfPiers);

            //universal weir
            SobekUniversalWeir universalWeir = (SobekUniversalWeir) definitions.Where(def=>def.Definition is SobekUniversalWeir).Select(def=>def.Definition).FirstOrDefault();
            Assert.AreEqual("1", universalWeir.CrossSectionId);

            //orifice
            SobekOrifice orifice = (SobekOrifice) definitions.Where(def=>def.Definition is SobekOrifice).Select(def=>def.Definition).FirstOrDefault();
            Assert.AreEqual(0.9f,orifice.GateHeight);

            //general structure
            SobekGeneralStructure generalStructure =
                (SobekGeneralStructure)
                definitions.Where(def => def.Definition is SobekGeneralStructure).Select(def => def.Definition).FirstOrDefault();
            Assert.AreEqual(5.0f, generalStructure.GateHeight);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSobekPumps()
        {
            //def file contains 6 different weirs
            string structureDefinitionFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Pump\TLS-1610litSTRUCT.DEF");
            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            //regular sobek weir
            IEnumerable<SobekStructureDefinition> definitions = reader.Read(structureDefinitionFile);
            Assert.AreEqual(1, definitions.Where(def => def.Type == 3).Count());
            Assert.AreEqual(7, definitions.Where(def => def.Type == 9).Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithStructure()
        {
            string structureDefinitionFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"NetworkWithStructures\struct.def");
            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            //75 structures
            IList<SobekStructureDefinition> definitions =
                reader.Read(structureDefinitionFile).ToList();
            Assert.AreEqual(68, definitions.Count);

            Assert.AreEqual(9, definitions.Count(d => d.Definition is SobekCulvert));
            Assert.AreEqual(5, definitions.Count(d => d.Definition is SobekWeir));
            Assert.AreEqual(8, definitions.Count(d => d.Definition is SobekRiverWeir));
            Assert.AreEqual(2, definitions.Count(d => d.Definition is SobekUniversalWeir));
            Assert.AreEqual(30, definitions.Count(d => d.Definition is SobekGeneralStructure));
            Assert.AreEqual(1, definitions.Count(d => d.Definition is SobekRiverAdvancedWeir));
            Assert.AreEqual(4, definitions.Count(d => d.Definition is SobekOrifice));
            // pump type 9 and 5 pumps type 3 (River pump)
            Assert.AreEqual(1 + 5, definitions.Count(d => d.Definition is SobekPump));
            Assert.AreEqual(3, definitions.Count(d => d.Definition is SobekBridge));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        public void ReadAllDefFilesShouldNeverCrash()
        {
            var reader = new SobekStructureDefFileReader(SobekType.Unknown);
            string dataDir = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly);
            string[] defFiles = Directory.GetFiles(dataDir, "*.def", SearchOption.AllDirectories);
            //string[] defFiles = Directory.GetFiles(dataDir, "struct.def", SearchOption.AllDirectories);
            foreach (string defFile in defFiles)
            {
                //read in the file.
                List<SobekStructureDefinition> definitions = reader.Read(defFile).ToList();
                //make sure all definitions have a definition and id
                Assert.IsTrue(definitions.All(d=>d.Definition != null));
                Assert.IsTrue(definitions.All(d => d.Id != ""));
                Debug.WriteLine(string.Format("read file {0},{1} structures",defFile, definitions.Count));
            }
        }

        [Test]
        public void SplitDefinitionsWithNonStandardAscii()
        {
            var reader = new SobekStructureDefFileReader(SobekType.Unknown);

            string defFileText = @"STDS id 'S_DoraBaltea_7' nm 'c.Cavour' ty 11 cl 0 si 'strS_DoraBaltea_7' ce 1 sv 0.667 rt 0 stds" + 
                Environment.NewLine + 
                "STDS id 'S_DoraBaltea_8' nm 'Mazzè' ty 11 cl 0 si 'strS_DoraBaltea_8' ce 1 sv 0.667 rt 0 stds";

            var definitions = reader.Parse(defFileText).ToArray();

            Assert.AreEqual(2, definitions.Length);
        }

        [Test]
        public void JakartDefinitionLineMissingName()
        {
            var reader = new SobekStructureDefFileReader(SobekType.Unknown);

            string defFileText = @"STDS id 'EBC_Gate_III' ty 7 cl 0.7 cw 2 gh 0.7 mu 0.63 sc 1 rt 3 mp 0 0 mn 0 0    stds";

            var definitions = reader.Parse(defFileText).ToArray();

            Assert.AreEqual(1, definitions.Length);
        }

        [Test]
        public void JakartaDefinitions()
        {
            string structureDefinitionFile = TestHelper.GetTestFilePath("Jakarta_struct.def");
            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            IList<SobekStructureDefinition> definitions =
                reader.Read(structureDefinitionFile).ToList();

            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "EBC_Gate_III"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "EBC_Gate_II"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "EBC_Wier2_2"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "EBC_Wier2_1"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "EBC_Wier2_5"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "EBC_Wier2_3"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "Marina_4"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "Marina_5"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "Marina_2"));
            Assert.IsNotNull(definitions.FirstOrDefault(d => d.Id == "Marina_3"));

        }

        [Test]
        public void ReadLine_WithExtraSpaces()
        {
            var line = "STDS id '10-BBB6c1-10-BBB6c2' nm '10-BBB6c1-10-BBB6c2' ty 6 cl  2.05 cw  8 ce  .8 sc 1 rt  0               stds";

            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            var definitions = reader.Parse(line);

            Assert.AreEqual(1, definitions.Count());
        }

    }
}
