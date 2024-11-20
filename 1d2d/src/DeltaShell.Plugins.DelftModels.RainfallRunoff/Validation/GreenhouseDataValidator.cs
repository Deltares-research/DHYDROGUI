using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class GreenhouseDataValidator : IValidator<RainfallRunoffModel, IEnumerable<GreenhouseData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rainfallRunoffModel, IEnumerable<GreenhouseData> target = null)
        {
            if (!target.Any())
            {
                return ValidationReport.Empty("Greenhouse concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            if (!rainfallRunoffModel.GreenhouseYear.IsInRange(RainfallRunoffModel.MinGreenhouseYear, RainfallRunoffModel.MaxGreenhouseYear))
            {
                var message = $"Greenhouse year must be in the period between {RainfallRunoffModel.MinGreenhouseYear} and {RainfallRunoffModel.MaxGreenhouseYear} ({RainfallRunoffModel.MaxGreenhouseYear} is the default year).";
                issues.Add(new ValidationIssue("Greenhouse year", ValidationSeverity.Error, message));
            }

            foreach (var greenhouseData in target)
            {
                RainfallRunoffModelValidator.ValidateRunoffs(greenhouseData, issues);
            }

            return new ValidationReport("Greenhouse concept", issues);
        }
    }
}