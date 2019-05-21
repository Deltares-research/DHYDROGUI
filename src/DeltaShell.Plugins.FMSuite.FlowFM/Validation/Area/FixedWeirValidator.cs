using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class FixedWeirValidator
    {
        private static string fixedWeirScheme;

        /// <summary>
        /// Validate the fixed weirs and return any encountered issues.
        /// </summary>
        /// <param name="fixedWeirs"> The set of fixed weirs to be evaluated. </param>
        /// <param name="gridExtent"> The grid extent to which the fixed weir should be snapped. </param>
        /// <param name="fixedWeirsProperties"> The fixed weir properties </param>
        /// <param name="scheme">Fixed weir scheme type (numerical, villemonte, tabellenboek or none)</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<FixedWeir> fixedWeirs, Envelope gridExtent, IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties, string scheme)
        {
            fixedWeirScheme = scheme;
            var issues = new List<ValidationIssue>();

            if (fixedWeirScheme == "0") return issues;

            var fixedWeirsPropertyList = fixedWeirsProperties.ToList();
            var availableFixedWeirs = fixedWeirs.ToArray();
            var gridSnappingValidationMessage = new List<ValidationIssue>();
            var sillDepthValidationMessage = new List<ValidationIssue>();

            for (int i = 0; i < availableFixedWeirs.Length; i++)
            {
                gridSnappingValidationMessage = availableFixedWeirs[i].ValidateSnapping(gridExtent).ToList();
                sillDepthValidationMessage = availableFixedWeirs[i].ValidateSillDepths(fixedWeirsPropertyList).ToList();
                issues.AddRange(gridSnappingValidationMessage);
                issues.AddRange(sillDepthValidationMessage);
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

        private static IEnumerable<ValidationIssue> ValidateSillDepths(this FixedWeir fixedWeir,
                                                                       IEnumerable<ModelFeatureCoordinateData<FixedWeir>
                                                                       > fixedWeirsProperties)
        {
            ModelFeatureCoordinateData<FixedWeir> dataToCheck =
                fixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir);
            if (dataToCheck == null)
            {
                yield break;
            }

            var numericalIsSelected = fixedWeirScheme == ((int)FixedWeirSchemes.Scheme6).ToString();
            var villemonteIsSelected = fixedWeirScheme == ((int)FixedWeirSchemes.Scheme9).ToString();
            var tabellenBoekSelected = fixedWeirScheme == ((int)FixedWeirSchemes.Scheme8).ToString();
            var counter = dataToCheck.DataColumns[1].ValueList.Count;

            if (numericalIsSelected || villemonteIsSelected )
            {
                for (var i = 0; i < counter; i++)
                {
                    var valueIsBelowMinimum = (double)dataToCheck.DataColumns[1].ValueList[i] < 0.0 || (double)dataToCheck.DataColumns[2].ValueList[i] < 0.0;
                    if (valueIsBelowMinimum)
                    {
                        yield return new ValidationIssue(fixedWeir,
                                                         ValidationSeverity.Info,
                                                         string.Format(Resources.FixedWeirValidator_ValidateSillDepths_fixed_weir___0___has_unphysical_sill_depths__parts_will_be_ignored_by_dflow_fm_, fixedWeir.Name),
                                                         fixedWeir);
                    }
                }
            }

            if (tabellenBoekSelected)
            {
                for (var i = 0; i < counter; i++)
                {
                    var valueIsBelowMinimum = (double)dataToCheck.DataColumns[1].ValueList[i] < 0.1 || (double)dataToCheck.DataColumns[2].ValueList[i] < 0.1;
                    if (valueIsBelowMinimum)
                    {
                        yield return new ValidationIssue(fixedWeir,
                                                         ValidationSeverity.Info,
                                                         string.Format(Resources.FixedWeirValidator_ValidateSillDepths__0___Fixed_weir_with_type_1_have_a_ground_heights_smaller_than_0_10_m__A_minimum_of_0_10_m_will_be_applied_by_the_computational_core_, fixedWeir.Name),
                                                         fixedWeir);

                    }
                }
            }
        }
    }
}