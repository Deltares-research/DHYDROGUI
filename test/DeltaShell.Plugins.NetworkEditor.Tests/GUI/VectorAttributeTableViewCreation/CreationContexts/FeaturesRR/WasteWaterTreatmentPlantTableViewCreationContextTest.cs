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
    public class WasteWaterTreatmentPlantTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Waste water treatment plant table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(null, Enumerable.Empty<WasteWaterTreatmentPlant>());

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();

            // Act
            void Call() => creationContext.IsRegionData(Substitute.For<IDrainageBasin>(), null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainWasteWaterTreatmentPlants_ReturnsFalse()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();
            var region = Substitute.For<IDrainageBasin>();
            region.WasteWaterTreatmentPlants.Returns(new EventedList<WasteWaterTreatmentPlant>());

            // Act
            bool result = creationContext.IsRegionData(region, new WasteWaterTreatmentPlant[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsWasteWaterTreatmentPlants_ReturnsTrue()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();
            var region = Substitute.For<IDrainageBasin>();
            region.WasteWaterTreatmentPlants.Returns(new EventedList<WasteWaterTreatmentPlant>());

            // Act
            bool result = creationContext.IsRegionData(region, region.WasteWaterTreatmentPlants);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();

            // Act
            void Call() => creationContext.CreateFeatureRowObject(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();
            WasteWaterTreatmentPlant feature = Substitute.For<WasteWaterTreatmentPlant, INotifyPropertyChanged>();

            // Act
            WasteWaterTreatmentPlantRow result = creationContext.CreateFeatureRowObject(feature);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableView_DoesNothing()
        {
            // Arrange
            var creationContext = new WasteWaterTreatmentPlantTableViewCreationContext();

            // Act
            void Call() => creationContext.CustomizeTableView(null, null, null);

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}