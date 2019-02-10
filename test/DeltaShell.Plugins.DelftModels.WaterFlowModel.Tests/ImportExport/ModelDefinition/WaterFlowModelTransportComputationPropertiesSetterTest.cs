using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
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

        [Test]
        public void GivenTransportComputationDataModelWithUnkownProperty_WhenSettingModelProperties_ThenUnknownPropertyIsSkippedAndErrorMessageIsReturned()
        {
            // Given
            var unknownPropertyName = "UnknownProperty";
            var category = GetCorrectTransportComputationDataModel();
            category.AddProperty(unknownPropertyName, 1);

            // When
            var errorMessages = new List<string>();
            transportComputationPropertiesSetter.SetProperties(category, waterFlowModel1D, errorMessages);

            // Then
            Assert.IsTrue(waterFlowModel1D.UseTemperature);
            Assert.That(waterFlowModel1D.DensityType, Is.EqualTo(DensityType.eckart));
            Assert.That(waterFlowModel1D.TemperatureModelType, Is.EqualTo(TemperatureModelType.Excess));

            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedMessage = string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                    0, unknownPropertyName);
            Assert.Contains(expectedMessage, errorMessages);
        }

        private static DelftIniCategory GetCorrectTransportComputationDataModel()
        {
            var timeSettingsCategory = new DelftIniCategory(ModelDefinitionsRegion.TransportComputationValuesHeader);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "1");
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.eckart.ToString());
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Excess.ToString());

            return timeSettingsCategory;
        }
    }
}