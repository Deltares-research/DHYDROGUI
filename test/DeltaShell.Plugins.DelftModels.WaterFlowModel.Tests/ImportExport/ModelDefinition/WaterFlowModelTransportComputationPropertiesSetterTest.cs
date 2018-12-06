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
        [Test]
        public void GivenCorrectTranpostComputationDataModel_WhenSettingModelProperties_ThenCorrectPropertiesAreSet()
        {
            // Given
            var category = GetCorrectTransportComputationDataModel();
            var model = new WaterFlowModel1D();

            // When
            new WaterFlowModelTransportComputationPropertiesSetter().SetProperties(category, model, (s, list) => { });

            // Then
            Assert.IsTrue(model.UseTemperature);
            Assert.That(model.DensityType, Is.EqualTo(DensityType.eckart));
            Assert.That(model.TemperatureModelType, Is.EqualTo(TemperatureModelType.Excess));
        }

        private DelftIniCategory GetCorrectTransportComputationDataModel()
        {
            var timeSettingsCategory = new DelftIniCategory(ModelDefinitionsRegion.TransportComputationValuesHeader);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "1");
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.Density.Key, "eckart");
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, "Excess");

            return timeSettingsCategory;
        }
    }
}