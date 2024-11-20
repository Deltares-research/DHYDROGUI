using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class SacramentoDataValidator : IValidator<RainfallRunoffModel, IEnumerable<SacramentoData>>
    {
        public ValidationReport Validate(RainfallRunoffModel rootObject, IEnumerable<SacramentoData> target = null)
        {
            if (target == null || !target.Any())
            {
                return ValidationReport.Empty("Sacramento concept"); //nothing to report
            }
            var issues = new List<ValidationIssue>();

            foreach (var sacramentoData in target)
            {
                issues.AddRange(ValidateSacramento(sacramentoData));
            }

            return new ValidationReport("Sacramento concept", issues);
        }

        private IEnumerable<ValidationIssue> ValidateSacramento(SacramentoData sacramentoData)
        {
            if (sacramentoData.UpperZoneFreeWaterStorageCapacity < 1.0)
            {
                yield return
                    new ValidationIssue(sacramentoData, ValidationSeverity.Error,
                                        "Upper zone free water storage capacity should be larger than or equal to 1",
                                        sacramentoData.Catchment.Basin);
            }
            if (sacramentoData.UpperZoneTensionWaterStorageCapacity < 1.0)
            {
                yield return
                    new ValidationIssue(sacramentoData, ValidationSeverity.Error,
                                        "Upper zone tension water storage capacity should be larger than or equal to 1",
                                        sacramentoData.Catchment.Basin);
            }
            if (sacramentoData.LowerZoneTensionWaterStorageCapacity < 1.0)
            {
                yield return
                    new ValidationIssue(sacramentoData, ValidationSeverity.Error,
                                        "Lower zone tension water storage capacity should be larger than or equal to 1",
                                        sacramentoData.Catchment.Basin);
            }
        }
    }
}