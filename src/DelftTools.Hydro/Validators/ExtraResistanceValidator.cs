using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;

namespace DelftTools.Hydro.Validators
{
    public static class ExtraResistanceValidator
    {
        public static ValidationReport Validate(IEnumerable<IStructure1D> extraResistances)
        {
            var issues = new List<ValidationIssue>();

            foreach (IExtraResistance extraResistance in extraResistances)
            {
                int count = extraResistance.FrictionTable.Arguments[0].Values.Count;

                if (count == 0)
                {
                    issues.Add(new ValidationIssue(extraResistance, ValidationSeverity.Error,
                        "Empty roughness table"));
                }
            }
            return new ValidationReport("Extra resistance",
                issues);
        }
    }
}