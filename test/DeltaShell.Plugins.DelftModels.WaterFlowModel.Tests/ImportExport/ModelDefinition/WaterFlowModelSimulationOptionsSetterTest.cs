using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
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
            new WaterFlowModelSimulationOptionsSetter().SetProperties(category, model,  errorMessages);

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
            var unknownPropertyName = "bla2";
            var category = new DelftIniCategory(ModelDefinitionsRegion.SimulationOptionsValuesHeader);
            category.AddProperty(unknownPropertyName, 2);
            category.AddProperty(ModelDefinitionsRegion.AccelerationTermFactor.Key, "3");

            // Create ModelParameters
            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelSimulationOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            var expectedMessage = string.Format(
                Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(expectedMessage, errorMessages[0]);

            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.AccelerationTermFactor.Key);

            //ParameterSetting can never be null here, because in this situation the error report has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("3", parameterSetting.Value);
        }

        [Test]
        [TestCase(true, true, TestName = "UseRestart and WriteRestart are true")]
        [TestCase(false, false, TestName = "UseRestart and WriteRestart are false")]
        [TestCase(true, false, TestName = "UseRestart is true and WriteRestart is false")]
        [TestCase(false, true, TestName = "UseRestart is false and WriteRestart is true")]
        public void
            GivenAnOldFormatSimulationOptionsCategoryWithRestartSettings_WhenSettingTheseModelProperties_ThenSaveStateStartTimeAndSaveStateStopTimeShouldBeEqualToModelStopTimeIndependentOfUseRestartAndWriteRestartSettings(bool useRestart, bool writeRestart)
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.SimulationOptionsValuesHeader);
            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, writeRestart ? "1" : "0");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, useRestart ? "1" : "0");

            // Create ModelParameters
            var startTime = DateTime.Today;
            var stopTime = startTime.AddDays(2);
            var timeStep = TimeSpan.FromSeconds(3);

            // Create ModelParameters
            var model = new WaterFlowModel1D()
            {
                StartTime = startTime,
                StopTime = stopTime,
                TimeStep = timeStep,
            };

            var errorMessages = new List<string>();

            //When
            new WaterFlowModelSimulationOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);
            
            Assert.AreEqual(useRestart, model.UseRestart);
            Assert.AreEqual(writeRestart, model.WriteRestart);

            Assert.AreEqual(stopTime, model.SaveStateStartTime);
            Assert.AreEqual(stopTime, model.SaveStateStopTime);
            Assert.AreEqual(timeStep, model.SaveStateTimeStep);
        }
    }
}