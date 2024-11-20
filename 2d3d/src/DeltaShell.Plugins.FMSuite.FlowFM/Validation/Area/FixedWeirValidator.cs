using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
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
        /// <param name="scheme"> The fixed weir scheme the model is using. </param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<FixedWeir> fixedWeirs,
                                                            Envelope gridExtent,
                                                            IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties,
                                                            FixedWeirSchemes scheme)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(fixedWeirs.ValidateSnapping(gridExtent));

            if (scheme != FixedWeirSchemes.None)
            {
                issues.AddRange(ValidateGroundHeights(fixedWeirsProperties, scheme));
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateSnapping(this IEnumerable<FixedWeir> fixedWeirs,
                                                                     Envelope gridExtent)
        {
            foreach (FixedWeir fixedWeir in fixedWeirs)
            {
                if (!fixedWeir.Geometry.SnapsToFlowFmGrid(gridExtent))
                {
                    yield return new ValidationIssue(fixedWeir,
                                                     ValidationSeverity.Warning,
                                                     string.Format(Resources.FixedWeirValidator_ValidateSnapping_fixed_weir___0___not_within_grid_extent_,
                                                                   fixedWeir.Name),
                                                     fixedWeir);
                }
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGroundHeights(
            IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties,
            FixedWeirSchemes scheme)
        {
            string schemeName = scheme.GetDescription();
            double minimalGroundHeight = scheme.GetMinimalAllowedGroundHeight();

            foreach (ModelFeatureCoordinateData<FixedWeir> fixedWeirProperty in fixedWeirsProperties)
            {
                FixedWeir fixedWeir = fixedWeirProperty.Feature;

                if (fixedWeir == null)
                {
                    break;
                }

                ValidationIssue issueLeftColumn = ValidateGroundHeightColumn(fixedWeirProperty, 1, "left", schemeName, minimalGroundHeight);
                if (issueLeftColumn != null)
                {
                    yield return issueLeftColumn;
                }

                ValidationIssue issueRightColumn = ValidateGroundHeightColumn(fixedWeirProperty, 2, "right", schemeName, minimalGroundHeight);
                if (issueRightColumn != null)
                {
                    yield return issueRightColumn;
                }
            }
        }

        private static ValidationIssue ValidateGroundHeightColumn(ModelFeatureCoordinateData<FixedWeir> fixedWeirProperty,
                                                                  int columnIndex,
                                                                  string side,
                                                                  string schemeName,
                                                                  double minimalGroundHeight)
        {
            FixedWeir fixedWeir = fixedWeirProperty.Feature;

            if (fixedWeirProperty.DataColumns[columnIndex].ValueList.Cast<double>()
                                 .Any(value => value < minimalGroundHeight))
            {
                return new ValidationIssue(fixedWeir,
                                           ValidationSeverity.Info,
                                           string.Format(Resources.FixedWeirValidator_Fixed_weir_contains_ground_heights_smaller_than_minimum,
                                                         fixedWeir.Name,
                                                         schemeName,
                                                         side,
                                                         minimalGroundHeight.ToString("0.00", CultureInfo.InvariantCulture)));
            }

            return null;
        }
    }
}