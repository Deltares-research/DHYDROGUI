using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
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
                creationContext.IsRegionData(null, new List<ObservationPoint2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainObservationPoints_ReturnsFalse()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();
            var region = new HydroArea();
            region.ObservationPoints.AddRange(new ObservationPoint2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<ObservationPoint2D>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsObservationPoints_ReturnsTrue()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();
            var region = new HydroArea();
            region.ObservationPoints.AddRange(new ObservationPoint2D[3]);

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
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<ObservationPoint2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new ObservationPoint2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new ObservationPointTableViewCreationContext();
            var feature = new ObservationPoint2D();

            // Act
            GroupableFeature2DPointRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<ObservationPoint2D>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}