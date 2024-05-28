using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekStructureDatFileReaderTest
    {
        [Test]
        public void ReadFromString()
        {
            string fileText =  @"STRU id 'C_2##S_13' nm 'hvl_1' dd '0' ca 1 1 0 0 cj '22128_1' '18044_1' '-1' '-1'  cm 1 mo 'C_2' stru" + Environment.NewLine +
                               @"STRU id 'C_2##S_17125' nm 'hvl_2' dd '16223' ca 1 1 0 0 cj '22130_1' '18044_2' '-1' '-1'  cm 1 mo 'C_2' stru" + Environment.NewLine +
                               @"STRU id 'C_2##S_17127' nm 'hvl_3' dd '16223' ca 1 1 0 0 cj '22132_1' '18044_2' '-1' '-1'  cm 1 mo 'C_2' stru";

            var structureMappings = new SobekStructureDatFileReader().Parse(fileText).ToArray();

            Assert.AreEqual(3, structureMappings.Length);
        }

        [Test]
        public void ReadFromStringWithShortPattern()
        {
            string fileText =
                @"STRU id '6' nm '' dd '6' stru" + Environment.NewLine +
                @"STRU id '7' nm '' dd '7' stru";

            var structureMappings = new SobekStructureDatFileReader().Parse(fileText).ToArray();

            Assert.AreEqual(2, structureMappings.Length);
        }

        [Test]
        public void ReadFromStringWithNoNamePattern()
        {
            string fileText =
                @"STRU id 'AL2_88' dd 'AL2_84' ca 1 1 0 0 cj 'AL2_76' 'AL2_77' '-1' '-1' cm 0 stru" + Environment.NewLine +
                @"STRU id 'AL2_86' dd 'AL2_82' ca 1 1 1 0 cj 'AL2_73' 'AL2_74' 'AL2_75' '-1' cm 1 stru" + Environment.NewLine +
                @"STRU id 'AL2_87' dd 'AL2_83' ca 0 0 0 0 cj '-1' '-1' '-1' '-1' cm 1 stru";

            var structureMappings = new SobekStructureDatFileReader().Parse(fileText).ToArray();

            Assert.AreEqual(3, structureMappings.Length);
            Assert.IsNotNull(structureMappings[0].ControllerIDs);
            Assert.AreEqual(2, structureMappings[0].ControllerIDs.Count);
        }

        [Test]
        public void ReadFromStringWithControllerIDs()
        {
            string fileText = @"STRU id 'C_2##S_13' nm 'hvl_1' dd '0' ca 1 1 0 0 cj '22128_1' '18044_1' '-1' '-1'  cm 1 mo 'C_2' stru" + Environment.NewLine +
                               @"STRU id 'C_2##S_17125' nm 'hvl_2' dd '16223' ca 1 1 0 0 cj '22130_1' '18044_2' '-1' '-1'  cm 1 mo 'C_2' stru" + Environment.NewLine +
                               @"STRU id 'C_2##S_17127' nm 'hvl_3' dd '16223' ca 1 1 0 0 cj '22132_1' '18044_2' '-1' '-1'  cm 1 mo 'C_2' stru";

            var structureMappings = new SobekStructureDatFileReader().Parse(fileText).ToArray();

            Assert.AreEqual(3, structureMappings.Length);
            Assert.IsNotNull(structureMappings[0].ControllerIDs);
            Assert.AreEqual(2, structureMappings[0].ControllerIDs.Count);
            Assert.AreEqual("CTR_22128_1",structureMappings[0].ControllerIDs[0]);
            Assert.AreEqual("CTR_18044_1", structureMappings[0].ControllerIDs[1]);
        }

        [Test]
        public void ReadFromStringWithControllerIDsWithOtherPattern()
        {
            string fileText = @"STRU id 'C_2##S_13' nm 'hvl_1' dd '0' ca 1 cj '3' '3' '3' stru";

            var structureMappings = new SobekStructureDatFileReader().Parse(fileText).ToArray();

            Assert.AreEqual(1, structureMappings.Length);
            Assert.IsNotNull(structureMappings[0].ControllerIDs);
            Assert.AreEqual(1, structureMappings[0].ControllerIDs.Count);
        }

        [Test]
        public void ReadFromStringWithJustTwoActiveControllerIDs()
        {
            string fileText =
                @"STRU id '5574716' dd '5573285' ca 1 1 0 0 cj '5574729' '5573291' '5576958' '-1' cm 1 stru";

            var structureMapping = new SobekStructureDatFileReader().Parse(fileText).First();

            Assert.AreEqual(2, structureMapping.ControllerIDs.Count); //2 controllers are active (ca 1 1 0 0)
        }

        [Test]
        public void ReadFrom212File()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ndb_controllertriggerfiles\STRUCT.DAT");
            var structureMappings = new SobekStructureDatFileReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(41, structureMappings.Length);
            Assert.IsNotNull(structureMappings[39].ControllerIDs);
            Assert.AreEqual(3, structureMappings[39].ControllerIDs.Count);
        }

        [Test]
        public void ReadFromREFile()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ndb_controllertriggerfiles\DEFSTR.2");
            var structureMappings = new SobekStructureDatFileReader().Read(pathToSobekNetwork).ToArray();

            Assert.AreEqual(41, structureMappings.Length);
            Assert.IsNotNull(structureMappings[39].ControllerIDs);
            Assert.AreEqual(2, structureMappings[39].ControllerIDs.Count); //first two are active
        }

        [Test]
        //TestCase 175 of testbench
        public void ReadStructureLineWithoutMo()
        {
            string fileText = @"STRU id '5' nm '' dd '##3' ca 1 0 0 0 cj '##1' '-1' '-1' '-1' cm 0 stru" +
                              Environment.NewLine +
                              @"STRU id '5##1' nm 'General structure member' dd '##3' ca 1 0 0 0 cj '##1' '-1' '-1' '-1' cm 1 mo '5' stru";

            var structureMappings = new SobekStructureDatFileReader().Parse(fileText);

            Assert.AreEqual(2, structureMappings.Count());

            var structure = structureMappings.First();

            Assert.AreEqual("5", structure.StructureId);
            Assert.AreEqual("##3", structure.DefinitionId);
            Assert.AreEqual("CTR_##1", structure.ControllerIDs[0]);
        }
    }
}
