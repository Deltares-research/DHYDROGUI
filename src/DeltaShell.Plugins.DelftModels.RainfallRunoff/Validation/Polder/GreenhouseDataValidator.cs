using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation.Polder
{
    public class GreenhouseDataValidator : IValidator<RainfallRunoffModel, IEnumerable<GreenhouseData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<GreenhouseData> targets = null)
        {
            if (!targets.Any())
            {
                return ValidationReport.Empty("Greenhouse concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            foreach (var greenhouseData in targets)
            {
                RainfallRunoffModelValidator.ValidateRunoffs(greenhouseData, issues);
            }

            return new ValidationReport("Greenhouse concept", issues);
        }
    }
}