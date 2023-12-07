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
    public class FixedWeirTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Fixed weir table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<FixedWeir>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainFixedWeirs_ReturnsFalse()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();
            var region = new HydroArea();
            region.FixedWeirs.AddRange(new FixedWeir[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<FixedWeir>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsFixedWeirs_ReturnsTrue()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();
            var region = new HydroArea();
            region.FixedWeirs.AddRange(new FixedWeir[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.FixedWeirs);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<FixedWeir>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new FixedWeir(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new FixedWeirTableViewCreationContext();
            var feature = new FixedWeir();

            // Act
            FixedWeirRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<FixedWeir>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}