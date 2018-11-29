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
        // SetTimeProperties
        private readonly DateTime defaultStartTime = new DateTime(2018, 11, 30, 15, 15, 0); // 2018-11-30 15:15:00
        private readonly DateTime defaultStopTime = new DateTime(2018, 12, 4, 21, 0, 0); // 2018-12-04 21:00:00
        private readonly TimeSpan defaultTimeStep = new TimeSpan(0, 0, 15, 0); // 15 minutes
        private readonly TimeSpan defaultGridPointsTimeStep = new TimeSpan(0, 0, 10, 0); // 10 minutes
        private readonly TimeSpan defaultStructuresTimeStep = new TimeSpan(0, 0, 5, 0); // 5 minutes

        [Test]
        public void GivenTimeSettingsDataModel_WhenSettingWaterFlowModelProperties_ThenTimeSettingsAreCorrect()
        {
            // Given
            var timeSettingsCategories = GetCorrectTimeSettingsDataModel();

            // When
            var model = new WaterFlowModel1D();
            WaterFlowModelPropertySetter.SetTimeProperties(timeSettingsCategories, model);

            // Then
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

        [Test]
        public void GivenDataModelWithoutTimeCategory_WhenSettingWaterFlowModelProperties_ThenTimeSettingsHaveNotChanged()
        {
            // Given
            var notTimeCategory = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            notTimeCategory.AddProperty(ModelDefinitionsRegion.StartTime.Key, defaultStartTime);
            var categories = new List<DelftIniCategory> {notTimeCategory};

            // When
            var model = new WaterFlowModel1D();
            var startTimeBefore = model.StartTime;
            var stopTimeBefore = model.StopTime;
            var timeStepBefore = model.TimeStep;
            var gridPointsTimeStepBefore = model.OutputSettings.GridOutputTimeStep;
            var structuresTimeStepBefore = model.OutputSettings.StructureOutputTimeStep;

            WaterFlowModelPropertySetter.SetTimeProperties(categories, model);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(startTimeBefore));
            Assert.That(model.StopTime, Is.EqualTo(stopTimeBefore));
            Assert.That(model.TimeStep, Is.EqualTo(timeStepBefore));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(gridPointsTimeStepBefore));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(structuresTimeStepBefore));
        }

        [Test]
        public void WhenSettingWaterFlowModelPropertiesWithCategoriesEqualToNull_ThenTimeSettingsHaveNotChanged()
        {
            // When
            var model = new WaterFlowModel1D();
            var startTimeBefore = model.StartTime;
            var stopTimeBefore = model.StopTime;
            var timeStepBefore = model.TimeStep;
            var gridPointsTimeStepBefore = model.OutputSettings.GridOutputTimeStep;
            var structuresTimeStepBefore = model.OutputSettings.StructureOutputTimeStep;

            WaterFlowModelPropertySetter.SetTimeProperties(null, model);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(startTimeBefore));
            Assert.That(model.StopTime, Is.EqualTo(stopTimeBefore));
            Assert.That(model.TimeStep, Is.EqualTo(timeStepBefore));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(gridPointsTimeStepBefore));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(structuresTimeStepBefore));
        }

        [Test]
        public void WhenSettingWaterFlowModelPropertiesWithoutAModel_ThenNoException()
        {
            try
            {
                WaterFlowModelPropertySetter.SetTimeProperties(null, null);
            }
            catch (Exception e)
            {
                Assert.Fail("No exception was expected, but got: " + e.Message);
            }
        }
    }
}