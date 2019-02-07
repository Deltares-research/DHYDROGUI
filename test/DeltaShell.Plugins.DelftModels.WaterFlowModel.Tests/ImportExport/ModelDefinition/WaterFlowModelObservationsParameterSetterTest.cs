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
    public class WaterFlowModelObservationsParameterSetterTest
    {
        [TestCase("InterpolationType", "Linear")]
        [TestCase("InterpolationType", "Nearest")]
        public void
            GivenAnInitialConditionCategoryWithOneProperty_WhenSettingThisModelProperty_ThenThisParameterShouldBeSetInTheModel(string propertyName, string value)
        {
            
            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.ObservationsHeader);
            category.AddProperty(propertyName, value);

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelObservationsParameterSetter().SetProperties(category, model,
                errorMessages);

            // Then
            Assert.AreEqual(0, errorMessages.Count);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == propertyName);
            // ParameterSetting can never be null here, because in this situation the error report has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual(value, parameterSetting.Value);
        }

        [Test]
        public void
            GivenAWronglyDefinedCategoryHeader_WhenSettingThisModelProperty_ThenThisParameterShouldNotBeSetInTheModel()
        {
            const string propertyName = "InterpolationType";

            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, "Linear");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelObservationsParameterSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.AreEqual(0, errorMessages.Count);

            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == propertyName);

            // ParameterSetting can never be null here, because in this situation the error report has also a message.
            Assert.NotNull(parameterSetting);
            // parameter is not set to Linear.
            Assert.AreEqual("Nearest", parameterSetting.Value);
        }

        [Test]
        public void
            GivenANumericalParameterCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            // Given
            var unknownPropertyName = "bla";
            var category = new DelftIniCategory(ModelDefinitionsRegion.ObservationsHeader);

            category.AddProperty(unknownPropertyName, "2");
            category.AddProperty(ModelDefinitionsRegion.InterpolationType.Key, "Nearest");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

           // When
            new WaterFlowModelObservationsParameterSetter().SetProperties(category, model,
                errorMessages);

            // Then
            var expectedMessage = string.Format(
                Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.AreEqual(expectedMessage, errorMessages[0]);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.InterpolationType.Key);
            // ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("Nearest", parameterSetting.Value);
        }
    }
}
