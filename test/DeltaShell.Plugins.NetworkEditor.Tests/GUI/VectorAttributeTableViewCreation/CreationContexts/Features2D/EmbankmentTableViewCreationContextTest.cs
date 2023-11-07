using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    [TestFixture]
    public class EmbankmentTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Embankment table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(null, new List<Embankment>());

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(new HydroArea(), null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainEmbankments_ReturnsFalse()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();
            var region = new HydroArea();
            region.Embankments.AddRange(new Embankment[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<Embankment>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsEmbankments_ReturnsTrue()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();
            var region = new HydroArea();
            region.Embankments.AddRange(new Embankment[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Embankments);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();

            // Act
            void Call() => creationContext.CreateFeatureRowObject(null);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();
            var feature = new Embankment();

            // Act
            EmbankmentRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new EmbankmentTableViewCreationContext();

            // Act
            void Call() => creationContext.CustomizeTableView(null, null, null);

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}