using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// Validator for the model definition properties of <see cref="WaveModel"/> instances.
    /// </summary>
    public static class WavePropertiesValidator
    {
        /// <summary>
        /// Validates the properties of a <see cref="WaveModel"/>.
        /// </summary>
        /// <param name="waveModel"> The D-Waves model. </param>
        /// <returns> A validation report with encountered issues. </returns>
        public static ValidationReport Validate(WaveModel waveModel)
        {
            return new ValidationReport(Resources.WavePropertiesValidator_Validate_Waves_Model_Properties,
                                        new List<ValidationReport>
                                        {
                                            GenerateGeneralCategoryValidationReport(waveModel),
                                            new ValidationReport(KnownWaveSections.ProcessesSection, GetProcessesValidationIssues(waveModel))
                                        });
        }

        private static ValidationReport GenerateGeneralCategoryValidationReport(WaveModel model)
        {
            var validationIssues = new List<ValidationIssue>();

            validationIssues.AddRange(GetTimeStepTimeIntervalValidationIssues(model));
            validationIssues.AddRange(GetBoundaryValidationIssues(model));

            return new ValidationReport(KnownWaveSections.GeneralSection, validationIssues);
        }

        private static IEnumerable<ValidationIssue> GetBoundaryValidationIssues(WaveModel model)
        {
            IBoundaryContainer boundaryContainer = model.BoundaryContainer;
            bool useFile = boundaryContainer.DefinitionPerFileUsed;

            if (!useFile)
            {
                yield break;
            }

            string filePath = boundaryContainer.FilePathForBoundariesPerFile;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                var waveValidationShortcut = new WaveValidationShortcut()
                {
                    WaveModel = model,
                    TabName = KnownWaveSections.GeneralSection
                };
                yield return new ValidationIssue(waveValidationShortcut, ValidationSeverity.Error,
                                                 Resources.WavePropertiesValidator_Validate_No_spectrum_file_has_been_selected);
            }
        }

        private static IEnumerable<ValidationIssue> GetTimeStepTimeIntervalValidationIssues(WaveModel model)
        {
            WaveModelProperty timeStepProperty = model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeStep);
            var timeStep = (double)timeStepProperty.Value;

            WaveModelProperty tScaleProperty = model.ModelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.TimeScale);
            var tScale = (double)tScaleProperty.Value;

            if (timeStep > tScale && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error,
                                                 Resources.WavePropertiesValidator_ValidateThat_TimeStep_Is_Not_Bigger_Than_TimeScale);
            }

            if (tScale % timeStep >= 1e-10 && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Error,
                                                 Resources.WavePropertiesValidator_ValidateDivisor);
            }

            if (tScale % 1 >= 1e-10 && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Warning,
                                                 Resources.WavePropertiesValidator_ValidateTScale);
            }

            if (timeStep % 1 >= 1e-10 && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                yield return new ValidationIssue(model, ValidationSeverity.Warning,
                                                 Resources.WavePropertiesValidator_ValidateTimeStep);
            }
        }

        private static IEnumerable<ValidationIssue> GetProcessesValidationIssues(WaveModel waveModel)
        {
            if (waveModel.ModelDefinition.DefaultWindUsage != UsageFromFlowType.DoNotUse)
            {
                yield break;
            }

            IVariable windSpeedValueTimeSeries = waveModel.TimeFrameData.TimeVaryingData.Components.FirstOrDefault(c => c.Name == "Wind Speed");

            WaveModelProperty quadrupletsProperty = waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.Quadruplets);
            bool quadrupletsSelected = Convert.ToBoolean(quadrupletsProperty.Value);

            if (quadrupletsSelected)
            {
                if (waveModel.TimeFrameData.WindInputDataType == WindInputDataType.Constant &&
                    Math.Abs(waveModel.TimeFrameData.WindConstantData.Speed) <= double.Epsilon)
                {
                    yield return new ValidationIssue(waveModel, ValidationSeverity.Error,
                                                     Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruplets_is_activated_);
                }
                else if (waveModel.TimeFrameData.WindInputDataType == WindInputDataType.TimeVarying &&
                         windSpeedValueTimeSeries != null &&
                         windSpeedValueTimeSeries.Values.Cast<double>()
                                                 .Any(windSpeedValue => Math.Abs(windSpeedValue) < double.Epsilon))
                {
                    yield return new ValidationIssue(waveModel, ValidationSeverity.Error,
                                                     Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruplets_is_activated_);
                }
            }
        }
    }
}