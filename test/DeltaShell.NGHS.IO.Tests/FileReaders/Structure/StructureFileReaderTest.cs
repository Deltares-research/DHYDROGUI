using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileReaders.Structure;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Structure
{
    [TestFixture]
    public class StructureFileReaderTest
    {
        [Test, Category(TestCategory.DataAccess)]
        public void GivenStructureFileReader_ReadingStructureFile_ShouldResultInAddedStructuresInNetwork()
        {
            //Arrange
            var networkFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\FlowFM_net.nc");
            var crossSectionLocationFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\crsloc.ini");
            var crossSectionDefinitionFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\crsdef.ini");
            var structureFilePath = Path.Combine(TestHelper.GetTestDataDirectory(),@"FileReaders\StructureFileReaderTest\structures.ini");

            IHydroNetwork network = new HydroNetwork();
            IDiscretization discretization = new Discretization();
            UGridFileHelper.ReadNetworkAndDiscretisation(networkFilePath, 
                                                         discretization, 
                                                         network, 
                                                         Enumerable.Empty<CompartmentProperties>(), 
                                                         Enumerable.Empty<BranchProperties>());
            var definitions = CrossSectionFileReader.ReadFile(crossSectionLocationFilePath,crossSectionDefinitionFilePath, network, null);

            // Act
            StructureFileReader.ReadFile(structureFilePath, definitions, network, DateTime.Today);

            // Assert
            Assert.AreEqual(26, network.BranchFeatures.Count());
            Assert.AreEqual(8, network.CompositeBranchStructures.Count());
            Assert.AreEqual(1, network.Bridges.Count());
            Assert.AreEqual(2, network.Culverts.Count());
            Assert.AreEqual(6, network.Weirs.Count());
            Assert.AreEqual(1, network.Pumps.Count());
        }
    }
}
