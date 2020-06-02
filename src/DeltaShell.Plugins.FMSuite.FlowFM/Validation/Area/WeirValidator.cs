using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using ValidationAspects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class WeirValidator
    {
        public const string CrestWidthPropertyName = "Crest Width";
        public const string Upstream1WidthPropertyName = "Upstream 1 Width";
        public const string Upstream2WidthPropertyName = "Upstream 2 Width";
        public const string Downstream1WidthPropertyName = "Downstream 1 Width";
        public const string Downstream2WidthPropertyName = "Downstream 2 Width";
        private static DateTime modelStartTime;
        private static DateTime modelStopTime;

        /// <summary>
        /// Validate the weirs and return any encountered issues.
        /// </summary>
        /// <param name="weirs"> The set of weirs to be evaluated. </param>
        /// <param name="gridExtent"> The Envelope that describes the extent of the FM model grid. </param>
        /// <param name="startTime"> The model start time. </param>
        /// <param name="stopTime"> The model stop time. </param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<Weir2D> weirs, Envelope gridExtent,
                                                            DateTime startTime, DateTime stopTime)
        {
            var issues = new List<ValidationIssue>();

            modelStartTime = startTime;
            modelStopTime = stopTime;

            foreach (Weir2D weir in weirs)
            {
                issues.AddRange(weir.ValidateWeirObject());
                issues.AddRange(weir.ValidateSnapping(gridExtent));
                issues.AddRange(weir.ValidateLateralContraction());
                issues.AddRange(weir.ValidateCrestLevel());
                issues.AddRange(weir.ValidateCrestWidth(weir.CrestWidth, CrestWidthPropertyName));

                if (weir.WeirFormula is IGatedWeirFormula gatedWeirFormula)
                {
                    issues.AddRange(weir.ValidateGatedWeir(gatedWeirFormula));
                }

                if (weir.WeirFormula is GeneralStructureWeirFormula generalStructureFormula)
                {
                    issues.AddRange(weir.ValidateGeneralStructure(generalStructureFormula));
                }
            }

            return issues;
        }

        /// <summary>
        /// Add an issue to this issues if any is encountered for the specified <paramref name="crestWidthValue"/>.
        /// </summary>
        /// <param name="subjectWeir"> The weir to which the crest width property belongs. </param>
        /// <param name="crestWidthValue"> The crest width value to be evaluated. </param>
        /// <param name="crestWidthPropertyName"> The name of the crest width property to be evaluated. </param>
        /// <remarks> Issues is not null. </remarks>
        private static IEnumerable<ValidationIssue> ValidateCrestWidth(this IWeir subjectWeir, double crestWidthValue,
                                                                       string crestWidthPropertyName)
        {
            if (double.IsNaN(crestWidthValue))
            {
                yield return new ValidationIssue(subjectWeir,
                                                 ValidationSeverity.Info,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___will_be_calculated_by_the_computational_core_,
                                                     crestWidthPropertyName, subjectWeir.Name,
                                                     subjectWeir.WeirFormula.GetName2D()),
                                                 subjectWeir);
            }
            else if (crestWidthValue <= 0.0)
            {
                yield return new ValidationIssue(subjectWeir,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_,
                                                     crestWidthPropertyName, subjectWeir.Name,
                                                     subjectWeir.WeirFormula.GetName2D()),
                                                 subjectWeir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateWeirObject(this Weir2D weir)
        {
            ValidationResult result = weir.Validate();
            if (!result.IsValid)
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Error,
                                                 $"{weir.Name}: {result.ValidationException.Messages}",
                                                 weir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateSnapping(this Weir2D weir, Envelope gridExtent)
        {
            if (!weir.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Warning,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateSnapping__0__is_not_within_grid_extend_,
                                                     weir.Name),
                                                 weir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateLateralContraction(this Weir2D weir)
        {
            if (weir.WeirFormula is SimpleWeirFormula weirFormula &&
                weirFormula.LateralContraction < 0.0)
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateLateralContraction___0____lateral_contraction_coefficient_must_be_greater_than_or_equal_to_zero_,
                                                     weir.Name),
                                                 weir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateCrestLevel(this Weir2D weir)
        {
            if (!weir.UseCrestLevelTimeSeries)
            {
                yield break;
            }

            if (weir.CrestLevelTimeSeries.Time.Values.Any())
            {
                DateTime startTime = weir.CrestLevelTimeSeries.Time.Values.First();
                DateTime stopTime = weir.CrestLevelTimeSeries.Time.Values.Last();

                if (startTime > modelStartTime || stopTime < modelStopTime)
                {
                    yield return new ValidationIssue(weir,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_span_the_model_run_interval_,
                                                         weir.Name),
                                                     weir);
                }
            }
            else
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_contain_any_values_,
                                                     weir.Name),
                                                 weir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGatedWeir(this Weir2D weir,
                                                                      IGatedWeirFormula gatedWeirFormula)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(weir.ValidateDoorHeight(gatedWeirFormula));
            issues.AddRange(weir.ValidateHorizontalDoorOpeningWidth(gatedWeirFormula));
            issues.AddRange(weir.ValidateLowerEdgeLevel(gatedWeirFormula));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateGeneralStructure(
            this Weir2D weir, GeneralStructureWeirFormula generalStructureFormula)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(weir.ValidateHorizontalDoorOpeningDirection(generalStructureFormula));
            issues.AddRange(weir.ValidateCrestWidth(generalStructureFormula.WidthStructureLeftSide,
                                                    Upstream2WidthPropertyName));
            issues.AddRange(weir.ValidateCrestWidth(generalStructureFormula.WidthLeftSideOfStructure,
                                                    Upstream1WidthPropertyName));
            issues.AddRange(weir.ValidateCrestWidth(generalStructureFormula.WidthStructureRightSide,
                                                    Downstream1WidthPropertyName));
            issues.AddRange(weir.ValidateCrestWidth(generalStructureFormula.WidthRightSideOfStructure,
                                                    Downstream2WidthPropertyName));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateHorizontalDoorOpeningDirection(
            this Weir2D weir, GeneralStructureWeirFormula generalStructureFormula)
        {
            if (generalStructureFormula.HorizontalDoorOpeningDirection
                != GateOpeningDirection.Symmetric)
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateHorizontalDoorOpeningDirection___0____only_symmetric_horizontal_door_opening_direction_is_supported_for_general_structures_,
                                                     weir.Name),
                                                 weir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateLowerEdgeLevel(
            this Weir2D weir, IGatedWeirFormula gatedWeirFormula)
        {
            if (!gatedWeirFormula.UseLowerEdgeLevelTimeSeries)
            {
                yield break;
            }

            TimeSeries lowerEdgeLevelTimeSeries = gatedWeirFormula.LowerEdgeLevelTimeSeries;
            if (lowerEdgeLevelTimeSeries.Time.Values.Any())
            {
                DateTime startTime = lowerEdgeLevelTimeSeries.Time.Values.First();
                DateTime stopTime = lowerEdgeLevelTimeSeries.Time.Values.Last();

                if (startTime > modelStartTime || stopTime < modelStopTime)
                {
                    yield return new ValidationIssue(weir,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WeirValidator_ValidateLowerEdgeLevel___0____lower_edge_level_time_series_does_not_span_the_model_run_interval_,
                                                         weir.Name),
                                                     weir);
                }
            }
            else
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateLowerEdgeLevel___0____lower_edge_level_time_series_does_not_contain_any_values_,
                                                     weir.Name),
                                                 weir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateHorizontalDoorOpeningWidth(
            this Weir2D weir, IGatedWeirFormula gatedWeirFormula)
        {
            if (gatedWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries)
            {
                TimeSeries doorOpeningTimeSeries =
                    gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries;
                if (doorOpeningTimeSeries.Components[0].Values.Cast<object>()
                                         .Any(value => (double) value < 0.0))
                {
                    yield return new ValidationIssue(weir,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_time_series_values_must_be_greater_than_or_equal_to_0_,
                                                         weir.Name),
                                                     weir);
                }

                if (doorOpeningTimeSeries.Time.Values.Any())
                {
                    DateTime startTime = doorOpeningTimeSeries.Time.Values.First();
                    DateTime stopTime = doorOpeningTimeSeries.Time.Values.Last();

                    if (startTime > modelStartTime || stopTime < modelStopTime)
                    {
                        yield return new ValidationIssue(weir,
                                                         ValidationSeverity.Error,
                                                         string.Format(
                                                             Resources
                                                                 .WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_time_series_does_not_span_the_model_run_interval_,
                                                             weir.Name),
                                                         weir);
                    }
                }
                else
                {
                    yield return new ValidationIssue(weir,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_time_series_does_not_contain_any_values_,
                                                         weir.Name),
                                                     weir);
                }
            }
            else if (gatedWeirFormula.HorizontalDoorOpeningWidth < 0.0)
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_must_be_greater_than_or_equal_to_0_,
                                                     weir.Name),
                                                 weir);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateDoorHeight(this Weir2D weir,
                                                                       IGatedWeirFormula gatedWeirFormula)
        {
            if (gatedWeirFormula.DoorHeight < 0.0)
            {
                yield return new ValidationIssue(weir,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateDoorHeight___0____door_height_must_be_greater_than_or_equal_to_0_,
                                                     weir.Name),
                                                 weir);
            }
        }
    }
}