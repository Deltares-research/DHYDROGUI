using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    [TestFixture]
    public class RetentionTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Retention table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Substitute.For<IEnumerable<IRetention>>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainRetentions_ReturnsFalse()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Retentions.Returns(new IRetention[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new IRetention[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsRetentions_ReturnsTrue()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Retentions.Returns(new IRetention[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Retentions);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<IRetention>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(Substitute.For<IRetention>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new RetentionTableViewCreationContext();
            IRetention feature = Substitute.For<IRetention, INotifyPropertyChanged>();

            // Act
            RetentionRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<IRetention>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}