using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using ValidationAspects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class StructureValidator
    {
        public const string CrestWidthPropertyName = "Crest Width";
        public const string Upstream1WidthPropertyName = "Upstream 1 Width";
        public const string Upstream2WidthPropertyName = "Upstream 2 Width";
        public const string Downstream1WidthPropertyName = "Downstream 1 Width";
        public const string Downstream2WidthPropertyName = "Downstream 2 Width";
        private static DateTime modelStartTime;
        private static DateTime modelStopTime;

        /// <summary>
        /// Validate the structures and return any encountered issues.
        /// </summary>
        /// <param name="structures"> The set of structures to be evaluated. </param>
        /// <param name="gridExtent"> The Envelope that describes the extent of the FM model grid. </param>
        /// <param name="startTime"> The model start time. </param>
        /// <param name="stopTime"> The model stop time. </param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(this IEnumerable<IStructure> structures, 
                                                            Envelope gridExtent,
                                                            DateTime startTime, 
                                                            DateTime stopTime)
        {
            var issues = new List<ValidationIssue>();

            modelStartTime = startTime;
            modelStopTime = stopTime;

            foreach (IStructure structure in structures)
            {
                issues.AddRange(structure.ValidateWeirObject());
                issues.AddRange(structure.ValidateSnapping(gridExtent));
                issues.AddRange(structure.ValidateLateralContraction());
                issues.AddRange(structure.ValidateCrestLevel());
                issues.AddRange(structure.ValidateCrestWidth(structure.CrestWidth, CrestWidthPropertyName));

                if (structure.Formula is IGatedStructureFormula gatedWeirFormula)
                {
                    issues.AddRange(structure.ValidateGatedWeir(gatedWeirFormula));
                }

                if (structure.Formula is GeneralStructureFormula generalStructureFormula)
                {
                    issues.AddRange(structure.ValidateGeneralStructure(generalStructureFormula));
                }
            }

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateCrestWidth(this IStructure structure, 
                                                                       double crestWidthValue,
                                                                       string crestWidthPropertyName)
        {
            if (double.IsNaN(crestWidthValue))
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Info,
                                                 string.Format(
                                                     Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___will_be_calculated_by_the_computational_core_,
                                                     crestWidthPropertyName, structure.Name,
                                                     structure.Formula.Name),
                                                 structure);
            }
            else if (crestWidthValue <= 0.0)
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_,
                                                     crestWidthPropertyName, structure.Name,
                                                     structure.Formula.Name),
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateWeirObject(this IStructure structure)
        {
            ValidationResult result = structure.Validate();
            if (!result.IsValid)
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 $"{structure.Name}: {result.ValidationException.Messages}",
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateSnapping(this IStructure structure, Envelope gridExtent)
        {
            if (!structure.Geometry.SnapsToFlowFmGrid(gridExtent))
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Warning,
                                                 string.Format(
                                                     Resources.WeirValidator_ValidateSnapping__0__is_not_within_grid_extend_,
                                                     structure.Name),
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateLateralContraction(this IStructure structure)
        {
            if (structure.Formula is SimpleWeirFormula weirFormula &&
                weirFormula.LateralContraction < 0.0)
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources.WeirValidator_ValidateLateralContraction___0____lateral_contraction_coefficient_must_be_greater_than_or_equal_to_zero_,
                                                     structure.Name),
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateCrestLevel(this IStructure structure)
        {
            if (!structure.UseCrestLevelTimeSeries)
            {
                yield break;
            }

            if (structure.CrestLevelTimeSeries.Time.Values.Any())
            {
                DateTime startTime = structure.CrestLevelTimeSeries.Time.Values.First();
                DateTime stopTime = structure.CrestLevelTimeSeries.Time.Values.Last();

                if (startTime > modelStartTime || stopTime < modelStopTime)
                {
                    yield return new ValidationIssue(structure,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources.WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_span_the_model_run_interval_,
                                                         structure.Name),
                                                     structure);
                }
            }
            else
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources.WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_contain_any_values_,
                                                     structure.Name),
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGatedWeir(this IStructure structure,
                                                                      IGatedStructureFormula gatedStructureFormula)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(structure.ValidateGateHeight(gatedStructureFormula));
            issues.AddRange(structure.ValidateHorizontalGateOpeningWidth(gatedStructureFormula));
            issues.AddRange(structure.ValidateGateLowerEdgeLevel(gatedStructureFormula));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateGeneralStructure(this IStructure structure, 
                                                                             GeneralStructureFormula generalStructureFormula)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(structure.ValidateGateOpeningHorizontalDirection(generalStructureFormula));
            issues.AddRange(structure.ValidateCrestWidth(generalStructureFormula.Upstream2Width,
                                                    Upstream2WidthPropertyName));
            issues.AddRange(structure.ValidateCrestWidth(generalStructureFormula.Upstream1Width,
                                                    Upstream1WidthPropertyName));
            issues.AddRange(structure.ValidateCrestWidth(generalStructureFormula.Downstream1Width,
                                                    Downstream1WidthPropertyName));
            issues.AddRange(structure.ValidateCrestWidth(generalStructureFormula.Downstream2Width,
                                                    Downstream2WidthPropertyName));

            return issues;
        }

        private static IEnumerable<ValidationIssue> ValidateGateOpeningHorizontalDirection(this IStructure structure, 
                                                                                           GeneralStructureFormula generalStructureFormula)
        {
            if (generalStructureFormula.GateOpeningHorizontalDirection
                != GateOpeningDirection.Symmetric)
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources.WeirValidator_ValidateHorizontalGateOpeningDirection___0____only_symmetric_gate_opening_horizontal_direction_is_supported_for_general_structures_,
                                                     structure.Name),
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGateLowerEdgeLevel(this IStructure structure, 
                                                                           IGatedStructureFormula gatedStructureFormula)
        {
            if (!gatedStructureFormula.UseGateLowerEdgeLevelTimeSeries)
            {
                yield break;
            }

            TimeSeries gateLowerEdgeLevelTimeSeries = gatedStructureFormula.GateLowerEdgeLevelTimeSeries;
            if (gateLowerEdgeLevelTimeSeries.Time.Values.Any())
            {
                DateTime startTime = gateLowerEdgeLevelTimeSeries.Time.Values.First();
                DateTime stopTime = gateLowerEdgeLevelTimeSeries.Time.Values.Last();

                if (startTime > modelStartTime || stopTime < modelStopTime)
                {
                    yield return new ValidationIssue(structure,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources.WeirValidator_ValidateGateLowerEdgeLevel___0____gate_lower_edge_level_time_series_does_not_span_the_model_run_interval_,
                                                         structure.Name),
                                                     structure);
                }
            }
            else
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources.WeirValidator_ValidateGateLowerEdgeLevel___0____gate_lower_edge_level_time_series_does_not_contain_any_values_,
                                                     structure.Name),
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateHorizontalGateOpeningWidth(
            this IStructure structure, IGatedStructureFormula gatedStructureFormula)
        {
            if (gatedStructureFormula.UseHorizontalGateOpeningWidthTimeSeries)
            {
                TimeSeries gateOpeningTimeSeries =
                    gatedStructureFormula.HorizontalGateOpeningWidthTimeSeries;
                if (gateOpeningTimeSeries.Components[0].Values.Cast<object>()
                                         .Any(value => (double) value < 0.0))
                {
                    yield return new ValidationIssue(structure,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_time_series_values_must_be_greater_than_or_equal_to_0_,
                                                         structure.Name),
                                                     structure);
                }

                if (gateOpeningTimeSeries.Time.Values.Any())
                {
                    DateTime startTime = gateOpeningTimeSeries.Time.Values.First();
                    DateTime stopTime = gateOpeningTimeSeries.Time.Values.Last();

                    if (startTime > modelStartTime || stopTime < modelStopTime)
                    {
                        yield return new ValidationIssue(structure,
                                                         ValidationSeverity.Error,
                                                         string.Format(
                                                             Resources
                                                                 .WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_time_series_does_not_span_the_model_run_interval_,
                                                             structure.Name),
                                                         structure);
                    }
                }
                else
                {
                    yield return new ValidationIssue(structure,
                                                     ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_time_series_does_not_contain_any_values_,
                                                         structure.Name),
                                                     structure);
                }
            }
            else if (gatedStructureFormula.HorizontalGateOpeningWidth < 0.0)
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_must_be_greater_than_or_equal_to_0_,
                                                     structure.Name),
                                                 structure);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGateHeight(this IStructure structure,
                                                                       IGatedStructureFormula gatedStructureFormula)
        {
            if (gatedStructureFormula.GateHeight < 0.0)
            {
                yield return new ValidationIssue(structure,
                                                 ValidationSeverity.Error,
                                                 string.Format(
                                                     Resources
                                                         .WeirValidator_ValidateGateHeight___0____gate_height_must_be_greater_than_or_equal_to_0_,
                                                     structure.Name),
                                                 structure);
            }
        }
    }
}