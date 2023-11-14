using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    [TestFixture]
    public class GullyTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new GullyTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Gully table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new GullyTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<Gully>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new GullyTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainGullies_ReturnsFalse()
        {
            // Arrange
            var creationContext = new GullyTableViewCreationContext();
            var region = new HydroArea();
            region.Gullies.AddRange(new Gully[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<Gully>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsGullies_ReturnsTrue()
        {
            // Arrange
            var creationContext = new GullyTableViewCreationContext();
            var region = new HydroArea();
            region.Gullies.AddRange(new Gully[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Gullies);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new GullyTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new GullyTableViewCreationContext();
            var feature = new Gully();

            // Act
            GullyRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}