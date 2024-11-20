using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    [TestFixture]
    public class CompositeBranchStructureTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Composite branch structure table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Substitute.For<IEnumerable<ICompositeBranchStructure>>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroNetwork>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainCompositeBranchStructures_ReturnsFalse()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.CompositeBranchStructures.Returns(new ICompositeBranchStructure[3]);

            // Act
            bool result = creationContext.IsRegionData(region, new ICompositeBranchStructure[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsCompositeBranchStructures_ReturnsTrue()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();
            var region = Substitute.For<IHydroNetwork>();
            region.CompositeBranchStructures.Returns(new ICompositeBranchStructure[3]);

            // Act
            bool result = creationContext.IsRegionData(region, region.CompositeBranchStructures);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<ICompositeBranchStructure>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(Substitute.For<ICompositeBranchStructure>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();
            ICompositeBranchStructure feature = Substitute.For<ICompositeBranchStructure, INotifyPropertyChanged>();

            // Act
            CompositeBranchStructureRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<ICompositeBranchStructure>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new CompositeBranchStructureTableViewCreationContext();

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