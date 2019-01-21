using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Geometries;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class FixedWeirValidator
    {
        /// <summary>
        /// Validate the fixed weirs and return any encountered issues.
        /// </summary>
        /// <param name="fixedWeirs">The set of fixed weirs to be evaluated.</param>
        /// <param name="gridExtent">The grid extent to which the fixed weir should be snapped.</param>
        /// <param name="fixedWeirsProperties">The fixed weir properties</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(IEnumerable<FixedWeir> fixedWeirs, Envelope gridExtent, IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties)
        {
            var issues = new List<ValidationIssue>();
            var fixedWeirsPropertyList = fixedWeirsProperties.ToList();

            foreach (var fixedWeir in fixedWeirs)
            {
                issues.AddRange(fixedWeir.ValidateSnapping(gridExtent));
                issues.AddRange(fixedWeir.ValidateSillDepths(fixedWeirsPropertyList));
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateSnapping(this FixedWeir fixedWeir, Envelope gridExtent)
        {
            if (!fixedWeir.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                yield return new ValidationIssue(fixedWeir,
                    ValidationSeverity.Warning,
                    string.Format(Resources.FixedWeirValidator_ValidateSnapping_fixed_weir___0___not_within_grid_extent_, fixedWeir.Name),
                    fixedWeir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateSillDepths(this FixedWeir fixedWeir, IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties)
        {
            var dataToCheck = fixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);

            if (dataToCheck == null) yield break;

            var counter = dataToCheck.DataColumns[1].ValueList.Count;
            for (var i = 0; i < counter; i++)
            {
                if ((double) dataToCheck.DataColumns[1].ValueList[i] <= 0.0 ||
                    (double) dataToCheck.DataColumns[2].ValueList[i] <= 0.0)
                {
                    yield return new ValidationIssue(fixedWeir,
                        ValidationSeverity.Warning,
                        string.Format(Resources.FixedWeirValidator_ValidateSillDepths_fixed_weir___0___has_unphysical_sill_depths__parts_will_be_ignored_by_dflow_fm_, fixedWeir.Name),
                        fixedWeir);
                }
            }
        }
    }
}