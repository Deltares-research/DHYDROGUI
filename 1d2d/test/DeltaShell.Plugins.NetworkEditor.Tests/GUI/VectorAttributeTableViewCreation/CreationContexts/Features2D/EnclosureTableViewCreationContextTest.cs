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
    public class EnclosureTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Enclosure table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<GroupableFeature2DPolygon>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainEnclosures_ReturnsFalse()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();
            var region = new HydroArea();
            region.Enclosures.AddRange(new GroupableFeature2DPolygon[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<GroupableFeature2DPolygon>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsEnclosures_ReturnsTrue()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();
            var region = new HydroArea();
            region.Enclosures.AddRange(new GroupableFeature2DPolygon[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Enclosures);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<GroupableFeature2DPolygon>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new GroupableFeature2DPolygon(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new EnclosureTableViewCreationContext();
            var feature = new GroupableFeature2DPolygon();

            // Act
            GroupableFeature2DPolygonRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<GroupableFeature2DPolygon>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}