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
    public class LandBoundary2DTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Land boundary 2D table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<LandBoundary2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainLandBoundaries_ReturnsFalse()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();
            var region = new HydroArea();
            region.LandBoundaries.AddRange(new LandBoundary2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<LandBoundary2D>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsLandBoundaries_ReturnsTrue()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();
            var region = new HydroArea();
            region.LandBoundaries.AddRange(new LandBoundary2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.LandBoundaries);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<LandBoundary2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new LandBoundary2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new LandBoundary2DTableViewCreationContext();
            var feature = new LandBoundary2D();

            // Act
            LandBoundary2DRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<LandBoundary2D>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}