using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    [TestFixture]
    public class LeveeBreachTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Levee breach table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(null, new List<Feature2D>());

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(new HydroArea(), null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainLeveeBreaches_ReturnsFalse()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();
            var region = new HydroArea();
            region.LeveeBreaches.AddRange(new Feature2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<Feature2D>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsLeveeBreaches_ReturnsTrue()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();
            var region = new HydroArea();
            region.LeveeBreaches.AddRange(new Feature2D[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.LeveeBreaches);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();

            // Act
            void Call() => creationContext.CreateFeatureRowObject(null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();
            var feature = new Feature2D();

            // Act
            Feature2DRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new LeveeBreachTableViewCreationContext();

            // Act
            void Call() => creationContext.CustomizeTableView(null, null, null);

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}