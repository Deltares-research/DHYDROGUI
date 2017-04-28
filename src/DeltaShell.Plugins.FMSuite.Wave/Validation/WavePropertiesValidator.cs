using System;
using System.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WavePropertiesValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            return new ValidationReport(Resources.WavePropertiesValidator_Validate_Waves_Model_Properties, new List<ValidationReport>{ValidateWindSpeedAndQuadruple(model)});
        }

        private static ValidationReport ValidateWindSpeedAndQuadruple(WaveModel waveModel)
        {
            var windConstantSelected = waveModel.TimePointData.WindDataType == InputFieldDataType.Constant;

            var windSpeedValue = waveModel.TimePointData.WindSpeedConstant;
            bool quadrupleSelected = false;
            try
            {
                 quadrupleSelected = Convert.ToBoolean(waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Quadruplets).Value);
            }
            catch (Exception)
            {
                quadrupleSelected = false;
            }
            
            var issues = new List<ValidationIssue>();
            if (windConstantSelected && quadrupleSelected && (Math.Abs(windSpeedValue) <= double.Epsilon) )
            {
                issues.Add(new ValidationIssue(waveModel, ValidationSeverity.Warning, Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruple_is_true_));
            }

            return new ValidationReport(string.Format(Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_Domain___0_, waveModel), issues); 
        }
    }
}