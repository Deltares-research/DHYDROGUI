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
    public class ObservationCrossSection2DTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Observation cross section 2D table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<ObservationCrossSection2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainObservationCrossSections_ReturnsFalse()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();
            var region = new HydroArea();
            region.ObservationCrossSections.AddRange(new ObservationCrossSection2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<ObservationCrossSection2D>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsObservationCrossSections_ReturnsTrue()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();
            var region = new HydroArea();
            region.ObservationCrossSections.AddRange(new ObservationCrossSection2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.ObservationCrossSections);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<ObservationCrossSection2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new ObservationCrossSection2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new ObservationCrossSection2DTableViewCreationContext();
            var feature = new ObservationCrossSection2D();

            // Act
            ObservationCrossSection2DRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<ObservationCrossSection2D>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}