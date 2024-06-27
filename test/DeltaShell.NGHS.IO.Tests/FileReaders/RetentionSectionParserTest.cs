using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileReaders.Retention;
using DeltaShell.NGHS.IO.FileWriters.Retention;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class RetentionSectionParserTest
    {
        private static IniSection CreateDefaultIniSection(string headerName = RetentionRegion.StorageNodeHeader)
        {
            var iniSection = new IniSection(headerName);
            iniSection.AddProperty("id", "some_id");
            iniSection.AddProperty("name", "some_name");
            iniSection.AddProperty("useTable", true);
            iniSection.AddProperty("numLevels", 1);
            iniSection.AddProperty("levels", 1.23);
            iniSection.AddProperty("storageArea", 4.56);
            iniSection.AddProperty("interpolate", "block");

            return iniSection;

        }

        [Test]
        public void ParseIniSections_WithRetentionWithOneLevel_AddCorrectRetentionToNetwork()
        {
            // Setup
            var iniSection = CreateDefaultIniSection();
            iniSection.AddProperty("branchId", "some_branch");
            iniSection.AddProperty("chainage", 50.0);
            
            IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

            // Call
            var retentions = RetentionSectionParser.ParseIniSections(new []{iniSection}, network);

            // Assert
            IRetention retention = retentions.Single();
            Assert.That(retention.Name, Is.EqualTo("some_id"));
            Assert.That(retention.LongName, Is.EqualTo("some_name"));
            Assert.That(retention.Branch, Is.SameAs(network.Branches[0]));
            Assert.That(retention.Chainage, Is.EqualTo(50));
            Assert.That(retention.Geometry, Is.EqualTo(new Point(0, 50)));
            Assert.That(retention.UseTable, Is.False);
            Assert.That(retention.BedLevel, Is.EqualTo(1.23));
            Assert.That(retention.StorageArea, Is.EqualTo(4.56));
        }

        [Test]
        public void ParseIniSections_UseTablePropertyNotInFile_LogsError()
        {
            // Setup
            var iniSection = CreateDefaultIniSection();
            iniSection.AddProperty("branchId", "some_branch");
            iniSection.AddProperty("chainage", 50.0);
            iniSection.LineNumber = 6;
            iniSection.RemoveProperty(iniSection.FindProperty("useTable"));

            IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

            // Call
            void Call() => RetentionSectionParser.ParseIniSections(new []{iniSection}, network).ToArray();

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("The section StorageNode on line 6 does not contain the useTable property."));
            Assert.That(network.Retentions, Is.Empty);
        }

        [Test]
        public void ParseIniSections_WithRetentionWithMultipleLevels_AddCorrectRetentionToNetwork()
        {
            // Setup
            var iniSection = new IniSection(RetentionRegion.StorageNodeHeader);
            iniSection.AddProperty("id", "some_id");
            iniSection.AddProperty("name", "some_name");
            iniSection.AddProperty("branchId", "some_branch");
            iniSection.AddProperty("chainage", 50.0);
            iniSection.AddProperty("useTable", true);
            iniSection.AddProperty("numLevels", 3);
            iniSection.AddPropertyWithMultipleValuesWithOptionalComment("levels", new []{ 1.11, 2.22, 3.33 });
            iniSection.AddPropertyWithMultipleValuesWithOptionalComment("storageArea", new []{ 4.44, 5.55, 6.66 });
            iniSection.AddProperty("interpolate", "linear");

            IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

            // Call
            var retentions = RetentionSectionParser.ParseIniSections(new []{iniSection}, network);

            // Assert
            IRetention retention = retentions.Single();
            Assert.That(retention.Name, Is.EqualTo("some_id"));
            Assert.That(retention.LongName, Is.EqualTo("some_name"));
            Assert.That(retention.Branch, Is.SameAs(network.Branches[0]));
            Assert.That(retention.Chainage, Is.EqualTo(50));
            Assert.That(retention.Geometry, Is.EqualTo(new Point(0, 50)));
            Assert.That(retention.UseTable, Is.True);
            Assert.That(retention.Data.Arguments[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
            Assert.That(retention.Data.Arguments[0].GetValues<double>().ToArray(), Is.EquivalentTo(new[]
            {
                1.11,
                2.22,
                3.33
            }));
            Assert.That(retention.Data.Components[0].GetValues<double>().ToArray(), Is.EquivalentTo(new[]
            {
                4.44,
                5.55,
                6.66
            }));
        }

        private static IHydroNetwork GetNetworkWithChannel(double length, string branchName)
        {
            var node1 = new HydroNode("some_node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("some_node2") {Geometry = new Point(0, length)};

            var channel = new Channel(branchName, node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, length)
                })
            };

            var network = new HydroNetwork();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(channel);

            return network;
        }

        [Test]
        public void GivenRetentionFileReader_ParseIniSections_ShouldCorrectlyParseNodeId()
        {
            //Arrange
            var iniSection = CreateDefaultIniSection();
            iniSection.AddProperty("nodeId", "some_node");
            
            var network = Substitute.For<IHydroNetwork>();
            var expectedBranch = Substitute.For<IBranch>();
            var node = Substitute.For<INode>();
            var branchFeatures = new EventedList<IBranchFeature>();

            expectedBranch.Name.Returns("some_branch");
            expectedBranch.BranchFeatures.Returns(branchFeatures);
            expectedBranch.Target.Returns(node);
            expectedBranch.Geometry.Returns(new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 100)
            }));
            expectedBranch.Length.Returns(100);

            node.Name.Returns("some_node");
            node.IncomingBranches.Returns(new EventedList<IBranch> { expectedBranch });
            
            network.Branches.Returns(new EventedList<IBranch> { expectedBranch });
            network.Nodes.Returns(new EventedList<INode> { node });

            // Act
            var retentions = RetentionSectionParser.ParseIniSections(new []{iniSection}, network);

            // Assert
            var retention = retentions.FirstOrDefault();
            Assert.NotNull(retention);
            Assert.AreEqual(100, retention.Chainage);
            Assert.AreEqual(expectedBranch, retention.Branch);
        }

        [Test]
        public void GivenRetentionFileReader_ParseIniSections_ShouldGiveMessageIfNodeIdCanNotBeFound()
        {
            //Arrange
            var iniSection = CreateDefaultIniSection();
            iniSection.AddProperty("nodeId", "some_node");

            var network = Substitute.For<IHydroNetwork>();

            // Act & Assert
            const string expectedLogMessage = "Could not find node with nodeId some_node (retention some_id, line number 0)";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                RetentionSectionParser.ParseIniSections(new[] { iniSection }, network).ToArray();
            }, expectedLogMessage);
        }

        [Test]
        public void GivenRetentionFileReader_ParseIniSections_ShouldGiveMessageIfNoBranchCanNotBeFoundForNode()
        {
            //Arrange
            var iniSection = CreateDefaultIniSection();
            iniSection.AddProperty("nodeId", "some_node");
            
            var network = Substitute.For<IHydroNetwork>();
            var node = Substitute.For<INode>();

            node.Name.Returns("some_node");
            network.Nodes.Returns(new EventedList<INode> { node });

            // Act & Assert
            var expectedLogMessage = "Could not find a branch for node with nodeId some_node (retention some_id, line number 0)";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                RetentionSectionParser.ParseIniSections(new[] { iniSection }, network).ToArray();
            }, expectedLogMessage);
        }

        [Test]
        public void GivenRetentionFileReader_ParseIniSections_ShouldGiveMessageIfNoBranchCanNotBeFoundForBranchId()
        {
            //Arrange
            var iniSection = CreateDefaultIniSection();
            iniSection.AddProperty("chainage", 0);
            iniSection.AddProperty("branchId", "some_branch"); 
            
            var network = Substitute.For<IHydroNetwork>();
            
            // Act & Assert
            var expectedLogMessage = "Could not find branch for branch id \"some_branch\" (retention some_id, line number 0)";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                RetentionSectionParser.ParseIniSections(new[] { iniSection }, network).ToArray();
            }, expectedLogMessage);
        }

        [Test]
        public void GivenRetentionFileReader_ParseIniSections_SkipsIniSectionsWithDifferentIniSectionName()
        {
            //Arrange
            var iniSection = CreateDefaultIniSection("ABC");
            iniSection.AddProperty("chainage", 0);
            iniSection.AddProperty("branchId", "some_branch");

            var network = Substitute.For<IHydroNetwork>();

            // Act
            var retentions = RetentionSectionParser.ParseIniSections(new[] { iniSection }, network);

            // Assert
            Assert.AreEqual(0, retentions.Count());
        }
    }
}
