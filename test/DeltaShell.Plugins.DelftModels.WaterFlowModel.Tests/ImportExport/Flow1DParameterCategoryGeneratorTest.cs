using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class Flow1DParameterCategoryGeneratorTest
    {
        [Test]
        public void GivenAWaterFlowModel1D_WhenGenerateSpecialsValuesIsCalledWithThisModel_ThenACorrectGenerateSpecialsValuesIsGenerated()
        {
            //Given 
            var model = new WaterFlowModel1D("TestModel")
            {
                DesignFactorDlg = 1.0
            };

            //When
            var category = Flow1DParameterCategoryGenerator.GenerateSpecialsValues(model);

            // Then
            Assert.That(category, Is.Not.Null);
            Assert.That(category, Is.TypeOf<DelftIniCategory>());
            Assert.That(category.Properties, Is.Not.Null);

            var properties = category.Properties.ToList();
            Assert.That(properties.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowModel1DWithSedimentValues_WhenGeneratingSedimentProperties_ThenADelftIniCategoryIsReturned()
        {
            var model = new WaterFlowModel1D("TestModel")
            {
                D50 = 0.0005,
                D90 = 0.001,
                DepthUsedForSediment = 0.1
            };

            var category = Flow1DParameterCategoryGenerator.GenerateSedimentValues(model);

            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        [Test]
        public void GivenAWaterFlowModel1DWithoutSedimentValues_WhenGeneratingSedimentProperties_ThenNoPropertiesAreReturnedAndValuesAreNotSetOnModel()
        {
            var model = new WaterFlowModel1D("TestModel");

            var category = Flow1DParameterCategoryGenerator.GenerateSedimentValues(model);

            Assert.That(category.Properties.Count, Is.EqualTo(0));
            Assert.That(model.D50, Is.EqualTo(null));
            Assert.That(model.D90, Is.EqualTo(null));
            Assert.That(model.DepthUsedForSediment, Is.EqualTo(null));
        }

        /// <summary>
        /// GIVEN a WaterFlowModel1D
        /// WHEN GenerateTimeValues is called with this model
        /// THEN a correct Time DelftIniCategory is generated
        /// </summary>
        [Test]
        public void GivenAWaterFlowModel1D_WhenGenerateTimeValuesIsCalledWithThisModel_ThenACorrectTimeDelftIniCategoryIsGenerated()
        {
            // Given
            // Relevant values
            var startTime = DateTime.Today;
            var stopTime = startTime.AddDays(2);
            var timeStep = TimeSpan.FromSeconds(3);

            var gridOutputTimeStep = TimeSpan.FromSeconds(7);
            var structureOutputTimeStep = TimeSpan.FromSeconds(77);

            // Set up the model with relevant values
            var model = new WaterFlowModel1D("Bacon")
            {
                StartTime = startTime,
                StopTime = stopTime,
                TimeStep = timeStep,
                OutputSettings =
                {
                    GridOutputTimeStep      = gridOutputTimeStep,
                    StructureOutputTimeStep = structureOutputTimeStep
                },
            };

            // When
            var result = Flow1DParameterCategoryGenerator.GenerateTimeValues(model);

            // Then
            Assert.That(result.Properties, Is.Not.Null, "Expected the returned DelftIniCategory.Properties not to be null.");
            var properties = result.Properties.ToList();

            Assert.That(properties.Count, Is.EqualTo(5), "Expected the [Time] DelftIniCategory to have a different number of properties:");

            // Expected strings
            var expectedStartTimeString = startTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var expectedStopTimeString = stopTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var expectedTimeStepString = timeStep.TotalSeconds.ToString(ModelDefinitionsRegion.TimeStep.Format,
                                                                        CultureInfo.InvariantCulture);
            var expectedGridTimeStepString = gridOutputTimeStep.TotalSeconds.ToString(ModelDefinitionsRegion.MapOutputTimeStep.Format,
                                                                                      CultureInfo.InvariantCulture);
            var expectedStructureTimeStepString = structureOutputTimeStep.TotalSeconds.ToString(ModelDefinitionsRegion.HisOutputTimeStep.Format,
                                                                                                CultureInfo.InvariantCulture);

            // Verify expected properties
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.StartTime, expectedStartTimeString);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.StopTime, expectedStopTimeString);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.TimeStep, expectedTimeStepString);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.MapOutputTimeStep, expectedGridTimeStepString);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.HisOutputTimeStep, expectedStructureTimeStepString);
        }

        /// <summary>
        /// GIVEN a WaterFlowModel1D
        /// WHEN GenerateRestartOptionsValues is called with this model
        /// THEN a correct Restart DelftIniCategory is generated. This correct category always contains all the 5 properties,
        /// so this is independent of the userestart and writerestart settings.
        /// </summary>
        [Test]
        [TestCase(true, true, TestName = "UseRestart and WriteRestart are true")]
        [TestCase(false, false, TestName = "UseRestart and WriteRestart are false")]
        [TestCase(true, false, TestName = "UseRestart is true and WriteRestart is false")]
        [TestCase(false, true, TestName = "UseRestart is false and WriteRestart is true")]
        public void GivenAWaterFlowModel1D_WhenGenerateRestartOptionsValuesIsCalledWithThisModel_ThenACorrectRestartDelftIniCategoryIsGenerated(bool useRestart, bool writeRestart)
        {
            // Given
            // Relevant values
            var saveStateStartTime = DateTime.Today;
            var saveStateStopTime = saveStateStartTime.AddDays(2);
            var saveStateTimeStep = TimeSpan.FromSeconds(3);

            // Set up the model with relevant values
            var model = new WaterFlowModel1D("TestModel")
            {
                UseRestart = useRestart,
                WriteRestart = writeRestart,
                SaveStateStartTime = saveStateStartTime,
                SaveStateStopTime = saveStateStopTime,
                SaveStateTimeStep = saveStateTimeStep,
            };

            // When
            var result = Flow1DParameterCategoryGenerator.GenerateRestartOptionsValues(model);

            // Then
            Assert.That(result.Properties, Is.Not.Null, "Expected the returned DelftIniCategory.Properties not to be null.");
            var properties = result.Properties.ToList();

            Assert.That(properties.Count, Is.EqualTo(5), "Expected the [Time] DelftIniCategory to have a different number of properties:");

            // Expected strings
            var expectedRestartStartTimeString = saveStateStartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var expectedRestartStopTimeString = saveStateStopTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var expectedRestartTimeStepString = ((int)(saveStateTimeStep.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
            var expectedUseRestart = useRestart ? "1" : "0";
            var expectedWriteRestart = writeRestart ? "1" : "0";

            // Verify expected properties
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.RestartStartTime, expectedRestartStartTimeString);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.RestartStopTime, expectedRestartStopTimeString);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.RestartTimeStep, expectedRestartTimeStepString);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.UseRestart, expectedUseRestart);
            AssertThatCategoryContainsPropertyWithValue(properties, ModelDefinitionsRegion.WriteRestart, expectedWriteRestart);
        }

        private static void AssertThatCategoryContainsPropertyWithValue(IList<DelftIniProperty> properties,
                                                                        ConfigurationSetting expectedProperty,
                                                                        string expectedValueString)
        {
            Assert.That(properties.Any(prop => prop.Name == expectedProperty.Key), Is.True, $"Expected DelftIniCategory.Properties to contain {expectedProperty.Key}.");
            var relevantProperty = properties.First(prop => prop.Name == expectedProperty.Key);

            Assert.That(relevantProperty.Comment, Is.EqualTo(expectedProperty.Description), $"Expected {expectedProperty.Key} to have a different description:");
            Assert.That(relevantProperty.Value, Is.EqualTo(expectedValueString), $"Expected {expectedProperty.Key} to have a different value:");
        }
    }
}