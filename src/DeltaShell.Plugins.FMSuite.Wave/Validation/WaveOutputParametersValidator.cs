using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveOutputParametersValidator
    {
        public static ValidationReport Validate(WaveModel waveModel)
        {
            var validationIssues = new List<ValidationIssue>();
            if (waveModel.WriteTable && !waveModel.ObservationPoints.Any())
            {
                validationIssues.Add(new ValidationIssue(waveModel, 
                    ValidationSeverity.Warning,
                    Resources.WaveOutputParametersValidator_Validate_Option__Write_Tables__is_selected_but_there_are_no_Observation_Points_in_your_model_));
            }

            return new ValidationReport("Output parameters", validationIssues);
        }
    }
}