using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WavePropertiesValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            return new ValidationReport(Resources.WavePropertiesValidator_Validate_Waves_Model_Properties,
                                        new List<ValidationReport>
                                        {
                                            ValidateTimeStepTimeInterval(model),
                                            ValidateWindSpeedAndQuadruple(model)
                                        });
        }

        private static ValidationReport ValidateTimeStepTimeInterval(WaveModel model)
        {
            WaveModelProperty timeStepProperty = model.ModelDefinition.GetModelProperty(
                KnownWaveCategories.GeneralCategory,
                KnownWaveProperties.TimeStep);
            var timeStep = (double) timeStepProperty.Value;

            WaveModelProperty tScaleProperty = model.ModelDefinition.GetModelProperty(
                KnownWaveCategories.GeneralCategory,
                KnownWaveProperties.TimeScale);
            var tScale = (double) tScaleProperty.Value;

            var issues = new List<ValidationIssue>();

            if (timeStep > tScale && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                               Resources
                                                   .WavePropertiesValidator_ValidateThat_TimeStep_Is_Not_Bigger_Than_TimeScale));
            }

            if (tScale % timeStep >= 1e-10 && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                               Resources.WavePropertiesValidator_ValidateDivisor));
            }

            if (tScale % 1 >= 1e-10 && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                                               Resources.WavePropertiesValidator_ValidateTScale));
            }

            if (timeStep % 1 >= 1e-10 && timeStepProperty.IsEnabled(model.ModelDefinition.Properties) &&
                tScaleProperty.IsEnabled(model.ModelDefinition.Properties))
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Warning,
                                               Resources.WavePropertiesValidator_ValidateTimeStep));
            }

            return new ValidationReport(KnownWaveCategories.GeneralCategory, issues);
        }

        private static ValidationReport ValidateWindSpeedAndQuadruple(WaveModel waveModel)
        {
            bool windConstantSelected = waveModel.TimePointData.WindDataType == InputFieldDataType.Constant;
            bool windTimeseriesSelected = waveModel.TimePointData.WindDataType == InputFieldDataType.TimeVarying;

            double windSpeedValueConstant = waveModel.TimePointData.WindSpeedConstant;
            IVariable windSpeedValueTimeseries =
                waveModel.TimePointData.InputFields.Components.FirstOrDefault(c => c.Name == "Wind Speed");

            var quadrupleSelected = false;
            try
            {
                quadrupleSelected =
                    Convert.ToBoolean(waveModel.ModelDefinition
                                               .GetModelProperty(KnownWaveCategories.ProcessesCategory,
                                                                 KnownWaveProperties.Quadruplets).Value);
            }
            catch (Exception)
            {
                quadrupleSelected = false;
            }

            var issues = new List<ValidationIssue>();

            if (windConstantSelected && quadrupleSelected && Math.Abs(windSpeedValueConstant) <= double.Epsilon)
            {
                issues.Add(new ValidationIssue(waveModel, ValidationSeverity.Error,
                                               Resources
                                                   .WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruple_is_true_));
            }
            else if (windTimeseriesSelected && quadrupleSelected && windSpeedValueTimeseries != null)
            {
                foreach (object windSpeedValue in windSpeedValueTimeseries.Values)
                {
                    if (Math.Abs((double) windSpeedValue) <= double.Epsilon)
                    {
                        issues.Add(new ValidationIssue(waveModel, ValidationSeverity.Error,
                                                       Resources
                                                           .WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruple_is_true_));
                        break;
                    }
                }
            }

            return new ValidationReport(KnownWaveCategories.ProcessesCategory, issues);
        }
    }
}