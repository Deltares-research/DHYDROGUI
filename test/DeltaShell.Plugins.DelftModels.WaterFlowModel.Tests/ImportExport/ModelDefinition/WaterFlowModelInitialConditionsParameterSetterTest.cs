using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelInitialConditionsParameterSetterTest
    {
        [Test]
        public void
            GivenAnInitialConditionCategoryWithOneProperty_WhenSettingThisModelProperty_ThenThisParameterShouldBeSetInTheModel()
        {
            const string propertyName = "InitialEmptyWells";
           
            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.InitialConditionsValuesHeader);
            category.AddProperty(propertyName, "1");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelInitialConditionsParameterSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.AreEqual(0, errorMessages.Count);

            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == propertyName);

            // ParameterSetting can never be null here, because in this situation the error report has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("True", parameterSetting.Value);
        }

        [Test]
        public void
            GivenAWronglyDefinedCategoryHeader_WhenSettingThisModelProperty_ThenThisParameterShouldNotBeSetInTheModel()
        {
            const string propertyName = "InitialEmptyWells";

            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, "1");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelInitialConditionsParameterSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.AreEqual(0, errorMessages.Count);

            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == propertyName);

            // ParameterSetting can never be null here, because in this situation the error report has also a message.
            Assert.NotNull(parameterSetting);
            // parameter is not set to true.
            Assert.AreEqual("false", parameterSetting.Value);
        }

        [Test]
        public void
            GivenANumericalParameterCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.InitialConditionsValuesHeader);

            category.AddProperty("bla", "2");
            category.AddProperty(ModelDefinitionsRegion.InitialEmptyWells.Key, 0);

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelInitialConditionsParameterSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(
                "Line 0: Parameter bla found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                errorMessages[0]);

            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.InitialEmptyWells.Key);

            // ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("False", parameterSetting.Value);
        }
    }
}
