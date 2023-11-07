using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    [TestFixture]
    public class ManholeTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Manhole node table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(null, Substitute.For<IEnumerable<IManhole>>());

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainManholes_ReturnsFalse()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Manholes.Returns(new IManhole[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new IManhole[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsManholes_ReturnsTrue()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.Manholes.Returns(new IManhole[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.Manholes);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();

            // Act
            void Call() => creationContext.CreateFeatureRowObject(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();
            IManhole feature = Substitute.For<IManhole, INotifyPropertyChanged>();

            // Act
            ManholeRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new ManholeTableViewCreationContext();

            // Act
            void Call() => creationContext.CustomizeTableView(null, null, null);

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}