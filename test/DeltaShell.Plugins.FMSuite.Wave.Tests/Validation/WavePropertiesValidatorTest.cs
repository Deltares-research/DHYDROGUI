using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WavePropertiesValidatorTest
    {
        private const string windWarningMessage = "Wind speed should be greater than zero when the option of quadruplets is activated.";

        [Test]
        [TestCase(0.0, false, false, TestName = "ConstantWindSpeedZero_QuadrupletsFalse_ForConstantWindCase_NoWarning")]
        [TestCase(0.0, true, true, TestName = "ConstantWindSpeedZero_QuadrupletsTrue_ForConstantWindCase_WarningMustBeThere")]
        [TestCase(30.0, false, false, TestName = "ConstantWindSpeedGreaterThanZero_QuadrupletsFalse_ForConstantWindCase_NoWarning")]
        [TestCase(30.0, true, false, TestName = "ConstantWindSpeedGreaterThanZero_QuadrupletsTrue_ForConstantWindCase_NoWarning")]
        public void CheckWavePropertiesWithFlowModel_ConstantWindSpeed(double windSpeedConstant, bool quadruplets, bool warningAlert)
        {
            using (WaveModel model = WaveModelForPropertiesValidationBuilder.Start()
                                                                            .WithQuadruplets(quadruplets)
                                                                            .WithConstantWindSpeed(windSpeedConstant)
                                                                            .Finish())
            {
                ValidationReport validationReport = WavePropertiesValidator.Validate(model);
                if (warningAlert)
                {
                    Assert.IsTrue(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == ValidationSeverity.Error && i.Message == windWarningMessage));
                }
                else
                {
                    Assert.IsFalse(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == ValidationSeverity.Error && i.Message == windWarningMessage));
                }
            }
        }

        [Test]
        public void Validate_WaveModelWithDefaultWindUsageNotEqualToDoNotUse_Constant_ThenReturnValidationWarningsForWind()
        {
            // Arrange
            using (WaveModel waveModel = WaveModelForPropertiesValidationBuilder.Start()
                                                                                .WithDefaultWindUsage(UsageFromFlowType.DoNotUse)
                                                                                .WithQuadruplets(true)
                                                                                .WithConstantWindSpeed(0.0)
                                                                                .Finish())
            {
                // Act
                ValidationReport validationReport = WavePropertiesValidator.Validate(waveModel);

                // Assert
                ValidationReport processesReport = validationReport.SubReports.Single(r => r.Category == "Processes");
                ValidationIssue validationIssue = processesReport.Issues.Single(i => i.Message == "Wind speed should be greater than zero when the option of quadruplets is activated.");
                Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            }
        }

        [Test]
        public void Validate_WaveModelWithDefaultWindUsageNotEqualToDoNotUse_TimeVarying_ThenReturnValidationWarningsForWind()
        {
            // Arrange
            using (WaveModel waveModel = WaveModelForPropertiesValidationBuilder.Start()
                                                                                .WithDefaultWindUsage(UsageFromFlowType.DoNotUse)
                                                                                .WithQuadruplets(true)
                                                                                .WithWindSpeedTimeSeries(0.0)
                                                                                .Finish())
            {
                // Act
                ValidationReport validationReport = WavePropertiesValidator.Validate(waveModel);

                // Assert
                ValidationReport processesReport = validationReport.SubReports.Single(r => r.Category == "Processes");
                ValidationIssue validationIssue = processesReport.Issues.Single(i => i.Message == "Wind speed should be greater than zero when the option of quadruplets is activated.");
                Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
            }
        }

        [Test]
        public void GivenAWaveModel_WhenTScaleAndTimeStepAreIntegersAndDivisors_ThenNoValidationIssuesShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeScale).SetValueAsString("60");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeStep).SetValueAsString("10");

            model.TimeFrameData.WindInputDataType = WindInputDataType.Constant;
            model.TimeFrameData.WindConstantData.Speed = 10;

            ValidationReport validationReport = WavePropertiesValidator.Validate(model);

            Assert.AreEqual(0, validationReport.GetAllIssuesRecursive().Count);
        }

        [Test]
        public void GivenAWaveModel_WhenTScaleIsNotAnIntegerAndTimeStepNotADivisor_ThenAnErrorAndWarningShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeScale).SetValueAsString("60.1");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeStep).SetValueAsString("10");

            string expectedMessage1 = Resources.WavePropertiesValidator_ValidateTScale;
            string expectedMessage2 = Resources.WavePropertiesValidator_ValidateDivisor;

            ValidationReport validationReport = WavePropertiesValidator.Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                                .Any(
                                    i =>
                                        i.Severity == ValidationSeverity.Warning && i.Message == expectedMessage1));

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                                .Any(
                                    i =>
                                        i.Severity == ValidationSeverity.Error && i.Message == expectedMessage2));
        }

        [Test]
        public void GivenAWaveModel_WhenTimeStepIsNotAnIntegerAndNotADivisor_ThenAnErrorAndWarningShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeScale).SetValueAsString("60");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeStep).SetValueAsString("9.9");

            string expectedMessage1 = Resources.WavePropertiesValidator_ValidateTimeStep;
            string expectedMessage2 = Resources.WavePropertiesValidator_ValidateDivisor;

            ValidationReport validationReport = WavePropertiesValidator.Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                                .Any(
                                    i =>
                                        i.Severity == ValidationSeverity.Warning && i.Message == expectedMessage1));

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                                .Any(
                                    i =>
                                        i.Severity == ValidationSeverity.Error && i.Message == expectedMessage2));
        }

        [Test]
        public void GivenAWaveModel_WhenTimeStepIsBiggerThanTScale_ThenAnErrorShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeScale).SetValueAsString("60");
            model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeStep).SetValueAsString("65");

            string expectedMessage1 = Resources.WavePropertiesValidator_ValidateThat_TimeStep_Is_Not_Bigger_Than_TimeScale;

            ValidationReport validationReport = WavePropertiesValidator.Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                                .Any(
                                    i =>
                                        i.Severity == ValidationSeverity.Error && i.Message == expectedMessage1));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Validate_WaveModelWithBoundaryDefinitionPerFileUsedButFilePathNullOrWhitespace_ThenAnErrorShouldBeGiven(string filePath)
        {
            // Arrange
            using (var model = new WaveModel())
            {
                model.BoundaryContainer.DefinitionPerFileUsed = true;
                model.BoundaryContainer.FilePathForBoundariesPerFile = filePath;

                // Act
                ValidationReport validationReport = WavePropertiesValidator.Validate(model);

                // Assert
                const string expectedMessage = "No spectrum file has been selected.";
                IEnumerable<ValidationIssue> validationIssues = validationReport.GetAllIssuesRecursive();
                Assert.IsTrue(validationIssues.Any(
                                  i => i.Severity == ValidationSeverity.Error && i.Message == expectedMessage));
            }
        }

        [Test]
        public void Validate_ValidWaveModelWithBoundaryDefinitionPerFileUsedAndNonEmptyFilePath_ThenNoErrorShouldBeGiven()
        {
            // Arrange
            using (var model = new WaveModel())
            {
                const string filePath = "NonEmptyFilePath";

                model.BoundaryContainer.DefinitionPerFileUsed = true;
                model.BoundaryContainer.FilePathForBoundariesPerFile = filePath;

                // Act
                ValidationReport validationReport = WavePropertiesValidator.Validate(model);

                // Assert
                ValidationReport generalReport = validationReport.SubReports.Single(r => r.Category == KnownWaveSections.GeneralSection);
                IEnumerable<ValidationIssue> validationIssues = generalReport.GetAllIssuesRecursive();

                Assert.That(validationIssues, Has.Count.EqualTo(0));
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        [TestCase("NonEmptyFilePath")]
        public void Validate_ValidWaveModelWithoutBoundaryDefinitionPerFileUsed_ThenNoErrorShouldBeGiven(string filePath)
        {
            // Arrange
            using (var model = new WaveModel())
            {
                model.BoundaryContainer.DefinitionPerFileUsed = false;
                model.BoundaryContainer.FilePathForBoundariesPerFile = filePath;

                // Act
                ValidationReport validationReport = WavePropertiesValidator.Validate(model);

                // Assert
                ValidationReport generalReport = validationReport.SubReports.Single(r => r.Category == KnownWaveSections.GeneralSection);
                IEnumerable<ValidationIssue> validationIssues = generalReport.GetAllIssuesRecursive();

                Assert.That(validationIssues, Has.Count.EqualTo(0));
            }
        }

        [TestCase(new[]
        {
            30.0,
            30.0
        }, false, false, TestName = "TimeseriesGreaterThanZero_QuadrupletsFalse_ForTimeSeriesWindCase_NoError")]
        [TestCase(new[]
        {
            30.0,
            0.0
        }, false, false, TestName = "TimeseriesNotAllGreaterThanZero_QuadrupletsFalse_ForTimeSeriesWindCase_NoError")]
        [TestCase(new[]
        {
            0.0,
            0.0
        }, false, false, TestName = "TimeseriesZero_QuadrupletsFalse_ForTimeSeriesWindCase_NoError")]
        [TestCase(new[]
        {
            30.0,
            30.0
        }, true, false, TestName = "TimeseriesGreaterThanZero_QuadrupletsTrue_ForTimeSeriesWindCase_NoError")]
        [TestCase(new[]
        {
            30.0,
            0.0
        }, true, true, TestName = "TimeSeriesNotAllGreaterThanZero_QuadrupletsTrue_ForTimeSeriesWindCase_ErrorMustBeThere")]
        [TestCase(new[]
        {
            0.0,
            0.0
        }, true, true, TestName = "TimeSeriesZero_QuadrupletsTrue_ForTimeSeriesWindCase_ErrorMustBeThere")]
        public void CheckWavePropertiesWithFlowModel_TimeVaryingWindSpeed(double[] windSpeedTimeSeries, bool quadruplets, bool warningAlert)
        {
            using (WaveModel model = WaveModelForPropertiesValidationBuilder.Start()
                                                                            .WithQuadruplets(quadruplets)
                                                                            .WithWindSpeedTimeSeries(windSpeedTimeSeries)
                                                                            .Finish())
            {
                ValidationReport validationReport = WavePropertiesValidator.Validate(model);
                if (warningAlert)
                {
                    Assert.IsTrue(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == ValidationSeverity.Error && i.Message == windWarningMessage));
                }
                else
                {
                    Assert.IsFalse(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == ValidationSeverity.Error && i.Message == windWarningMessage));
                }
            }
        }

        [TestCase(UsageFromFlowType.UseDoNotExtend)]
        [TestCase(UsageFromFlowType.UseAndExtend)]
        public void Validate_WaveModelWithDefaultWindUsageNotEqualToDoNotUse_TimeVarying_ThenReturnValidationWarningsForWind(UsageFromFlowType windDataType)
        {
            // Arrange
            using (WaveModel waveModel = WaveModelForPropertiesValidationBuilder.Start()
                                                                                .WithDefaultWindUsage(windDataType)
                                                                                .Finish())
            {
                // Act
                ValidationReport validationReport = WavePropertiesValidator.Validate(waveModel);

                // Assert
                ValidationReport processesReport = validationReport.SubReports.Single(r => r.Category == "Processes");
                Assert.IsEmpty(processesReport.Issues);
            }
        }

        /// <summary>
        /// Fluent builder for <see cref="waveModel"/> instances.
        /// </summary>
        private class WaveModelForPropertiesValidationBuilder
        {
            private readonly WaveModel waveModel;

            private WaveModelForPropertiesValidationBuilder()
            {
                waveModel = new WaveModel();
            }

            /// <summary>
            /// Creates a new instance of <see cref="WaveModelForPropertiesValidationBuilder"/>.
            /// </summary>
            /// <returns> The new instance. </returns>
            public static WaveModelForPropertiesValidationBuilder Start()
            {
                return new WaveModelForPropertiesValidationBuilder();
            }

            /// <summary>
            /// Sets the usage type for wind in a D-Waves model.
            /// </summary>
            /// <param name="windUsageType"> The wind usage type. </param>
            /// <returns> An instance of <see cref="WaveModelForPropertiesValidationBuilder"/>. </returns>
            public WaveModelForPropertiesValidationBuilder WithDefaultWindUsage(UsageFromFlowType windUsageType)
            {
                waveModel.ModelDefinition.DefaultWindUsage = windUsageType;

                return this;
            }

            /// <summary>
            /// Sets the quadruplets property value to a given value.
            /// </summary>
            /// <param name="value"> The value to set. </param>
            /// <returns> An instance of <see cref="WaveModelForPropertiesValidationBuilder"/>. </returns>
            public WaveModelForPropertiesValidationBuilder WithQuadruplets(bool value)
            {
                WaveModelProperty quadrupletsProperty = waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.Quadruplets);
                quadrupletsProperty.Value = value;

                return this;
            }

            /// <summary>
            /// Sets the wind speed constant value to a given value.
            /// </summary>
            /// <param name="value"> The value to set. </param>
            /// <returns> An instance of <see cref="WaveModelForPropertiesValidationBuilder"/>. </returns>
            public WaveModelForPropertiesValidationBuilder WithConstantWindSpeed(double value)
            {
                waveModel.TimeFrameData.WindInputDataType = WindInputDataType.Constant;
                waveModel.TimeFrameData.WindConstantData.Speed = value;

                return this;
            }

            /// <summary>
            /// Sets the wind speed time series values to a given set of values.
            /// </summary>
            /// <param name="values"> The values to set on the time series. </param>
            /// <returns> An instance of <see cref="WaveModelForPropertiesValidationBuilder"/>. </returns>
            public WaveModelForPropertiesValidationBuilder WithWindSpeedTimeSeries(params double[] values)
            {
                waveModel.TimeFrameData.WindInputDataType = WindInputDataType.TimeVarying;
                IVariable timeSeriesWindSpeed = waveModel.TimeFrameData.TimeVaryingData.Components.FirstOrDefault(c => c.Name == "Wind Speed");
                timeSeriesWindSpeed?.Values.AddRange(values);

                return this;
            }

            /// <summary>
            /// Returns a configured instance of <see cref="waveModel"/>.
            /// </summary>
            /// <returns> The configured D-waves model. </returns>
            public WaveModel Finish()
            {
                return waveModel;
            }
        }
    }
}