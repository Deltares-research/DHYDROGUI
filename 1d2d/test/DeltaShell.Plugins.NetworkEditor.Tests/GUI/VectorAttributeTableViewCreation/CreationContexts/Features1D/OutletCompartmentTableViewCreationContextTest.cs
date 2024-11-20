using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    [TestFixture]
    public class OutletCompartmentTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Outlet compartment table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Substitute.For<IEnumerable<OutletCompartment>>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainOutletCompartments_ReturnsFalse()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.OutletCompartments.Returns(new OutletCompartment[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new OutletCompartment[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsOutletCompartments_ReturnsTrue()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.OutletCompartments.Returns(new OutletCompartment[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.OutletCompartments);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<OutletCompartment>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new OutletCompartment(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new OutletCompartmentTableViewCreationContext();
            OutletCompartment feature = Substitute.For<OutletCompartment, INotifyPropertyChanged>();

            // Act
            OutletCompartmentRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<OutletCompartment>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}