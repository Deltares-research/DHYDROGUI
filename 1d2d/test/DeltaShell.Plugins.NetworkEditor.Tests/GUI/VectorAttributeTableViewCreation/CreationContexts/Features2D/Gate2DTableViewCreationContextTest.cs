using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    [TestFixture]
    public class Gate2DTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Gate 2D table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<Gate2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainGates_ReturnsFalse()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();
            var region = new HydroArea();
            region.Gates.AddRange(new Gate2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<Gate2D>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsGates_ReturnsTrue()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();
            var region = new HydroArea();
            region.Gates.AddRange(new Gate2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Gates);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<Gate2D>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new Gate2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new Gate2DTableViewCreationContext();
            var feature = new Gate2D();

            // Act
            Gate2DRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<Gate2D>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}