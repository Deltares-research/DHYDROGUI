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
    public class DryPointTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Dry point table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, new List<GroupablePointFeature>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(new HydroArea(), null);
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainDryPoints_ReturnsFalse()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();
            var region = new HydroArea();
            region.DryPoints.AddRange(new GroupablePointFeature[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new List<GroupablePointFeature>());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsDryPoints_ReturnsTrue()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();
            var region = new HydroArea();
            region.DryPoints.AddRange(new GroupablePointFeature[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.DryPoints);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<GroupablePointFeature>());
            }

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new GroupablePointFeature(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new DryPointTableViewCreationContext();
            var feature = new GroupablePointFeature();

            // Act
            GroupablePointFeatureRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<GroupablePointFeature>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}