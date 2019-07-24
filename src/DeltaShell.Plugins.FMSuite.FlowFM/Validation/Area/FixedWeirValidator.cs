using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class FixedWeirValidator
    {
        /// <summary>
        /// Validate the fixed weirs and return any encountered issues.
        /// </summary>
        /// <param name="fixedWeirs"> The set of fixed weirs to be evaluated. </param>
        /// <param name="gridExtent"> The grid extent to which the fixed weir should be snapped. </param>
        /// <param name="fixedWeirsProperties"> The fixed weir properties </param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<FixedWeir> fixedWeirs,
                                                            Envelope gridExtent,
                                                            IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties,
                                                            string fixedWeirScheme)
        {
            var issues = new List<ValidationIssue>();
            List<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsPropertyList = fixedWeirsProperties.ToList();

            Enum.TryParse(fixedWeirScheme, true, out FixedWeirSchemes scheme);
            double minimalGroundHeight = GetMinimalGroundHeightForScheme(scheme);
            string schemeName = scheme.GetDescription();

            foreach (FixedWeir fixedWeir in fixedWeirs)
            {
                issues.AddRange(fixedWeir.ValidateSnapping(gridExtent));
                if (scheme != FixedWeirSchemes.None)
                {
                    issues.AddRange(fixedWeir.ValidateGroundHeights(fixedWeirsPropertyList, minimalGroundHeight, schemeName));
                }
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateSnapping(this FixedWeir fixedWeir, Envelope gridExtent)
        {
            if (!fixedWeir.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                yield return new ValidationIssue(fixedWeir,
                                                 ValidationSeverity.Warning,
                                                 string.Format(
                                                     Resources
                                                         .FixedWeirValidator_ValidateSnapping_fixed_weir___0___not_within_grid_extent_,
                                                     fixedWeir.Name),
                                                 fixedWeir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGroundHeights(this FixedWeir fixedWeir,
                                                                          IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties,
                                                                          double minimalGroundHeight,
                                                                          string schemeName)
        {
            ModelFeatureCoordinateData<FixedWeir> dataToCheck = fixedWeirsProperties
                .FirstOrDefault(d => d.Feature == fixedWeir);

            if (dataToCheck == null)
            {
                yield break;
            }

            bool ValidationFailedGroundHeightColumn(int columnIndex, string side, out ValidationIssue issue)
            {
                if (dataToCheck.DataColumns[columnIndex].ValueList.Cast<double>().Any(value => value < minimalGroundHeight))
                {
                    issue = new ValidationIssue(fixedWeir, ValidationSeverity.Info,
                                                string.Format(Resources.FixedWeirValidator_Fixed_weir_contains_ground_heights_smaller_than_minimum,
                                                              fixedWeir.Name, schemeName, side, minimalGroundHeight));
                    return true;
                }

                issue = null;
                return false;
            }

            if (ValidationFailedGroundHeightColumn(1, "left", out ValidationIssue issueLeftColumn))
            {
                yield return issueLeftColumn;
            }

            if (ValidationFailedGroundHeightColumn(2, "right", out ValidationIssue issueRightColumn))
            {
                yield return issueRightColumn;
            }
        }

        private static double GetMinimalGroundHeightForScheme(FixedWeirSchemes scheme)
        {
            switch (scheme)
            {
                case FixedWeirSchemes.Scheme6:
                case FixedWeirSchemes.Scheme9:
                    return 0.0d;
                case FixedWeirSchemes.Scheme8:
                    return 0.10d;
                default:
                    return 0.0d;
            }
        }
    }
}