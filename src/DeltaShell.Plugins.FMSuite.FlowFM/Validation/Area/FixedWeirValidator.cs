using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class FixedWeirValidator
    {
        private static List<ValidationIssue> issues;

        /// <summary>
        /// Validate the fixed weirs and return any encountered issues.
        /// </summary>
        /// <param name="model">The model to which the fixed weirs belong.</param>
        /// <param name="fixedWeirs">The set of fixed weirs to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(WaterFlowFMModel model, IEnumerable<FixedWeir> fixedWeirs)
        {
            issues = new List<ValidationIssue>();

            foreach (var fixedWeir in fixedWeirs)
            {
                fixedWeir.ValidateSnapping(model);
                fixedWeir.ValidateSillDepths(model);
            }

            return issues;
        }

        private static void ValidateSillDepths(this FixedWeir fixedWeir, WaterFlowFMModel model)
        {
            var dataToCheck =
                model.FixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);

            if (dataToCheck == null) return;

            var counter = dataToCheck.DataColumns[1].ValueList.Count;
            for (var i = 0; i < counter; i++)
            {
                if ((double) dataToCheck.DataColumns[1].ValueList[i] <= 0.0 ||
                    (double) dataToCheck.DataColumns[2].ValueList[i] <= 0.0)
                {
                    issues.Add(new ValidationIssue(fixedWeir,
                        ValidationSeverity.Warning,
                        $"fixed weir '{fixedWeir.Name}' has unphysical sill depths, parts will be ignored by dflow-fm.",
                        fixedWeir));
                }
            }
        }

        private static void ValidateSnapping(this FixedWeir fixedWeir, WaterFlowFMModel model)
        {
            if (!model.SnapsToGrid(fixedWeir.Geometry))
            {
                issues.Add(new ValidationIssue(fixedWeir,
                    ValidationSeverity.Warning,
                    $"fixed weir '{fixedWeir.Name}' not within grid extent",
                    fixedWeir));
            }
        }
    }
}