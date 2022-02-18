using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class HbvDataValidator : IValidator<RainfallRunoffModel, IEnumerable<HbvData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<HbvData> target = null)
        {
            if (target == null || !target.Any())
            {
                return ValidationReport.Empty("HBV concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            foreach (var hbv in target)
            {
                issues.AddRange(ValidateHbv(hbv));
            }

            return new ValidationReport("HBV concept", issues);
        }

        private static IEnumerable<ValidationIssue> ValidateHbv(HbvData hbvData)
        {
            if (hbvData.BaseFlowReservoirConstant <= 0.0 || hbvData.BaseFlowReservoirConstant >= 1.0)
            {
                yield return
                    new ValidationIssue(hbvData.Catchment, ValidationSeverity.Error,
                                        "Base flow reservoir constant should be between 0 and 1",
                                        hbvData);
            }
            if (hbvData.InterflowReservoirConstant <= 0.0 || hbvData.InterflowReservoirConstant >= 1.0)
            {
                yield return
                    new ValidationIssue(hbvData.Catchment, ValidationSeverity.Error,
                                        "Inter flow reservoir constant should be between 0 and 1",
                                        hbvData);
            }
            if (hbvData.QuickFlowReservoirConstant <= 0.0 || hbvData.QuickFlowReservoirConstant >= 1.0)
            {
                yield return
                    new ValidationIssue(hbvData.Catchment, ValidationSeverity.Error,
                                        "Quick flow reservoir constant should be between 0 and 1",
                                        hbvData);
            }
        }
    }
}