using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class BranchFileTest
    {
        private string filePath;

        [SetUp]
        public void Setup()
        {
            filePath = Path.Combine(FileUtils.CreateTempDirectory(), NetworkPropertiesHelper.BranchGuiFileName);
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        public void GivenTwoSewerConnections_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var sewerConnections = new List<IBranch>
            {
                new SewerConnection("sc_1"), new SewerConnection("sc_2")
            };

            WriteAndCheckBranchTypeFileContent(sewerConnections);
        }

        [Test]
        public void GivenTwoPipes_WhenWritingBranchTypeFile_ThenBranchTypesAreCorrectlyWritten()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1" }, new Pipe { Name = "pipe_2" }
            };
            
            WriteAndCheckBranchTypeFileContent(pipes);
        }

        [Test]
        public void GivenSewerConnections_WhenWritingBranchTypeFile_ThenWaterTypeIsWrittenToBranchFile()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1", WaterType = SewerConnectionWaterType.Combined },
                new SewerConnection { Name = "sc_1", WaterType = SewerConnectionWaterType.DryWater },
                new Pipe { Name = "pipe_2", WaterType = SewerConnectionWaterType.None },
                new SewerConnection { Name = "sc_2", WaterType = SewerConnectionWaterType.StormWater }
            };
            
            WriteAndCheckBranchTypeFileContent(pipes);
        }

        [Test]
        public void GivenSewerConnections_WhenWritingBranchTypeFile_ThenMaterialIsWrittenToBranchFile()
        {
            var pipes = new List<IBranch>
            {
                new Pipe { Name = "pipe_1", Material = SewerProfileMapping.SewerProfileMaterial.Masonry },
                new Pipe { Name = "pipe_2", Material = SewerProfileMapping.SewerProfileMaterial.SheetMetal },
                new SewerConnection { Name = "sc_2" },
                new Channel { Name = "channel_1" }
            };
            
            WriteAndCheckBranchTypeFileContent(pipes);
        }

        [Test]
        public void GivenTwoChannels_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var channels = new List<IBranch>
            {
                new Channel { Name = "channel_1" }, new Channel { Name = "channel_2" }
            };
            
            WriteAndCheckBranchTypeFileContent(channels);
        }

        [Test]
        public void GivenDifferentTypesOfBranches_WhenWritingBranchTypeFile_ThenBranchTypesAreCorretlyWritten()
        {
            var branches = new List<IBranch>
            {
                new Pipe { Name = "myPipe" }, new SewerConnection("mySewerConnection")
            };
            
            WriteAndCheckBranchTypeFileContent(branches);
        }

        [Test]
        public void GivenChannel_GettingBranchProperties_ShouldGiveCorrectPropertiesValues()
        {
            //Arrange
            var node1 = new HydroNode("Node 1"){Geometry = new Point(0,0)};
            var node2 = new HydroNode("Node 2") { Geometry = new Point(10, 10) };

            var branch = new Channel("channel",node1, node2)
            {
                Geometry = new LineString(new []{new Coordinate(0,0), new Coordinate(10,10)}),
                Length = 100,
                IsLengthCustom = true
            };

            // Act
            var properties = branch.GetBranchProperties();

            // Assert
            Assert.AreEqual(BranchFile.BranchType.Channel, properties.BranchType);
            Assert.AreEqual(true, properties.IsCustomLength);
        }

        [Test]
        public void GivenPipe_GettingBranchProperties_ShouldGiveCorrectPropertiesValues()
        {
            //Arrange
            var manhole1 = new Manhole("Node 1") { Geometry = new Point(0, 0) };
            var manhole2 = new Manhole("Node 2") { Geometry = new Point(10, 10) };

            var compartment1 = new Compartment("Compartment 1");
            var compartment2 = new Compartment("Compartment 2");

            manhole1.Compartments.Add(compartment1);
            manhole2.Compartments.Add(compartment2);

            var pipe = new Pipe
            {
                Name = "Pipe 1",
                Source = manhole1,
                Target = manhole2,
                SourceCompartment = compartment1,
                TargetCompartment = compartment2,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 10) }),
                Length = 100,
                IsLengthCustom = true
            };

            // Act
            var properties = pipe.GetBranchProperties();

            // Assert
            Assert.AreEqual(BranchFile.BranchType.Pipe, properties.BranchType);
            Assert.AreEqual(pipe.Name, properties.Name);
            Assert.AreEqual(true, properties.IsCustomLength);
            Assert.AreEqual(compartment1.Name ,properties.SourceCompartmentName);
            Assert.AreEqual(compartment2.Name , properties.TargetCompartmentName);
            Assert.AreEqual(pipe.WaterType, properties.WaterType);
            Assert.AreEqual(pipe.Material, properties.Material);
        }

        [Test]
        public void GivenSewerConnection_GettingBranchProperties_ShouldGiveCorrectPropertiesValues()
        {
            //Arrange
            var manhole1 = new Manhole("Node 1") { Geometry = new Point(0, 0) };
            var manhole2 = new Manhole("Node 2") { Geometry = new Point(10, 10) };

            var compartment1 = new Compartment("Compartment 1");
            var compartment2 = new Compartment("Compartment 2");

            manhole1.Compartments.Add(compartment1);
            manhole2.Compartments.Add(compartment2);

            var sewerConnection = new SewerConnection("SewerConnection 1")
            {
                Source = manhole1,
                Target = manhole2,
                SourceCompartment = compartment1,
                TargetCompartment = compartment2,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 10) }),
                Length = 100,
                IsLengthCustom = true
            };

            // Act
            var properties = sewerConnection.GetBranchProperties();

            // Assert
            Assert.AreEqual(BranchFile.BranchType.SewerConnection, properties.BranchType);
            Assert.AreEqual(sewerConnection.Name, properties.Name);
            Assert.AreEqual(true, properties.IsCustomLength);
            Assert.AreEqual(compartment1.Name, properties.SourceCompartmentName);
            Assert.AreEqual(compartment2.Name, properties.TargetCompartmentName);
            Assert.AreEqual(sewerConnection.WaterType, properties.WaterType);
        }

        private void WriteAndCheckBranchTypeFileContent(List<IBranch> branches)
        {
            BranchFile.Write(filePath, branches);
            var propertiesPerBranch = BranchFile.Read(filePath, null);
            for (var n = 0; n < propertiesPerBranch.Count; n++)
            {
                Assert.That(propertiesPerBranch[n].Name, Is.EqualTo(branches[n].Name));
                Assert.That(propertiesPerBranch[n].BranchType, Is.EqualTo(GetBranchType(branches[n])));
                Assert.That(propertiesPerBranch[n].Material, Is.EqualTo(GetMaterial(branches[n])));
            }
        }

        private static SewerProfileMapping.SewerProfileMaterial GetMaterial(IBranch branch)
        {
            var pipe = branch as Pipe;
            return pipe?.Material ?? SewerProfileMapping.SewerProfileMaterial.Unknown;
        }

        private static BranchFile.BranchType GetBranchType(IBranch branch)
        {
            switch (branch)
            {
                case IPipe p:
                    return BranchFile.BranchType.Pipe;
                case ISewerConnection s:
                    return BranchFile.BranchType.SewerConnection;
                default:
                    return BranchFile.BranchType.Channel;

            }

        }
    }
}