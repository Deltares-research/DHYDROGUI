using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelTransportComputationPropertiesSetterTest
    {
        private WaterFlowModelTransportComputationPropertiesSetter transportComputationPropertiesSetter;
        private WaterFlowModel1D waterFlowModel1D;

        [SetUp]
        public void Initialize()
        {
            transportComputationPropertiesSetter = new WaterFlowModelTransportComputationPropertiesSetter();
            waterFlowModel1D = new WaterFlowModel1D();
        }

        [Test]
        public void GivenCorrectTransportComputationDataModel_WhenSettingModelProperties_ThenCorrectPropertiesAreSet()
        {
            // Given
            var category = GetCorrectTransportComputationDataModel();

            // When
            transportComputationPropertiesSetter.SetProperties(category, waterFlowModel1D, new List<string>());

            // Then
            Assert.IsTrue(waterFlowModel1D.UseTemperature);
            Assert.That(waterFlowModel1D.DensityType, Is.EqualTo(DensityType.eckart));
            Assert.That(waterFlowModel1D.TemperatureModelType, Is.EqualTo(TemperatureModelType.Excess));
        }

        private static DelftIniCategory GetCorrectTransportComputationDataModel()
        {
            var timeSettingsCategory = new DelftIniCategory(ModelDefinitionsRegion.TransportComputationValuesHeader);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "1");
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.Density.Key, "eckart");
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, "Excess");

            return timeSettingsCategory;
        }
    }
}