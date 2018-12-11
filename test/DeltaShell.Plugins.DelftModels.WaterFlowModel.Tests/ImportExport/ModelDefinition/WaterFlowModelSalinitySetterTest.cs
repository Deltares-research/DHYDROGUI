using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelSalinitySetterTest
    {
        [Test]
        public void GivenACategoryWithSalinityPropertiesWithValuesTrue_WhenSettingTheseModelProperties_ThenTheseParametersShouldBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.SalinityValuesHeader);

            category.AddProperty(ModelDefinitionsRegion.SaltComputation.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.DiffusionAtBoundaries.Key, "1");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelSalinitySetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(true, model.UseSaltInCalculation);

            var diffusionAtBoundariesParameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);

            Assert.NotNull(diffusionAtBoundariesParameterSetting);
            Assert.AreEqual(true.ToString(), diffusionAtBoundariesParameterSetting.Value);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void GivenACategoryWithSalinityPropertiesWithValuesFalse_WhenSettingTheseModelProperties_ThenTheseParametersShouldBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.SalinityValuesHeader);

            category.AddProperty(ModelDefinitionsRegion.SaltComputation.Key, "0");
            category.AddProperty(ModelDefinitionsRegion.DiffusionAtBoundaries.Key, "0");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelSalinitySetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(false, model.UseSaltInCalculation);

            var diffusionAtBoundariesParameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);

            Assert.NotNull(diffusionAtBoundariesParameterSetting);
            Assert.AreEqual(false.ToString(), diffusionAtBoundariesParameterSetting.Value);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void GivenACategoryWithSalinityPropertiesWithUnknownProperty_WhenSettingTheseModelProperties_ThenAWarningShouldBeGiven()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.SalinityValuesHeader);

            category.AddProperty(ModelDefinitionsRegion.SaltComputation.Key, "0");
            category.AddProperty(ModelDefinitionsRegion.DiffusionAtBoundaries.Key, "0");
            category.AddProperty("bla", "0");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();
            
            //When
            (new WaterFlowModelSalinitySetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(false, model.UseSaltInCalculation);

            var diffusionAtBoundariesParameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);

            Assert.NotNull(diffusionAtBoundariesParameterSetting);
            Assert.AreEqual(false.ToString(), diffusionAtBoundariesParameterSetting.Value);

            Assert.AreEqual(1,errorMessages.Count);
            Assert.AreEqual("Line 0: Unknown property bla found in salinity category", errorMessages[0]);
        }

        [Test]
        public void GivenACategoryWithSalinityPropertiesWithPropertyWithNotValidValue_WhenSettingTheseModelProperties_ThenAWarningShouldBeGiven()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.SalinityValuesHeader);

            category.AddProperty(ModelDefinitionsRegion.SaltComputation.Key, "0");
            category.AddProperty(ModelDefinitionsRegion.DiffusionAtBoundaries.Key, "bla");
            
            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            var diffusionAtBoundariesBefore = model.ParameterSettings.FirstOrDefault(
                ps => ps.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);

            Assert.NotNull(diffusionAtBoundariesBefore);

            var diffusionAtBoundariesBeforeValue = diffusionAtBoundariesBefore.Value;

            //When
            (new WaterFlowModelSalinitySetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(false, model.UseSaltInCalculation);

            var diffusionAtBoundariesParameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);

            Assert.NotNull(diffusionAtBoundariesParameterSetting);
            Assert.AreEqual(diffusionAtBoundariesBeforeValue, diffusionAtBoundariesParameterSetting.Value);

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Line 0: Input string was not in a correct format.", errorMessages[0]);
        }
    }
}