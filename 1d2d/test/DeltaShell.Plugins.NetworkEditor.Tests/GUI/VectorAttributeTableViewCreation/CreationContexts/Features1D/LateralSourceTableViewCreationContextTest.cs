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
    public class LateralSourceTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Lateral source table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Substitute.For<IEnumerable<ILateralSource>>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainLateralSources_ReturnsFalse()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.LateralSources.Returns(new ILateralSource[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new ILateralSource[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsLateralSources_ReturnsTrue()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.LateralSources.Returns(new ILateralSource[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.LateralSources);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<ILateralSource>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(Substitute.For<ILateralSource>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();
            ILateralSource feature = Substitute.For<ILateralSource, INotifyPropertyChanged>();

            // Act
            LateralSourceRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<ILateralSource>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new LateralSourceTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CustomizeTableView(null, null, null);
            }

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}