using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;
using System.Collections.Generic;

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
            category.AddProperty("BackgroundTemperature", "1.0");
            category.AddProperty("SurfaceArea", "2.0");
            category.AddProperty("AtmosphericPressure", "3.0");
            category.AddProperty("DaltonNumber", "4.0");
            category.AddProperty("StantonNumber", "5.0");
            category.AddProperty("HeatCapacityWater", "6.0");

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
            category.AddProperty("BackgroundTemperature", "true");
            category.AddProperty("SurfaceArea", "true");
            category.AddProperty("AtmosphericPressure", "true");
            category.AddProperty("DaltonNumber", "true");
            category.AddProperty("StantonNumber", "true");
            category.AddProperty("HeatCapacityWater", "true");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelTemperatureSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(6, errorMessages.Count);
            Assert.IsTrue(errorMessages.Contains(
                "Line 0: Parameter 'BackgroundTemperature' will not be imported. Valid values are doubles only."));
            Assert.IsTrue(errorMessages.Contains(
                "Line 0: Parameter 'SurfaceArea' will not be imported. Valid values are doubles only."));
            Assert.IsTrue(errorMessages.Contains(
                "Line 0: Parameter 'AtmosphericPressure' will not be imported. Valid values are doubles only."));
            Assert.IsTrue(errorMessages.Contains(
                "Line 0: Parameter 'DaltonNumber' will not be imported. Valid values are doubles only."));
            Assert.IsTrue(errorMessages.Contains(
                "Line 0: Parameter 'StantonNumber' will not be imported. Valid values are doubles only."));
            Assert.IsTrue(errorMessages.Contains(
                "Line 0: Parameter 'HeatCapacityWater' will not be imported. Valid values are doubles only."));

            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueBackgroundTemperatureDefault, model.BackgroundTemperature);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueSurfaceAreaDefault, model.SurfaceArea);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueAtmosphericPressureDefault, model.AtmosphericPressure);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueDaltonNumberDefault, model.DaltonNumber);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueStantonNumberDefault, model.StantonNumber);
            Assert.AreEqual(WaterFlowModel1DDataSet.Meteo.valueHeatCapacityWaterDefault, model.HeatCapacityWater);
        }

        [Test]
        public void GivenATemperatureCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.TemperatureValuesHeader);
            category.AddProperty("Unknown Property", "unknown");
            category.AddProperty("BackgroundTemperature", "1.0");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelTemperatureSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(
                "Line 0: Parameter 'Unknown Property' found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                errorMessages[0]);
            Assert.AreEqual(1.0, model.BackgroundTemperature);
        }

        [Test]
        public void
            GivenAWronglyDefinedCategoryHeader_WhenSettingThisModelProperty_ThenThisParameterShouldNotBeSetInTheModel()
        {
            const string propertyName = "BackgroundTemperature";

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
