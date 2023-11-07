using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    [TestFixture]
    public class Weir2DTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new Weir2DTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Weir 2D table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Weir2DTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(null, new List<Weir2D>());

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Weir2DTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(new HydroArea(), null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainWeirs_ReturnsFalse()
        {
            // Arrange
            var creationContext = new Weir2DTableViewCreationContext();
            var region = new HydroArea();
            region.Weirs.AddRange(new Weir2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<Weir2D>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsWeirs_ReturnsTrue()
        {
            // Arrange
            var creationContext = new Weir2DTableViewCreationContext();
            var region = new HydroArea();
            region.Weirs.AddRange(new Weir2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Weirs);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Weir2DTableViewCreationContext();

            // Act
            void Call() => creationContext.CreateFeatureRowObject(null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new Weir2DTableViewCreationContext();
            var feature = new Weir2D();

            // Act
            Weir2DRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}