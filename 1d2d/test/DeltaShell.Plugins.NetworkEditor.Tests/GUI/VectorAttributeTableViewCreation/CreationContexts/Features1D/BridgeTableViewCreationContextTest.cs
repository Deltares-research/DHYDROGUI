using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    [TestFixture]
    public class BridgeTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Bridge table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Enumerable.Empty<IBridge>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainBridges_ReturnsFalse()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Bridges.Returns(new IBridge[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new IBridge[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsBridges_ReturnsTrue()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Bridges.Returns(new IBridge[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Bridges);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<IBridge>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new Bridge(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();
            IBridge feature = Substitute.For<IBridge, INotifyPropertyChanged>();

            // Act
            BridgeRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<IBridge>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new BridgeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CustomizeTableView(null, null, null);
            }

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}