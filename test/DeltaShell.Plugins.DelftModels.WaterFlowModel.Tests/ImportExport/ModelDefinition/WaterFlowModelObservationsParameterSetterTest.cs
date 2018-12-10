using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
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
            var category = new DelftIniCategory(ModelDefinitionsRegion.ObservationsHeader);

            category.AddProperty("bla", "2");
            category.AddProperty(ModelDefinitionsRegion.InterpolationType.Key, "Nearest");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

           // When
            new WaterFlowModelObservationsParameterSetter().SetProperties(category, model,
                errorMessages);

            // Then
            Assert.AreEqual(
                "Line 0: Parameter bla found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                errorMessages[0]);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.InterpolationType.Key);
            // ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("Nearest", parameterSetting.Value);
        }
    }
}
