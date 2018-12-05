using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelTimePropertiesSetterTest
    {
        private readonly DateTime defaultStartTime = new DateTime(2018, 11, 30, 15, 15, 0); // 2018-11-30 15:15:00
        private readonly DateTime defaultStopTime = new DateTime(2018, 12, 4, 21, 0, 0); // 2018-12-04 21:00:00
        private readonly TimeSpan defaultTimeStep = new TimeSpan(0, 0, 15, 0); // 15 minutes
        private readonly TimeSpan defaultGridPointsTimeStep = new TimeSpan(0, 0, 10, 0); // 10 minutes
        private readonly TimeSpan defaultStructuresTimeStep = new TimeSpan(0, 0, 5, 0); // 5 minutes

        [Test]
        public void GivenTimeSettingsDataModel_WhenSettingWaterFlowModelTimeProperties_ThenTimeSettingsAreCorrect()
        {
            // Given
            var timeSettingsCategory = GetCorrectTimeSettingsDataModel();

            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, CreateAndAddErrorReport);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(defaultStartTime));
            Assert.That(model.StopTime, Is.EqualTo(defaultStopTime));
            Assert.That(model.TimeStep, Is.EqualTo(defaultTimeStep));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(defaultGridPointsTimeStep));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultStructuresTimeStep));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingStartTime_WhenSettingWaterFlowModelTimeProperties_ThenDefaultStartTimeIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.StartTime.Key);

            // Then
            var defaultDateTime = default(DateTime);
            Assert.That(model.StartTime, Is.EqualTo(defaultDateTime));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingStopTime_WhenSettingWaterFlowModelTimeProperties_ThenDefaultStopTimeIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.StopTime.Key);

            // Then
            var defaultDateTime = default(DateTime);
            Assert.That(model.StopTime, Is.EqualTo(defaultDateTime));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingTimeStep_WhenSettingWaterFlowModelTimeProperties_ThenDefaultTimeStepIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.TimeStep.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.TimeStep, Is.EqualTo(defaultTimeSpan));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingGridOutputTimeStep_WhenSettingWaterFlowModelTimeProperties_ThenDefaultGridOutputTimeStepIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.OutTimeStepGridPoints.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.OutputSettings.GridOutputTimeStep, Is.EqualTo(defaultTimeSpan));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingStructureOutputTimeStep_WhenSettingWaterFlowModelTimeProperties_ThenDefaultStructureOutputTimeStepIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.OutTimeStepStructures.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.OutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultTimeSpan));
        }

        private WaterFlowModel1D SetTimePropertiesWithMissingProperty(string missingPropertyName)
        {
            // Given
            var timeSettingsCategory = GetCorrectTimeSettingsDataModel();
            timeSettingsCategory.Properties.RemoveAllWhere(p => p.Name == missingPropertyName);

            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, CreateAndAddErrorReport);
            return model;
        }

        [Test]
        public void GivenDataModelWithoutTimeCategory_WhenSettingWaterFlowModelTimeProperties_ThenTimeSettingsHaveNotChanged()
        {
            // Given
            var notTimeCategory = new DelftIniCategory("NotTimeSettingsHeader");
            notTimeCategory.AddProperty(ModelDefinitionsRegion.StartTime.Key, defaultStartTime);

            // When
            var model = new WaterFlowModel1D();
            var startTimeBefore = model.StartTime;
            var stopTimeBefore = model.StopTime;
            var timeStepBefore = model.TimeStep;
            var gridPointsTimeStepBefore = model.OutputSettings.GridOutputTimeStep;
            var structuresTimeStepBefore = model.OutputSettings.StructureOutputTimeStep;

            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            new WaterFlowModelTimePropertiesSetter().SetProperties(notTimeCategory, model, CreateAndAddErrorReport);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(startTimeBefore));
            Assert.That(model.StopTime, Is.EqualTo(stopTimeBefore));
            Assert.That(model.TimeStep, Is.EqualTo(timeStepBefore));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(gridPointsTimeStepBefore));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(structuresTimeStepBefore));
        }

        [Test]
        public void WhenSettingWaterFlowModelTimePropertiesWithCategoriesEqualToNull_ThenTimeSettingsHaveNotChanged()
        {
            // When
            var model = new WaterFlowModel1D();
            var startTimeBefore = model.StartTime;
            var stopTimeBefore = model.StopTime;
            var timeStepBefore = model.TimeStep;
            var gridPointsTimeStepBefore = model.OutputSettings.GridOutputTimeStep;
            var structuresTimeStepBefore = model.OutputSettings.StructureOutputTimeStep;

            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            new WaterFlowModelTimePropertiesSetter().SetProperties(null, model, CreateAndAddErrorReport);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(startTimeBefore));
            Assert.That(model.StopTime, Is.EqualTo(stopTimeBefore));
            Assert.That(model.TimeStep, Is.EqualTo(timeStepBefore));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(gridPointsTimeStepBefore));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(structuresTimeStepBefore));
        }

        [Test]
        public void WhenSettingWaterFlowModelTimePropertiesWithoutAModel_ThenNoException()
        {
            TestDelegate testDelegate = () => { new WaterFlowModelTimePropertiesSetter().SetProperties(null, null, null); };
            Assert.DoesNotThrow(testDelegate);
        }

        private DelftIniCategory GetCorrectTimeSettingsDataModel()
        {
            var timeSettingsCategory = new DelftIniCategory(ModelDefinitionsRegion.TimeHeader);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StartTime.Key, defaultStartTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StopTime.Key, defaultStopTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.TimeStep.Key, defaultTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepGridPoints.Key, defaultGridPointsTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepStructures.Key, defaultStructuresTimeStep.TotalSeconds);

            return timeSettingsCategory;
        }
    }
}