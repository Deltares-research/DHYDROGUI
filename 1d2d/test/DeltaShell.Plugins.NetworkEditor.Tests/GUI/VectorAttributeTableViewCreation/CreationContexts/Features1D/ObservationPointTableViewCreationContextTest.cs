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
    public class ObservationPointTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Observation point table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Substitute.For<IEnumerable<IObservationPoint>>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainObservationPoints_ReturnsFalse()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.ObservationPoints.Returns(new IObservationPoint[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new IObservationPoint[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsObservationPoints_ReturnsTrue()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.ObservationPoints.Returns(new IObservationPoint[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.ObservationPoints);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<IObservationPoint>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(Substitute.For<IObservationPoint>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();
            IObservationPoint feature = Substitute.For<IObservationPoint, INotifyPropertyChanged>();

            // Act
            ObservationPointRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<IObservationPoint>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

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