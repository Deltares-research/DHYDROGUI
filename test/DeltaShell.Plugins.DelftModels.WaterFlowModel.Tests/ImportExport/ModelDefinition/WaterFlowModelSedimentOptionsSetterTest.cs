using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelSedimentOptionsSetterTest
    {
        [Test]
        public void GivenACategoryWithSedimentProperties_WhenSettingTheseModelProperties_ThenTheseParametersShouldBeSetInTheModel()
        {
            // Given
            const string d50 = "0.0005";
            const string d90 = "0.001";
            const string depthUsedForSediment = "0.3";

            var category = new DelftIniCategory(ModelDefinitionsRegion.SedimentValuesHeader);
            category.AddProperty(ModelDefinitionsRegion.D50.Key, d50);
            category.AddProperty(ModelDefinitionsRegion.D90.Key, d90);
            category.AddProperty(ModelDefinitionsRegion.DepthUsedForSediment.Key, depthUsedForSediment);
           
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            // When 
            new WaterFlowModelSedimentOptionsSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.NotNull(model.D50);
            Assert.NotNull(model.D90);
            Assert.NotNull(model.DepthUsedForSediment);
            Assert.AreEqual(0.0005, model.D50);
            Assert.AreEqual(0.001, model.D90);
            Assert.AreEqual(0.3, model.DepthUsedForSediment);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void GivenACategoryWithSedimentProperties_WhenTryingToSetThesePropertiesOnTheWrongCategory_ThenTheseParametersShouldNotBeSetInTheModel()
        {
            // Given
            const string d50 = "0.0005";
            const string d90 = "0.001";
            const string depthUsedForSediment = "0.3";

            var category = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            category.AddProperty(ModelDefinitionsRegion.D50.Key, d50);
            category.AddProperty(ModelDefinitionsRegion.D90.Key, d90);
            category.AddProperty(ModelDefinitionsRegion.DepthUsedForSediment.Key, depthUsedForSediment);

            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            // When 
            new WaterFlowModelSedimentOptionsSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.Null(model.D50);
            Assert.Null(model.D90);
            Assert.Null(model.DepthUsedForSediment);
        }

        [Test]
        public void GivenACategoryWithOneEmptySedimentProperty_WhenSettingTheseModelProperties_ThenThisParameterIsNotSetButTheOthersAreSet()
        {
            // Given
            const string d50 = "0.0005";
            var d90 = string.Empty;
            const string depthUsedForSediment = "0.3";

            var category = new DelftIniCategory(ModelDefinitionsRegion.SedimentValuesHeader);
            category.AddProperty(ModelDefinitionsRegion.D50.Key, d50);
            category.AddProperty(ModelDefinitionsRegion.D90.Key, d90);
            category.AddProperty(ModelDefinitionsRegion.DepthUsedForSediment.Key, depthUsedForSediment);

            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            // When 
            new WaterFlowModelSedimentOptionsSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.NotNull(model.D50);
            Assert.Null(model.D90);
            Assert.NotNull(model.DepthUsedForSediment);
            Assert.AreEqual(0.0005, model.D50);
            Assert.AreEqual(null, model.D90);
            Assert.AreEqual(0.3, model.DepthUsedForSediment);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void GivenACategoryWithAnInvalidProperty_WhenSettingTheseModelProperties_ThenAWarningShouldBeGiven()
        {
            // Given
            const string d50 = "0.0005";
            var category = new DelftIniCategory(ModelDefinitionsRegion.SedimentValuesHeader);
            category.AddProperty(ModelDefinitionsRegion.D50.Key, d50);
            category.AddProperty(ModelDefinitionsRegion.DelwaqNoStaggeredGrid.Key, "ItsOver9000!");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();
            
            // When
            new WaterFlowModelSedimentOptionsSetter().SetProperties(category, model, errorMessages);

            var d50ParameterSetting = model.ParameterSettings.FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.D50.Key);
 
            // Then
            Assert.NotNull(d50ParameterSetting);
            Assert.AreEqual(d50, d50ParameterSetting.Value);
            
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual($"Line 0: Parameter '{ModelDefinitionsRegion.DelwaqNoStaggeredGrid.Key}' found in the md1d file. This parameter will not be imported, since it is not supported by the GUI", errorMessages[0]);
        }
    }
}
