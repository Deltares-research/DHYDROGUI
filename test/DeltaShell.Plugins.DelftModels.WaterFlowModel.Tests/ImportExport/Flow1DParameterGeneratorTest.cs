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
    public class Flow1DParameterGeneratorTest
    {
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

            var gridOutputTimeStep      = TimeSpan.FromSeconds(7);
            var structureOutputTimeStep = TimeSpan.FromSeconds(77);

            // Set up the model with relevant values
            var model = new WaterFlowModel1D("Bacon")
            {
                StartTime = startTime,
                StopTime  = stopTime,
                TimeStep  = timeStep,
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
