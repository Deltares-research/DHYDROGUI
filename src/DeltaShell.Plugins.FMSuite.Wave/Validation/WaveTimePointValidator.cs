using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveTimePointValidator
    {
        /// <summary>
        /// Validation for wave model time point editing.
        /// <param name="model">A wave model entity</param>
        /// <returns>A validation report regarding wave model time points</returns>
        /// </summary>
        public static ValidationReport Validate(WaveModel model)
        {
            var issues = new List<ValidationIssue>();
            var timePoints = model.TimePointData.TimePoints;

            if (!model.IsCoupledToFlow && timePoints.Count == 0)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, Resources.WaveTimePointValidator_Validate_No_time_points_defined,
                                               model.TimePointData));
            }

            if (timePoints.Count > 0)
            {
               var hasInvalidTimePoint = timePoints.Any(tp => tp < model.ModelDefinition.ModelReferenceDateTime);

               if (hasInvalidTimePoint)
               {
                   issues.Add(new ValidationIssue(null, ValidationSeverity.Error, Resources.WaveTimePointValidator_Validate_Model_Start_time_precedes_Reference_Time,
                       model.TimePointData));
                }
            }

            return new ValidationReport("Waves Model Time Points", issues);
        }
    }
}