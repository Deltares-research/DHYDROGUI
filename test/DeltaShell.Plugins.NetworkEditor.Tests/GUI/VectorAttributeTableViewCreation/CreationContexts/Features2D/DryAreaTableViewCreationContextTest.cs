using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    [TestFixture]
    public class DryAreaTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new DryAreaTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Dry area table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new DryAreaTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(null, new List<GroupableFeature2DPolygon>());

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new DryAreaTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(new HydroArea(), null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainDryAreas_ReturnsFalse()
        {
            // Arrange
            var creationContext = new DryAreaTableViewCreationContext();
            var region = new HydroArea();
            region.DryAreas.AddRange(new GroupableFeature2DPolygon[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<GroupableFeature2DPolygon>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsDryAreas_ReturnsTrue()
        {
            // Arrange
            var creationContext = new DryAreaTableViewCreationContext();
            var region = new HydroArea();
            region.DryAreas.AddRange(new GroupableFeature2DPolygon[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.DryAreas);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new DryAreaTableViewCreationContext();

            // Act
            void Call() => creationContext.CreateFeatureRowObject(null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new DryAreaTableViewCreationContext();
            var feature = new GroupableFeature2DPolygon();

            // Act
            GroupableFeature2DPolygonRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}