using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelAdvancedOptionsSetterTest
    {
        [TestCase("ExtraResistanceGeneralStructure", "1.0", "1.0")]
        [TestCase("LateralLocation", "2.0", "2.0")]
        [TestCase("MaxLoweringCrossAtCulvert", "3.0", "3.0")]
        [TestCase("MaxVolFact", "4.0", "4.0")]
        [TestCase("TransitionHeightSD", "5.0", "5.0")]
        [TestCase("FillCulvertsWithGL", "0", "False")]
        [TestCase("FillCulvertsWithGL", "1", "True")]
        [TestCase("NoNegativeQlatWhenThereIsNoWater", "0", "False")]
        [TestCase("NoNegativeQlatWhenThereIsNoWater", "1", "True")]
        public void GivenAnAdvancedOptionsCategoryWithOneProperty_WhenSettingThisModelProperty_ThenThisParameterShouldBeSetInTheModel(string propertyName, string value, string expectedValue)
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, value);

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelAdvancedOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);
            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == propertyName);
            Assert.NotNull(parameterSetting);
            Assert.AreEqual(expectedValue, parameterSetting.Value);
        }

        [Test]
        public void GivenAnAdvancedOptionsCategoryWithLatitude_WhenSettingThisModelProperty_ThenThisPropertyShouldBeSetInTheModel()
        {
            var propertyName = ModelDefinitionsRegion.Latitude.Key;
            const string value = "6.0";
            const double expectedValue = 6.0;

            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, value);

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelAdvancedOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(expectedValue, model.Latitude);
        }

        [Test]
        public void GivenAnAdvancedOptionsCategoryWithLongitude_WhenSettingThisModelProperty_ThenThisPropertyShouldBeSetInTheModel()
        {
            var propertyName = ModelDefinitionsRegion.Longitude.Key;
            const string value = "7.0";
            const double expectedValue = 7.0;

            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, value);

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelAdvancedOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(expectedValue, model.Longitude);
        }

        [TestCase("1", true)]
        [TestCase("0", false)]
        [Test] public void GivenAnAdvancedOptionsCategoryWithCalculateDelwaqOutput_WhenSettingThisModelProperty_ThenThisPropertyShouldBeSetInTheModel(string value, bool expectedValue)
        {
            var propertyName = ModelDefinitionsRegion.CalculateDelwaqOutput.Key;

            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, value);

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelAdvancedOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(expectedValue, model.HydFileOutput);
        }

        [Test]
        public void GivenAnAdvancedOptionsCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            //Given
            var unknownPropertyName = "Unknown Property";
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(unknownPropertyName, "unknown");
            category.AddProperty(ModelDefinitionsRegion.ExtraResistanceGeneralStructure.Key, "1.0");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelAdvancedOptionsSetter().SetProperties(category, model, errorMessages);

            //Then
            var expectedMessage = string.Format(
                Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(expectedMessage, errorMessages[0]);
            var parameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.ExtraResistanceGeneralStructure.Key);
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("1.0", parameterSetting.Value);
        }
    }
}
