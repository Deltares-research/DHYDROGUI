using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class FixedWeirValidator
    {
        private static IList<ValidationIssue> issues;

        /// <summary>
        /// Validate the fixed weirs and return any encountered issues.
        /// </summary>
        /// <param name="fixedWeirs">The set of fixed weirs to be evaluated.</param>
        /// <param name="gridExtent">The grid extent to which the fixed weir should be snapped.</param>
        /// <param name="fixedWeirsProperties">The fixed weir properties</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(IEnumerable<FixedWeir> fixedWeirs, Envelope gridExtent, IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties)
        {
            issues = new List<ValidationIssue>();
            var fixedWeirsPropertyList = fixedWeirsProperties.ToList();

            foreach (var fixedWeir in fixedWeirs)
            {
                fixedWeir.ValidateSnapping(gridExtent);
                fixedWeir.ValidateSillDepths(fixedWeirsPropertyList);
            }

            return issues;
        }

        private static void ValidateSnapping(this FixedWeir fixedWeir, Envelope gridExtent)
        {
            if (!fixedWeir.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                issues.Add(new ValidationIssue(fixedWeir,
                    ValidationSeverity.Warning,
                    $"fixed weir '{fixedWeir.Name}' not within grid extent.",
                    fixedWeir));
            }
        }

        private static void ValidateSillDepths(this FixedWeir fixedWeir, IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties)
        {
            var dataToCheck = fixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);

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
    }
}