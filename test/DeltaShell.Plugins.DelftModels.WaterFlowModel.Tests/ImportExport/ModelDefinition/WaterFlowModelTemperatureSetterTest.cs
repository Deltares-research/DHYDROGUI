using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    class WaterFlowModelTemperatureSetterTest
    {
        [Test]
        public void GivenATemperatureCategoryWithProperties_WhenSettingTheseModelProperties_ThenPropertiesdShouldBeSetInModel()
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.TemperatureValuesHeader);
            category.AddProperty(ModelDefinitionsRegion.BackgroundTemperature.Key, "1.0");
            category.AddProperty(ModelDefinitionsRegion.SurfaceArea.Key, "2.0");
            category.AddProperty(ModelDefinitionsRegion.AtmosphericPressure.Key, "3.0");
            category.AddProperty(ModelDefinitionsRegion.DaltonNumber.Key, "4.0");
            category.AddProperty(ModelDefinitionsRegion.StantonNumber.Key, "5.0");
            category.AddProperty(ModelDefinitionsRegion.HeatCapacity.Key, "6.0");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelTemperatureSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(1.0, model.BackgroundTemperature);
            Assert.AreEqual(2.0, model.SurfaceArea);
            Assert.AreEqual(3.0, model.AtmosphericPressure);
            Assert.AreEqual(4.0, model.DaltonNumber);
            Assert.AreEqual(5.0, model.StantonNumber);
            Assert.AreEqual(6.0, model.HeatCapacityWater);
        }

        [Test]
        public void GivenATemperatureCategoryWithPropertiesWithInvalidValues_WhenSettingTheseModelProperties_ThenSixErrorMessagesShouldBeReportedAndDefaultValueSet()
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.TemperatureValuesHeader);
            category.AddProperty(ModelDefinitionsRegion.BackgroundTemperature.Key, "true");
            category.AddProperty(ModelDefinitionsRegion.SurfaceArea.Key, "true");
            category.AddProperty(ModelDefinitionsRegion.AtmosphericPressure.Key, "true");
            category.AddProperty(ModelDefinitionsRegion.DaltonNumber.Key, "true");
            category.AddProperty(ModelDefinitionsRegion.StantonNumber.Key, "true");
            category.AddProperty(ModelDefinitionsRegion.HeatCapacity.Key, "true");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelTemperatureSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(6, errorMessages.Count);
            Assert.IsTrue(errorMessages.Contains(GetExpectedDoublesValidationMessage(ModelDefinitionsRegion.BackgroundTemperature.Key)));
            Assert.IsTrue(errorMessages.Contains(GetExpectedDoublesValidationMessage(ModelDefinitionsRegion.SurfaceArea.Key)));
            Assert.IsTrue(errorMessages.Contains(GetExpectedDoublesValidationMessage(ModelDefinitionsRegion.AtmosphericPressure.Key)));
            Assert.IsTrue(errorMessages.Contains(GetExpectedDoublesValidationMessage(ModelDefinitionsRegion.DaltonNumber.Key)));
            Assert.IsTrue(errorMessages.Contains(GetExpectedDoublesValidationMessage(ModelDefinitionsRegion.StantonNumber.Key)));
            Assert.IsTrue(errorMessages.Contains(GetExpectedDoublesValidationMessage(ModelDefinitionsRegion.HeatCapacity.Key)));

            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueBackgroundTemperatureDefault, model.BackgroundTemperature);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueSurfaceAreaDefault, model.SurfaceArea);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueAtmosphericPressureDefault, model.AtmosphericPressure);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueDaltonNumberDefault, model.DaltonNumber);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueStantonNumberDefault, model.StantonNumber);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueHeatCapacityWaterDefault, model.HeatCapacityWater);
        }

        private static string GetExpectedDoublesValidationMessage(string propertyName)
        {
            return string.Format(Resources.WaterFlowModelTemperatureSetter_ParseStringToDouble_Line__0___Parameter___1___will_not_be_imported__Valid_values_are_doubles_only_
                , 0, propertyName);
        }

        [Test]
        public void GivenATemperatureCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            //Given
            var unknownPropertyName = "Unknown Property";
            var category = new DelftIniCategory(ModelDefinitionsRegion.TemperatureValuesHeader);
            category.AddProperty(unknownPropertyName, "unknown");
            category.AddProperty(ModelDefinitionsRegion.BackgroundTemperature.Key, "1.0");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelTemperatureSetter().SetProperties(category, model, errorMessages);

            //Then
            var expectedMessage = string.Format(
                Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(expectedMessage, errorMessages[0]);
            Assert.AreEqual(1.0, model.BackgroundTemperature);
        }

        [Test]
        public void
            GivenAWronglyDefinedCategoryHeader_WhenSettingThisModelProperty_ThenThisParameterShouldNotBeSetInTheModel()
        {
            var propertyName = ModelDefinitionsRegion.BackgroundTemperature.Key;

            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, "2.0");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelTemperatureSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueHeatCapacityWaterDefault, model.HeatCapacityWater);
        }
    }
}
