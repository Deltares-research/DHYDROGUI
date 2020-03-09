using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class StructureFileReaderTest
    {
        [Test, Category(TestCategory.DataAccess)]
        [Category("Quarantine")]
        public void GivenStructureFileReader_ReadingStructureFile_ShouldResultInAddedStructuresInNetwork()
        {
            //Arrange
            var networkFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\FlowFM_net.nc");
            var crossSectionDefinitionFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\crsdef.ini");
            var structureFilePath = Path.Combine(TestHelper.GetTestDataDirectory(),@"FileReaders\StructureFileReaderTest\structures.ini");

            IHydroNetwork network = new HydroNetwork();
            IDiscretization discretization = new Discretization();
            UGridToNetworkAdapter.LoadNetworkAndDiscretisation(networkFilePath, discretization, network, null, null);
            
            // Act
            Assert.AreEqual(0, network.BranchFeatures.Count());

            StructureFileReader.ReadFile(structureFilePath, crossSectionDefinitionFilePath, network);

            // Assert
            Assert.AreEqual(9, network.BranchFeatures.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());
            Assert.AreEqual(1, network.Bridges.Count());
            Assert.AreEqual(2, network.Culverts.Count());
            Assert.AreEqual(5, network.Weirs.Count());
            Assert.AreEqual(0, network.ExtraResistances.Count());
            Assert.AreEqual(0, network.Pumps.Count());
        }
    }
}