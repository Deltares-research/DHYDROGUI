using System.Collections.Generic;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveTimePointValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            var issues = new List<ValidationIssue>();

            if (!model.IsCoupledToFlow && model.TimePointData.TimePoints.Count == 0)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error, "No time points defined",
                                               model.TimePointData));
            }

            return new ValidationReport("Waves Model Time Points", issues);
        }
    }
}