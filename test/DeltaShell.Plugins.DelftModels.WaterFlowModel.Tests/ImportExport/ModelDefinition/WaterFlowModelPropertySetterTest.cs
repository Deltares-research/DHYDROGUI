using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelPropertySetterTest
    {
        private readonly DateTime defaultStartTime = new DateTime(2018, 11, 30, 15, 15, 0); // 2018-11-30 15:15:00
        private readonly DateTime defaultStopTime = new DateTime(2018, 12, 4, 21, 0, 0); // 2018-12-04 21:00:00
        private readonly TimeSpan defaultTimeStep = new TimeSpan(0, 0, 15, 0); // 15 minutes
        private readonly TimeSpan defaultGridPointsTimeStep = new TimeSpan(0, 0, 10, 0); // 10 minutes
        private readonly TimeSpan defaultStructuresTimeStep = new TimeSpan(0, 0, 5, 0); // 5 minutes

        [Test]
        public void GivenTimeSettingsDataModel_WhenSettingWaterFlowModelProperties_ThenTimeSettingsAreCorrect()
        {
            var modelSettingsCategories = GetCorrectTimeSettingsDataModel();

            var model = new WaterFlowModel1D();
            WaterFlowModelPropertySetter.SetProperties(modelSettingsCategories, model);

            Assert.That(model.StartTime, Is.EqualTo(defaultStartTime));
            Assert.That(model.StopTime, Is.EqualTo(defaultStopTime));
            Assert.That(model.TimeStep, Is.EqualTo(defaultTimeStep));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(defaultGridPointsTimeStep));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultStructuresTimeStep));
        }

        private IEnumerable<DelftIniCategory> GetCorrectTimeSettingsDataModel()
        {
            var timeSettingsCategory = new DelftIniCategory(ModelDefinitionsRegion.TimeHeader);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StartTime.Key, defaultStartTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StopTime.Key, defaultStopTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.TimeStep.Key, defaultTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepGridPoints.Key, defaultGridPointsTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepStructures.Key, defaultStructuresTimeStep.TotalSeconds);
            return new List<DelftIniCategory> {timeSettingsCategory};
        }
    }
}