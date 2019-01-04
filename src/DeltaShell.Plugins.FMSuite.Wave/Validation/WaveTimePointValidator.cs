using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// Validator class for wave model time point editing.
    /// <param name="model">A wave model entity</param>
    /// <returns>A validation report regarding wave model time points</returns>
    /// </summary>
    public static class WaveTimePointValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            var issues = new List<ValidationIssue>();
            var timePoints = model.TimePointData.TimePoints;

            if (!model.IsCoupledToFlow && timePoints.Count == 0)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, "No time points defined",
                                               model.TimePointData));
            }

            if (timePoints.Count > 0)
            {
               var hasInvalidTimePoint = timePoints.Select(tp => tp < model.ModelDefinition.ModelReferenceDateTime).FirstOrDefault();

               if (hasInvalidTimePoint)
               {
                   issues.Add(new ValidationIssue(null, ValidationSeverity.Error, "Model Start time precedes Reference Time",
                       model.TimePointData));
                }
            }

            return new ValidationReport("Waves Model Time Points", issues);
        }
    }
}