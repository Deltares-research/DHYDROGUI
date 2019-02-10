using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    class WaterFlowModelMorphologySetterTest
    {
        [TestCase("0", false)]
        [TestCase("1", true)]
        public void GivenAMorphologyCategoryWithProperties_WhenSettingTheseModelProperties_ThenPropertiesdShouldBeSetInModel(string value, bool expectedValue)
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.MorphologyValuesHeader);
            category.AddProperty("CalculateMorphology", value);
            category.AddProperty("AdditionalOutput", value);
            category.AddProperty("SedimentInputFile", "SedimentFilePath");
            category.AddProperty("MorphologyInputFile", "MorphologyFilePath");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelMorphologySetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(expectedValue, model.UseMorphology);
            Assert.AreEqual(expectedValue, model.AdditionalMorphologyOutput);
            Assert.AreEqual("SedimentFilePath", model.SedimentPath);
            Assert.AreEqual("MorphologyFilePath", model.MorphologyPath);
        }

        [Test]
        public void GivenAMorphologyCategoryWithPropertiesWithInvalidValues_WhenSettingTheseModelProperties_ThenAnErrorMessageShouldBeReported()
        {
            var propertyName = "CalculateMorphology";
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.MorphologyValuesHeader);
            category.AddProperty(propertyName, "yes");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelMorphologySetter().SetProperties(category, model, errorMessages);

            //Then
            Assert.AreEqual(1, errorMessages.Count);
            var expectedMessage = 
                string.Format(Resources.WaterFlowModelMorphologySetter_ParseValueToBool_Line__0___Parameter___1___will_not_be_imported__Valid_values_are__0___false__or__1___true__,
                0, propertyName);
            Assert.AreEqual(expectedMessage, errorMessages[0]);
            Assert.AreEqual(false, model.UseMorphology);
        }

        [Test]
        public void GivenAMorphologyCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            var unknownPropertyName = "Unknown Property";

            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.MorphologyValuesHeader);
            category.AddProperty(unknownPropertyName, "unknown");
            category.AddProperty("CalculateMorphology", "1");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            //When
            new WaterFlowModelMorphologySetter().SetProperties(category, model, errorMessages);

            //Then
            var expectedMessage = string.Format(
                Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual(expectedMessage, errorMessages[0]);
            Assert.AreEqual(true, model.UseMorphology);
        }

        [Test]
        public void
            GivenAWronglyDefinedCategoryHeader_WhenSettingThisModelProperty_ThenThisParameterShouldNotBeSetInTheModel()
        {
            const string propertyName = "CalculateMorphology";

            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(propertyName, "1");

            var model = new WaterFlowModel1D();
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelMorphologySetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(false, model.UseMorphology);
        }
    }
}
