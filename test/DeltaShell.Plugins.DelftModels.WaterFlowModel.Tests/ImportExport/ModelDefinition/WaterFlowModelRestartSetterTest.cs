using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelRestartSetterTest
    {
        [Test]
        public void GivenARestartCategoryWithoutSaveStateTimeRangePropertiesAndBothPropertiesFalse_WhenSettingTheseModelProperties_ThenTheseParametersShouldBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);

            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "0");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "0");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelRestartSetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(model.UseRestart, false);
            Assert.AreEqual(model.WriteRestart, false);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void GivenARestartCategoryWithoutSaveStateTimeRangePropertiesAndBothPropertiesTrue_WhenSettingTheseModelProperties_ThenTheseParametersShouldBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);

            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "1");
            

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelRestartSetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(model.UseRestart, true);
            Assert.AreEqual(model.WriteRestart, true);

            Assert.AreEqual(0, errorMessages.Count);
        }

        [Test]
        public void GivenARestartCategoryWithSaveStateTimeRangeProperties_WhenSettingTheseModelProperties_ThenTheseParametersShouldAlsoBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);

            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "0");

            category.AddProperty(ModelDefinitionsRegion.RestartStartTime.Key, "2014 - 01 - 01 00:00:00");
            category.AddProperty(ModelDefinitionsRegion.RestartStopTime.Key, "2014-01-02 00:00:00");
            category.AddProperty(ModelDefinitionsRegion.RestartTimeStep.Key, "16");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelRestartSetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(model.UseRestart, false);
            Assert.AreEqual(model.WriteRestart, true);

            Assert.AreEqual(0, errorMessages.Count);
            Assert.AreEqual(TimeSpan.FromSeconds(Convert.ToDouble("16")), model.SaveStateTimeStep);
            Assert.AreEqual(DateTime.Parse("2014 - 01 - 01 00:00:00"), model.SaveStateStartTime);
            Assert.AreEqual(DateTime.Parse("2014 - 01 - 02 00:00:00"), model.SaveStateStopTime);
        }

        [Test]
        public void GivenARestartCategoryWithSaveStateTimeRangePropertiesButOneIsMissing_WhenSettingTheseModelProperties_ThenTheseParametersShouldNotBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);

            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "0");

            category.AddProperty(ModelDefinitionsRegion.RestartStartTime.Key, "2014 - 01 - 01 00:00:00");
            category.AddProperty(ModelDefinitionsRegion.RestartStopTime.Key, "2014-01-02 00:00:00");
            
            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelRestartSetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(model.UseRestart, false);
            Assert.AreEqual(model.WriteRestart, true);

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Line 0: Information about the Save State is not complete and therefore ignored during import", errorMessages[0]);
        }

        [Test]
        public void GivenARestartCategoryWithSaveStateTimeRangePropertiesButOneIsMissing2_WhenSettingTheseModelProperties_ThenTheseParametersShouldNotBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);

            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "0");

            category.AddProperty(ModelDefinitionsRegion.RestartStartTime.Key, "2014 - 01 - 01 00:00:00");
            category.AddProperty(ModelDefinitionsRegion.RestartTimeStep.Key, "16");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelRestartSetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(model.UseRestart, false);
            Assert.AreEqual(model.WriteRestart, true);

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Line 0: Information about the Save State is not complete and therefore ignored during import", errorMessages[0]);
        }

        [Test]
        public void GivenARestartCategoryWithSaveStateTimeRangePropertiesButOneIsMissing3_WhenSettingTheseModelProperties_ThenTheseParametersShouldNotBeSetInTheModel()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);

            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "0");

            category.AddProperty(ModelDefinitionsRegion.RestartStopTime.Key, "2014-01-02 00:00:00");
            category.AddProperty(ModelDefinitionsRegion.RestartTimeStep.Key, "16");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelRestartSetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(model.UseRestart, false);
            Assert.AreEqual(model.WriteRestart, true);

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Line 0: Information about the Save State is not complete and therefore ignored during import", errorMessages[0]);
        }

        [Test]
        public void GivenARestartCategoryWithAnUnknownProperty_WhenSettingTheseModelProperties_ThenAllPropertiesShouldBeSetExceptTheUnknownProperty()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.RestartHeader);

            category.AddProperty(ModelDefinitionsRegion.WriteRestart.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.UseRestart.Key, "0");
            category.AddProperty("bla", "0");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorMessages = new List<string>();

            //When
            (new WaterFlowModelRestartSetter()).SetProperties(category, model, errorMessages);

            Assert.AreEqual(model.UseRestart, false);
            Assert.AreEqual(model.WriteRestart, true);

            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Line 0: Parameter bla found. This parameter will not be imported, since it is not supported by the GUI", errorMessages[0]);
        }
    }
}