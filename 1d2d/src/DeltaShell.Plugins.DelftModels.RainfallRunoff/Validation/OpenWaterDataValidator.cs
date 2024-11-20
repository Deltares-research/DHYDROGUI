using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class OpenWaterDataValidator : IValidator<RainfallRunoffModel, IEnumerable<OpenWaterData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<OpenWaterData> target = null)
        {
            if (!target.Any())
            {
                return ValidationReport.Empty("Open water concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            foreach (var openWaterData in target)
            {
                RainfallRunoffModelValidator.ValidateRunoffs(openWaterData, issues);
            }

            return new ValidationReport("Open water concept", issues);
        }
    }
}