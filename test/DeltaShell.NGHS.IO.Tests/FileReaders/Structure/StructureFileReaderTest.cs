using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileReaders.Structure;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Structure
{
    [TestFixture]
    public class StructureFileReaderTest
    {
        private MockFileSystem fileSystem;
        private StructureFileReader fileReader;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            fileReader = new StructureFileReader(fileSystem);
        }

        [Test]
        public void Constructor_FileSystemIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new StructureFileReader(null));
        }
        
        [Test, Category(TestCategory.DataAccess)]
        public void GivenStructureFileReader_ReadingStructureFile_ShouldResultInAddedStructuresInNetwork()
        {
            //Arrange
            var networkFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\FlowFM_net.nc");
            var crossSectionLocationFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\crsloc.ini");
            var crossSectionDefinitionFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), @"FileReaders\StructureFileReaderTest\crsdef.ini");
            var structureFilePath = Path.Combine(TestHelper.GetTestDataDirectory(),@"FileReaders\StructureFileReaderTest\structures.ini");
            
            fileSystem.AddFile(structureFilePath, new MockFileData(File.ReadAllText(structureFilePath)));

            IHydroNetwork network = new HydroNetwork();
            IDiscretization discretization = new Discretization();
            IConvertedUgridFileObjects convertedUGridFileObjects = new ConvertedUgridFileObjects()
            {
                Discretization = discretization,
                HydroNetwork = network
            };

            UGridFileHelper.ReadNetFileDataIntoModel(networkFilePath, convertedUGridFileObjects);
                                                         
            var definitions = CrossSectionFileReader.ReadFile(crossSectionLocationFilePath, crossSectionDefinitionFilePath, network, null);

            // Act
            fileReader.ReadFile(structureFilePath, definitions, network, DateTime.Today);

            // Assert
            Assert.AreEqual(26, network.BranchFeatures.Count());
            Assert.AreEqual(8, network.CompositeBranchStructures.Count());
            Assert.AreEqual(1, network.Bridges.Count());
            Assert.AreEqual(2, network.Culverts.Count());
            Assert.AreEqual(6, network.Weirs.Count());
            Assert.AreEqual(1, network.Pumps.Count());
        }
        
        [TestCase(StructureRegion.StructureTypeName.Weir, SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName, -10.0d, -10.0d)]
        [TestCase(StructureRegion.StructureTypeName.Orifice, SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName, -10.0d, -10.0d)]
        [TestCase(StructureRegion.StructureTypeName.Pump, SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName, 0.0d, 0.0d)]
        public void GivenStructureFileReader_ReadingStructureFileWithSewerConnectionsWithoutCrsDefs_ShouldResultInAddedStructuresWithDefaultSewerConnectionProfilesInNetwork(string typeString, string expectedSewerStructureProfileName, double expectedLevelSource, double expectedLevelTarget)
        {
            //Arrange
            const string path = "structures.ini";
            string ini = GenerateStructureFileData(typeString);

            fileSystem.AddFile(path, new MockFileData(ini));

            var network = Substitute.For<IHydroNetwork>();
            var sewerConnection = new SewerConnection("Connection") { Network = network };
                
            IEventedList<IBranch> branches = new EventedList<IBranch>(Enumerable.Repeat(sewerConnection, 1));
                
            network.Branches.Returns(branches);
            network.SewerConnections.Returns(Enumerable.Repeat(sewerConnection, 1));

            ICrossSectionDefinition[] crsDefs = Enumerable.Empty<ICrossSectionDefinition>().ToArray();

            // Act
            fileReader.ReadFile(path, crsDefs, network, DateTime.Today);

            // Assert
            Assert.That(sewerConnection.CrossSection, Is.Not.Null);
            Assert.That(sewerConnection.CrossSectionDefinitionName, Is.EqualTo(expectedSewerStructureProfileName));
            Assert.That(sewerConnection.LevelSource, Is.EqualTo(expectedLevelSource));
            Assert.That(sewerConnection.LevelTarget, Is.EqualTo(expectedLevelTarget));
        }

        [Test]
        public void GivenStructureFileReader_ReadingStructureFileWithoutCrsDefs_ShouldResultInPipeWithDefaultPipeProfileInNetwork()
        {
            //Arrange
            const string path = "structures.ini";
            string ini = GenerateStructureFileData();

            fileSystem.AddFile(path, new MockFileData(ini));

            var network = Substitute.For<IHydroNetwork>();
            var pipe = new Pipe { PipeId = "Connection", Network = network };

            IEventedList<IBranch> branches = new EventedList<IBranch>(Enumerable.Repeat(pipe,1));
            network.Branches.Returns(branches);
            network.SewerConnections.Returns(Enumerable.Repeat(pipe, 1));
                
            ICrossSectionDefinition[] crsDefs = Enumerable.Empty<ICrossSectionDefinition>().ToArray();

            // Act
            fileReader.ReadFile(path, crsDefs, network, DateTime.Today);

            // Assert
            Assert.That(pipe.CrossSection, Is.Not.Null);
            Assert.That(pipe.CrossSectionDefinitionName, Is.EqualTo(SewerCrossSectionDefinitionFactory.DefaultPipeProfileName));
            Assert.That(pipe.LevelSource, Is.EqualTo(-10.0d).Within(0.001));
            Assert.That(pipe.LevelTarget, Is.EqualTo(-10.0d).Within(0.001));
        }

        private static string GenerateStructureFileData(string typeString = StructureRegion.StructureTypeName.Weir)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"[{StructureRegion.Header}]");
            sb.AppendLine($"\t{StructureRegion.Id.Key} = StructureId");
            sb.AppendLine($"\t{StructureRegion.BranchId.Key} = Connection");
            sb.AppendLine($"\t{StructureRegion.DefinitionType.Key} = {typeString}");
            sb.AppendLine($"\t{StructureRegion.Direction.Key} = suctionSide");
            sb.AppendLine($"\t{StructureRegion.Capacity.Key} = 1.0000");
            
            return sb.ToString();
        }
    }
}
