using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class RainfallRunoffBasinValidator : IValidator<RainfallRunoffModel, IDrainageBasin>
    {
        #region IValidator<RainfallRunoffModel,DrainageBasin> Members

        public ValidationReport Validate(RainfallRunoffModel rootObject, IDrainageBasin target)
        {
            var issues = new List<ValidationIssue>();

            var allCatchments = target.AllCatchments.ToList();

            if (allCatchments.Count == 0)
            {
                issues.Add(new ValidationIssue(target, ValidationSeverity.Error, "Contains no concrete catchments", target));
            }

            issues.AddRange(ValidationHelper.ValidateDuplicateNames(allCatchments, "catchments",
                                                                    target));

            var links = target.Links.Cast<INameable>().ToList();

            var hydroRegion = target.Parent as IHydroRegion;
            if (hydroRegion != null)
            {
                links.AddRange(hydroRegion.Links);
            }

            issues.AddRange(ValidationHelper.ValidateDuplicateNames(links, "links", target));

            issues.AddRange(ValidationHelper.ValidateDuplicateNames(target.WasteWaterTreatmentPlants.Cast<INameable>(),
                                                                    "wastewater treatment plants", target));

            issues.AddRange(ValidationHelper.ValidateDuplicateNames(target.Boundaries.Cast<INameable>(),
                                                        "runoff boundaries", target));

            foreach (WasteWaterTreatmentPlant wwtp in target.WasteWaterTreatmentPlants)
            {
                if (wwtp.Links.Count(l => Equals(wwtp, l.Target)) == 0)
                {
                    issues.Add(new ValidationIssue(wwtp, ValidationSeverity.Warning,
                                                   "Wastewater Treatment Plant has no incoming runoff links", target));
                }
                if (wwtp.Links.Count(l => Equals(wwtp, l.Source)) == 0)
                {
                    issues.Add(new ValidationIssue(wwtp, ValidationSeverity.Warning,
                                                   "Wastewater Treatment Plant has no outgoing runoff links; an implicit boundary will be created.", target));
                }
                else if (wwtp.Links.Count(l => Equals(wwtp, l.Source)) > 1)
                {
                    issues.Add(new ValidationIssue(wwtp, ValidationSeverity.Error,
                                                   "Wastewater Treatment Plant has more than one outgoing runoff link",
                                                   target));
                }
            }

            return new ValidationReport("Basin", issues);
        }

        #endregion
    }
}