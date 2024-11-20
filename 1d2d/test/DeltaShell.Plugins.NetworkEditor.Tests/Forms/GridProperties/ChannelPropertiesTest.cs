using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.ComponentModel;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class ChannelPropertiesTest
    {
        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new Channel { Name = "some_name" };
            var properties = new ChannelProperties { Data = data };
            properties.NameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_invalid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);

            var data = new Channel { Name = "some_name" };
            var properties = new ChannelProperties { Data = data };
            properties.NameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_valid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void GivenChannelWithDifferentGeodeticLength_ThenPropertyVisibilityIsEitherShownOrNot()
        {
            var channel = new Channel() {GeodeticLength = 150 };
            var channelProperties = new ChannelProperties { Data = channel };
            Assert.That(DynamicVisibleAttribute.IsDynamicVisible(channelProperties, nameof(channel.GeodeticLength)), Is.True);
            Assert.That(channelProperties.IsVisible(nameof(channel.GeodeticLength)), Is.True);

            channel.GeodeticLength = double.NaN;
            Assert.That(DynamicVisibleAttribute.IsDynamicVisible(channelProperties, nameof(channel.GeodeticLength)), Is.False);
            Assert.That(channelProperties.IsVisible(nameof(channel.GeodeticLength)), Is.False);

            channel.GeodeticLength = 0;
            Assert.That(DynamicVisibleAttribute.IsDynamicVisible(channelProperties, nameof(channel.GeodeticLength)), Is.True);
            Assert.That(channelProperties.IsVisible(nameof(channel.GeodeticLength)), Is.True);
        }

        [Test]
        public void NumberOfStructures()
        {
            var network = new HydroNetwork();
            INode node1 = new HydroNode { Name = "Node1", Network = network };
            INode node2 = new HydroNode { Name = "Node2", Network = network };
            node1.Geometry = new Point(0.0, 0.0);
            node2.Geometry = new Point(100.0, 0.0);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch1", node1, node2) {Geometry = new LineString(new [] { node1.Geometry.Coordinate, node2.Geometry.Coordinate})};

            var weir1 = new Weir { Geometry = new Point(5, 0), OffsetY = 150, CrestWidth = 50, CrestLevel = 8 };
            var weir2 = new Weir { Geometry = new Point(5, 0), OffsetY = 150, CrestWidth = 55, CrestLevel = 10 };
            var compositeBranchStructure = new CompositeBranchStructure { Network = network, Geometry = new Point(5, 0), Chainage = 5 };
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, branch, compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir1);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir2);

            var channelProperties = new ChannelProperties { Data = branch };

            Assert.AreEqual(1, channelProperties.Structures);
            Assert.AreEqual(2, channelProperties.Weirs);

        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new ChannelProperties { Data = new Channel(new HydroNode(), new HydroNode()) });
        }
    }
}
