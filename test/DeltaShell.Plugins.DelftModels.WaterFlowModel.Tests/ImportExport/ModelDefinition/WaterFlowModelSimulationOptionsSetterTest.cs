using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelSimulationOptionsSetterTest
    {
        [TestCase("Debug", "1")]
        [TestCase("DebugTime", "1")]
        [TestCase("DispMaxFactor", "0.85")]
        [TestCase("DumpInput", "1")]
        [TestCase("Iadvec1D", "3")]
        [TestCase("Limtyphu1D", "4")]
        [TestCase("TimersOutputFrequency", "2")]
        [TestCase("UseTimers", "1")]
       public void
           GivenASimulationOptionsCategoryWithOneProperty_WhenSettingThisModelProperty_ThenThisParameterShouldBeSetInTheModel(string propertyName, string value)
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.SimulationOptionsValuesHeader);
            category.AddProperty(propertyName, value);
            
            // Create ModelParameters
            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelSimulationOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == propertyName);
            //ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            
            if (parameterSetting.Type == "typeof(bool)")
            {
                var value2 = Convert.ToBoolean(Convert.ToInt32(value));
                Assert.AreEqual(value2.ToString(), parameterSetting.Value);
            }
            else
            {
                Assert.AreEqual(value, parameterSetting.Value);
            }
        }

        [Test]
        public void
            GivenASimulationOptionsCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.SimulationOptionsValuesHeader);
            category.AddProperty("bla2", 2);
            category.AddProperty(ModelDefinitionsRegion.AccelerationTermFactor.Key, "3");

            // Create ModelParameters
            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelSimulationOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(
                "Line 0: Parameter bla2 found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                errorMessages[0]);

            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.AccelerationTermFactor.Key);

            //ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("3", parameterSetting.Value);
        }

        [Test]
        public void
            GivenASimulationOptionsCategoryWithRestartSettings_WhenSettingTheseModelProperties_ThenTheseParametersShouldBeSetInTheModel()
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.SimulationOptionsValuesHeader);
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "1");

            // Create ModelParameters
            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelSimulationOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);

            Assert.AreEqual(true, model.UseRestart);
            Assert.AreEqual(true, model.WriteRestart);
        }
    }
}