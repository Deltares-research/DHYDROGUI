using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class GreenhouseDataValidator : IValidator<RainfallRunoffModel, IEnumerable<GreenhouseData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<GreenhouseData> target = null)
        {
            if (!target.Any())
            {
                return ValidationReport.Empty("Greenhouse concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            foreach (var greenhouseData in target)
            {
                RainfallRunoffModelValidator.ValidateRunoffs(greenhouseData, issues);
            }

            return new ValidationReport("Greenhouse concept", issues);
        }
    }
}