using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class RainfallRunoffSettingsValidator
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, RainfallRunoffModel model)
        {
            var reports = new List<ValidationReport>();

            reports.Add(new ValidationReport("Evaporation period",
                                             ValidateEvaporationSettings(model.EvaporationStartActivePeriod,
                                                                         model.EvaporationEndActivePeriod, model).ToList
                                                 ()));

            return new ValidationReport("Settings", reports);
        }

        private static IEnumerable<ValidationIssue> ValidateEvaporationSettings(int start, int end,
                                                                                RainfallRunoffModel model)
        {
            var issues = new List<ValidationIssue>();
            if (start < 1)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                                 string.Format("Start evaporation period ({0}) is less than 1 hour", start)));
            }

            if (start > 24)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                                 string.Format("Start evaporation period ({0}) is greater than 24 hours", start)));
            }

            if (end < 1)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                                 string.Format("End evaporation period ({0}) is less than 1 hour", end)));
            }

            if (end > 24)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                                 string.Format("End evaporation period ({0}) is greater than 24 hours", end)));
            }

            if (end <= start)
            {
                issues.Add(new ValidationIssue(model, ValidationSeverity.Error,
                                                 string.Format(
                                                     "End evaporation period ({0}) is less than or equal to the start of the period ({1}).",
                                                     end, start)));
            }
            return issues;
        }
    }
}