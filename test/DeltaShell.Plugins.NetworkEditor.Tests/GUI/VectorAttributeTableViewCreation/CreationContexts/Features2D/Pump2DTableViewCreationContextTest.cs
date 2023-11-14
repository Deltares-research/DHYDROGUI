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
    public class Pump2DTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new Pump2DTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Pump 2D table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Pump2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<Pump2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Pump2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainPumps_ReturnsFalse()
        {
            // Arrange
            var creationContext = new Pump2DTableViewCreationContext();
            var region = new HydroArea();
            region.Pumps.AddRange(new Pump2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<Pump2D>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsPumps_ReturnsTrue()
        {
            // Arrange
            var creationContext = new Pump2DTableViewCreationContext();
            var region = new HydroArea();
            region.Pumps.AddRange(new Pump2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Pumps);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Pump2DTableViewCreationContext();

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
            var creationContext = new Pump2DTableViewCreationContext();
            var feature = new Pump2D();

            // Act
            Pump2DRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}