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
    public class WaterFlowModelNumericalParametersSetterTest
    {
        [TestCase("AccelerationTermFactor", "2")]
        [TestCase("AccurateVersusSpeed", "4")]
        [TestCase("CourantNumber", "0.9")]
        [TestCase("DtMinimum", "0.003")]
        [TestCase("EpsilonValueVolume", "0.0003")]
        [TestCase("EpsilonValueWaterDepth", "0.0002")]
        [TestCase("MaxDegree", "5")]
        [TestCase("MaxIterations", "9")]
        [TestCase("MinimumSurfaceatStreet", "0.3")]
        [TestCase("MinimumSurfaceinNode", "0.2")]
        [TestCase("MinimumLength", "1.4")]
        [TestCase("RelaxationFactor", "1.3")]
        [TestCase("Rho", "1001")]
        [TestCase("StructureInertiaDampingFactor", "1.2")]
        [TestCase("Theta", "1.1")]
        [TestCase("ThresholdValueFlooding", "0.02")]
        [TestCase("UseTimeStepReducerStructures", "1")]
       public void
            GivenANumericalParameterCategoryWithOneProperty_WhenSettingThisModelProperty_ThenThisParameterShouldBeSetInTheModel(string propertyName, string value)
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.NumericalParametersValuesHeader);

            category.AddProperty(propertyName, value);

            // Create ModelParameters
            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelNumericalParametersSetter()).SetProperties(category, model, errorMessages);

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
            GivenANumericalParameterCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            //Given
            var unsupportedPropertyName = "bla";
            var category = new DelftIniCategory(ModelDefinitionsRegion.NumericalParametersValuesHeader);

            category.AddProperty(unsupportedPropertyName, "2");
            category.AddProperty(ModelDefinitionsRegion.AccelerationTermFactor.Key, 3);


            // Create ModelParameters
            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();
            
            //When
            (new WaterFlowModelNumericalParametersSetter()).SetProperties(category, model, errorMessages);

            //Then
            var expectedMessage = string.Format(
                Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unsupportedPropertyName);
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(expectedMessage, errorMessages[0]);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.AccelerationTermFactor.Key);
            //ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("3", parameterSetting.Value);
        }
    }
}