using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
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

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, new List<string>());

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
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.MapOutputTimeStep.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.OutputSettings.GridOutputTimeStep, Is.EqualTo(defaultTimeSpan));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingStructureOutputTimeStep_WhenSettingWaterFlowModelTimeProperties_ThenDefaultStructureOutputTimeStepIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.HisOutputTimeStep.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.OutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultTimeSpan));
        }

        private WaterFlowModel1D SetTimePropertiesWithMissingProperty(string missingPropertyName)
        {
            // Given
            var timeSettingsCategory = GetCorrectTimeSettingsDataModel();
            timeSettingsCategory.Properties.RemoveAllWhere(p => p.Name == missingPropertyName);

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, new List<string>());
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

            new WaterFlowModelTimePropertiesSetter().SetProperties(notTimeCategory, model, new List<string>());

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

            new WaterFlowModelTimePropertiesSetter().SetProperties(null, model, new List<string>());

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

        /// <summary>
        /// GIVEN a DataModel with previous MapOutputTimeStep
        /// WHEN setting the WaterFlowModel time properties
        /// THEN the settings are correct
        ///  AND a warning for using deprecated properties is logged
        /// </summary>
        [Test]
        public void GivenADataModelWithPreviousMapOutputTimeStep_WhenSettingTheWaterFlowModelTimeProperties_ThenTheSettingsAreCorrectAndAWarningForUsingDeprecatedPropertiesIsLogged()
        {
            // Given
            var timeSettingsCategory = GetCorrectTimeSettingsDataModel();
            timeSettingsCategory.Properties.RemoveAllWhere(p => p.Name == ModelDefinitionsRegion.MapOutputTimeStep.Key);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepGridPoints, defaultGridPointsTimeStep.TotalSeconds);

            var errorMessages = new List<string>();

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, errorMessages);

            // Then
            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(defaultGridPointsTimeStep), $"Expected {ModelDefinitionsRegion.MapOutputTimeStep.Key} to have a different value:");

            Assert.That(errorMessages.Count, Is.EqualTo(1), "Expected only a single warning when reading a deprecated property.");

            var expectedErrorMessage = $"{ModelDefinitionsRegion.OutTimeStepGridPoints.Key} has been deprecated and will be replaced with {ModelDefinitionsRegion.MapOutputTimeStep.Key} upon saving.";
            Assert.That(errorMessages, Has.Member(expectedErrorMessage), $"Expected a different error message when reading {ModelDefinitionsRegion.OutTimeStepGridPoints.Key}:");
        }

        /// <summary>
        /// GIVEN a DataModel with previous HisOutputTimeStep
        /// WHEN setting the WaterFlowModel time properties
        /// THEN the settings are correct
        ///  AND a warning for using deprecated properties is logged
        /// </summary>
        [Test]
        public void GivenADataModelWithPreviousHisOutputTimeStep_WhenSettingTheWaterFlowModelTimeProperties_ThenTheSettingsAreCorrectAndAWarningForUsingDeprecatedPropertiesIsLogged()
        {
            // Given
            var timeSettingsCategory = GetCorrectTimeSettingsDataModel();
            timeSettingsCategory.Properties.RemoveAllWhere(p => p.Name == ModelDefinitionsRegion.HisOutputTimeStep.Key);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepStructures, defaultStructuresTimeStep.TotalSeconds);

            var errorMessages = new List<string>();

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, errorMessages);

            // Then
            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultStructuresTimeStep), $"Expected {ModelDefinitionsRegion.HisOutputTimeStep.Key} to have a different value:");

            Assert.That(errorMessages.Count, Is.EqualTo(1), "Expected only a single warning when reading a deprecated property.");

            var expectedErrorMessage = $"{ModelDefinitionsRegion.OutTimeStepStructures.Key} has been deprecated and will be replaced with {ModelDefinitionsRegion.HisOutputTimeStep.Key} upon saving.";
            Assert.That(errorMessages, Contains.Item(expectedErrorMessage), $"Expected a different error message when reading {ModelDefinitionsRegion.OutTimeStepStructures.Key}:");
        }

        /// <summary>
        /// GIVEN a DataModel with previous MapOutputTimeStep and MapOutputTimeStep
        /// WHEN setting the WaterFlowModel time properties
        /// THEN the settings are correct
        ///  AND a warning for using two similar values
        /// </summary>
        [Test]
        public void GivenADataModelWithPreviousMapOutputTimeStepAndMapOutputTimeStep_WhenSettingTheWaterFlowModelTimeProperties_ThenTheSettingsAreCorrectAndAWarningForUsingTwoSimilarValues()
        {
            // Given
            var timeSettingsCategory = GetCorrectTimeSettingsDataModel();
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepGridPoints, 50.0);

            var errorMessages = new List<string>();

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, errorMessages);

            // Then
            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(defaultGridPointsTimeStep), $"Expected {ModelDefinitionsRegion.MapOutputTimeStep.Key} to have a different value:");

            Assert.That(errorMessages.Count, Is.EqualTo(1), "Expected only a single warning when reading a two similar properties.");

            var expectedErrorMessage = $"Detected both {ModelDefinitionsRegion.MapOutputTimeStep.Key} and deprecated {ModelDefinitionsRegion.OutTimeStepGridPoints.Key}, using {ModelDefinitionsRegion.MapOutputTimeStep.Key}, {ModelDefinitionsRegion.OutTimeStepGridPoints.Key} will be removed upon saving.";
            Assert.That(errorMessages, Contains.Item(expectedErrorMessage), $"Expected a different error message when reading {ModelDefinitionsRegion.OutTimeStepGridPoints.Key} and {ModelDefinitionsRegion.MapOutputTimeStep.Key}:");
        }

        /// <summary>
        /// GIVEN a DataModel with previous HisOutputTimeStep and HisOutputTimeStep
        /// WHEN setting the WaterFlowModel time properties
        /// THEN the settings are correct
        ///  AND a warning for using two similar values
        /// </summary>
        [Test]
        public void GivenADataModelWithPreviousHisOutputTimeStepAndHisOutputTimeStep_WhenSettingTheWaterFlowModelTimeProperties_ThenTheSettingsAreCorrectAndAWarningForUsingTwoSimilarValues()
        {
            // Given
            var timeSettingsCategory = GetCorrectTimeSettingsDataModel();
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepStructures, 50.0);

            var errorMessages = new List<string>();

            // When
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(timeSettingsCategory, model, errorMessages);

            // Then
            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultStructuresTimeStep), $"Expected {ModelDefinitionsRegion.HisOutputTimeStep.Key} to have a different value:");

            Assert.That(errorMessages.Count, Is.EqualTo(1), "Expected only a single warning when reading a two similar properties.");

            var expectedErrorMessage = $"Detected both {ModelDefinitionsRegion.HisOutputTimeStep.Key} and deprecated {ModelDefinitionsRegion.OutTimeStepStructures.Key}, using {ModelDefinitionsRegion.HisOutputTimeStep.Key}, {ModelDefinitionsRegion.OutTimeStepStructures.Key} will be removed upon saving.";
            Assert.That(errorMessages, Contains.Item(expectedErrorMessage), $"Expected a different error message when reading {ModelDefinitionsRegion.OutTimeStepStructures.Key} and {ModelDefinitionsRegion.HisOutputTimeStep.Key}:");
        }

        [Test]
        public void GivenTimeDataModelWithUnknownProperty_WhenSettingModelProperties_ThenUnknownPropertyIsSkippedAndErrorMessageIsReturned()
        {
            // Given
            const string unknownPropertyName = "UnknownProperty";
            var category = GetCorrectTimeSettingsDataModel();
            category.AddProperty(unknownPropertyName, 1);

            // When
            var errorMessages = new List<string>();
            var model = new WaterFlowModel1D();
            new WaterFlowModelTimePropertiesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedMessage = string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.Contains(expectedMessage, errorMessages);
        }

        [Test]
        public void GivenACategoryWithCorrectPropertiesInLowerCase_WhenSettingProperties_ThenNoExceptionsOrErrorMessagesAreThrown()
        {
            // Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.TimeHeader);
            category.AddProperty("starttime", "2014-01-01 00:00:00");
            category.AddProperty("stoptime", "2014-01-16 00:00:00");
            category.AddProperty("timestep", 60.000);
            category.AddProperty("mapoutputtimestep", 3600.000);
            category.AddProperty("hisoutputtimestep", 600.000);

            // When - Then
            var errorMessages = new List<string>();
            var model = new WaterFlowModel1D();
            Assert.DoesNotThrow(() => new WaterFlowModelTimePropertiesSetter().SetProperties(category, model, errorMessages));

            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        private DelftIniCategory GetCorrectTimeSettingsDataModel()
        {
            var timeSettingsCategory = new DelftIniCategory(ModelDefinitionsRegion.TimeHeader);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StartTime.Key, defaultStartTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StopTime.Key, defaultStopTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.TimeStep.Key, defaultTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.MapOutputTimeStep.Key, defaultGridPointsTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.HisOutputTimeStep.Key, defaultStructuresTimeStep.TotalSeconds);

            return timeSettingsCategory;
        }
    }
}