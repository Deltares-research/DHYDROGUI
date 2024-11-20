using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    [TestFixture]
    public class HydroNodeTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Hydro node table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Substitute.For<IEnumerable<IHydroNode>>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainHydroNodes_ReturnsFalse()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.HydroNodes.Returns(new IHydroNode[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new IHydroNode[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsHydroNodes_ReturnsTrue()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.HydroNodes.Returns(new IHydroNode[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.HydroNodes);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<IHydroNode>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(Substitute.For<IHydroNode>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();
            IHydroNode feature = Substitute.For<IHydroNode, INotifyPropertyChanged>();

            // Act
            HydroNodeRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<IHydroNode>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new HydroNodeTableViewCreationContext();

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