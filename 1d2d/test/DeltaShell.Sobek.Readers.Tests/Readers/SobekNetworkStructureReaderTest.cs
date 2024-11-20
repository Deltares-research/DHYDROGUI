using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekNetworkStructureReaderTest
    {
        [Test]
        public void ReadRecordWithLcInName()
        {
            const string source = @"STRU id '13' nm 'steelcun' ci '1' lc 18270.969411203 stru";
            var structureLocation = SobekNetworkStructureReader.GetStructureLocation(source);
            Assert.AreEqual("13", structureLocation.ID);
            Assert.AreEqual("1", structureLocation.BranchID);
            Assert.AreEqual("steelcun", structureLocation.Name);
            Assert.AreEqual(18270.969411203, structureLocation.Offset, 1.0e-6);
            Assert.IsFalse(structureLocation.IsCompound);
        }


        [Test]
        public void ReadLocations()
        {
            var structureLocationFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"StrTest.lit\1\network.st");
            var structures = new SobekNetworkStructureReader().Read(structureLocationFile);
            // the demo network has 1 of each of the 11 main structure types
            Assert.AreEqual(11, structures.Count());
            foreach (var sobekStructureLocation in structures)
            {
                Assert.IsFalse(sobekStructureLocation.IsCompound);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExtraResistanceLocations()
        {
            var structureLocationFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"TestXRST.lit\4\network.st");
            var structures = new SobekNetworkStructureReader().Read(structureLocationFile);
            Assert.AreEqual(1, structures.Count());
            var sobekStructureLocation = structures.FirstOrDefault();
            Assert.IsFalse(sobekStructureLocation.IsCompound);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCompoundStructureLocations()
        {
            var structureLocationFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"compound.lit\2\network.st");
            var structures = new SobekNetworkStructureReader().Read(structureLocationFile);
            // the demo network has 1 of each of the 11 main structure types
            Assert.AreEqual(3, structures.Count());
            var sobekStructureLocation = structures.FirstOrDefault();
            Assert.IsTrue(sobekStructureLocation.IsCompound);
        }

    }
}
