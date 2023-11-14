using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.FeaturesRR;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.FeaturesRR
{
    [TestFixture]
    public class RunoffBoundaryTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Runoff boundary table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Enumerable.Empty<RunoffBoundary>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IDrainageBasin>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainRunoffBoundaries_ReturnsFalse()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();
            var region = Substitute.For<IDrainageBasin>();
            region.Boundaries.Returns(new EventedList<RunoffBoundary>());

            // Act
            bool result = creationContext.IsRegionData(region, new RunoffBoundary[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsRunoffBoundaries_ReturnsTrue()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();
            var region = Substitute.For<IDrainageBasin>();
            region.Boundaries.Returns(new EventedList<RunoffBoundary>());

            // Act
            bool result = creationContext.IsRegionData(region, region.Boundaries);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();
            RunoffBoundary feature = Substitute.For<RunoffBoundary, INotifyPropertyChanged>();

            // Act
            RunoffBoundaryRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new RunoffBoundaryTableViewCreationContext();

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