using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class UnpavedDataValidator : IValidator<RainfallRunoffModel, IEnumerable<UnpavedData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<UnpavedData> target = null)
        {
            if (!target.Any())
            {
                return ValidationReport.Empty("Unpaved concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            foreach (var unpavedData in target)
            {
                RainfallRunoffModelValidator.ValidateRunoffs(unpavedData, issues);
            }

            return new ValidationReport("Unpaved concept", issues);
        }
    }
}