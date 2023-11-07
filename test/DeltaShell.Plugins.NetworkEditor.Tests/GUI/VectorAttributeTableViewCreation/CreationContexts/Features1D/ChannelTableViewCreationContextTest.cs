using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    [TestFixture]
    public class ChannelTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Channel table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(null, Substitute.For<IEnumerable<IChannel>>());

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainChannels_ReturnsFalse()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Channels.Returns(new IChannel[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new IChannel[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsChannels_ReturnsTrue()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Channels.Returns(new IChannel[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Channels);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();

            // Act
            void Call() => creationContext.CreateFeatureRowObject(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();
            IChannel feature = Substitute.For<IChannel, INotifyPropertyChanged>();

            // Act
            ChannelRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new ChannelTableViewCreationContext();

            // Act
            void Call() => creationContext.CustomizeTableView(null, null, null);

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}