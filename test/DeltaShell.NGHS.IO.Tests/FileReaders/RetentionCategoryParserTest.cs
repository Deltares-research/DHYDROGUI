using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
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
    public class RetentionCategoryParserTest
    {
        private static IDelftIniCategory CreateDefaultCategory(string headerName = RetentionRegion.StorageNodeHeader)
        {
            var category = new DelftIniCategory(headerName);
            category.AddProperty("id", "some_id");
            category.AddProperty("name", "some_name");
            category.AddProperty("useTable", true);
            category.AddProperty("numLevels", 1);
            category.AddProperty("levels", 1.23);
            category.AddProperty("storageArea", 4.56);
            category.AddProperty("interpolate", "block");

            return category;

        }

        [Test]
        public void ParseCategories_WithRetentionWithOneLevel_AddCorrectRetentionToNetwork()
        {
            // Setup
            var category = CreateDefaultCategory();
            category.AddProperty("branchId", "some_branch");
            category.AddProperty("chainage", 50.0);
            
            IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

            // Call
            var retentions = RetentionCategoryParser.ParseCategories(new []{category}, network);

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
        public void ParseCategories_UseTablePropertyNotInFile_LogsError()
        {
            // Setup
            var category = CreateDefaultCategory();
            category.AddProperty("branchId", "some_branch");
            category.AddProperty("chainage", 50.0);
            category.LineNumber = 6;
            category.Properties.Remove((DelftIniProperty) category.GetProperty("useTable"));

            IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

            // Call
            void Call() => RetentionCategoryParser.ParseCategories(new []{category}, network).ToArray();

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("The category StorageNode on line 6 does not contain the useTable property."));
            Assert.That(network.Retentions, Is.Empty);
        }

        [Test]
        public void ParseCategories_WithRetentionWithMultipleLevels_AddCorrectRetentionToNetwork()
        {
            // Setup
            var category = new DelftIniCategory(RetentionRegion.StorageNodeHeader);
            category.AddProperty("id", "some_id");
            category.AddProperty("name", "some_name");
            category.AddProperty("branchId", "some_branch");
            category.AddProperty("chainage", 50.0);
            category.AddProperty("useTable", true);
            category.AddProperty("numLevels", 3);
            category.AddProperty("levels", new []{ 1.11, 2.22, 3.33 });
            category.AddProperty("storageArea", new []{ 4.44, 5.55, 6.66 });
            category.AddProperty("interpolate", "linear");

            IHydroNetwork network = GetNetworkWithChannel(100, "some_branch");

            // Call
            var retentions = RetentionCategoryParser.ParseCategories(new []{category}, network);

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
        public void GivenRetentionFileReader_ParseCategories_ShouldCorrectlyParseNodeId()
        {
            //Arrange
            var category = CreateDefaultCategory();
            category.AddProperty("nodeId", "some_node");
            
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
            var retentions = RetentionCategoryParser.ParseCategories(new []{category}, network);

            // Assert
            var retention = retentions.FirstOrDefault();
            Assert.NotNull(retention);
            Assert.AreEqual(100, retention.Chainage);
            Assert.AreEqual(expectedBranch, retention.Branch);
        }

        [Test]
        public void GivenRetentionFileReader_ParseCategories_ShouldGiveMessageIfNodeIdCanNotBeFound()
        {
            //Arrange
            var category = CreateDefaultCategory();
            category.AddProperty("nodeId", "some_node");

            var network = Substitute.For<IHydroNetwork>();

            // Act & Assert
            const string expectedLogMessage = "Could not find node with nodeId some_node (retention some_id, line number 0)";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                RetentionCategoryParser.ParseCategories(new[] { category }, network).ToArray();
            }, expectedLogMessage);
        }

        [Test]
        public void GivenRetentionFileReader_ParseCategories_ShouldGiveMessageIfNoBranchCanNotBeFoundForNode()
        {
            //Arrange
            var category = CreateDefaultCategory();
            category.AddProperty("nodeId", "some_node");
            
            var network = Substitute.For<IHydroNetwork>();
            var node = Substitute.For<INode>();

            node.Name.Returns("some_node");
            network.Nodes.Returns(new EventedList<INode> { node });

            // Act & Assert
            var expectedLogMessage = "Could not find a branch for node with nodeId some_node (retention some_id, line number 0)";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                RetentionCategoryParser.ParseCategories(new[] { category }, network).ToArray();
            }, expectedLogMessage);
        }

        [Test]
        public void GivenRetentionFileReader_ParseCategories_ShouldGiveMessageIfNoBranchCanNotBeFoundForBranchId()
        {
            //Arrange
            var category = CreateDefaultCategory();
            category.AddProperty("chainage", 0);
            category.AddProperty("branchId", "some_branch"); 
            
            var network = Substitute.For<IHydroNetwork>();
            
            // Act & Assert
            var expectedLogMessage = "Could not find branch for branch id \"some_branch\" (retention some_id, line number 0)";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                RetentionCategoryParser.ParseCategories(new[] { category }, network).ToArray();
            }, expectedLogMessage);
        }

        [Test]
        public void GivenRetentionFileReader_ParseCategories_SkipsCategoriesWithDifferentCategoryName()
        {
            //Arrange
            var category = CreateDefaultCategory("ABC");
            category.AddProperty("chainage", 0);
            category.AddProperty("branchId", "some_branch");

            var network = Substitute.For<IHydroNetwork>();

            // Act
            var retentions = RetentionCategoryParser.ParseCategories(new[] { category }, network);

            // Assert
            Assert.AreEqual(0, retentions.Count());
        }
    }
}
