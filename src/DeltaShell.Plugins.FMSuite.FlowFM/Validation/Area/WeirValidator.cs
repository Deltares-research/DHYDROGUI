using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using ValidationAspects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area
{
    public static class WeirValidator
    {
        private static IList<ValidationIssue> issues;

        /// <summary>
        /// Validate the weirs and return any encountered issues.
        /// </summary>
        /// <param name="model">The model to which the weirs belong.</param>
        /// <param name="weirs">The set of weirs to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(WaterFlowFMModel model, IEnumerable<Weir2D> weirs)
        {
            issues = new List<ValidationIssue>();

            foreach (var weir in weirs)
            {
                weir.ValidateWeirObject();
                weir.ValidateSnapping(model);
                weir.ValidateLateralContraction();
                weir.ValidateCrestLevel(model);
                weir.ValidateCrestWidth(weir.CrestWidth, "Crest Width");

                if (weir.WeirFormula is IGatedWeirFormula gatedWeirFormula)
                {
                    weir.ValidateGatedWeir(gatedWeirFormula, model);
                }

                if (weir.WeirFormula is GeneralStructureWeirFormula generalStructureFormula)
                {
                    weir.ValidateGeneralStructure(generalStructureFormula);
                }
            }

            return issues;
        }

        /// <summary>
        /// Add an issue to this issues if any is encountered for the specified <paramref name="crestWidthValue"/>.
        /// </summary>
        /// <param name="subjectWeir">The weir to which the crest width property belongs.</param>
        /// <param name="crestWidthValue">The crest width value to be evaluated.</param>
        /// <param name="crestWidthPropertyName">The name of the crest width property to be evaluated.</param>
        /// <remarks> Issues is not null. </remarks>
        private static void ValidateCrestWidth(this IWeir subjectWeir, double crestWidthValue, string crestWidthPropertyName)
        {
            if (double.IsNaN(crestWidthValue))
            {
                issues.Add(new ValidationIssue(subjectWeir,
                    ValidationSeverity.Info,
                    $"{crestWidthPropertyName} for '{subjectWeir.Name}' structure type: {subjectWeir.WeirFormula.GetName2D()}, will be calculated by the computational core.",
                    subjectWeir));
            }
            else if (crestWidthValue <= 0.0)
            {
                issues.Add(new ValidationIssue(subjectWeir,
                    ValidationSeverity.Error,
                    $"{crestWidthPropertyName} for '{subjectWeir.Name}' structure type: {subjectWeir.WeirFormula.GetName2D()}, must be greater than 0.",
                    subjectWeir));
            }
        }

        private static void ValidateWeirObject(this Weir2D weir)
        {
            var result = weir.Validate();
            if (!result.IsValid)
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Error,
                    $"{weir.Name}: {result.ValidationException.Messages}",
                    weir));
            }
        }

        private static void ValidateSnapping(this Weir2D weir, WaterFlowFMModel model)
        {
            if (!weir.Geometry.SnapsToFlowFmGrid(model.GridExtent))
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Warning,
                    $"{weir.Name} is not within grid extend.",
                    weir));
            }
        }

        private static void ValidateLateralContraction(this Weir2D weir)
        {
            if (weir.WeirFormula is SimpleWeirFormula weirFormula &&
                weirFormula.LateralContraction < 0.0)
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Error,
                    $"'{weir.Name}': lateral contraction coefficient must be greater than or equal to zero.",
                    weir));
            }
        }

        private static void ValidateCrestLevel(this Weir2D weir, WaterFlowFMModel model)
        {
            if (!weir.UseCrestLevelTimeSeries) return;

            if (weir.CrestLevelTimeSeries.Time.Values.Any())
            {
                var startTime = weir.CrestLevelTimeSeries.Time.Values.First();
                var stopTime = weir.CrestLevelTimeSeries.Time.Values.Last();

                if (startTime > model.StartTime || stopTime < model.StopTime)
                {
                    issues.Add(new ValidationIssue(weir,
                        ValidationSeverity.Error,
                        $"'{weir.Name}': crest level time series does not span the model run interval.",
                        weir));
                }
            }
            else
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Error,
                    $"'{weir.Name}': crest level time series does not contain any values.",
                    weir));
            }
        }

        private static void ValidateGatedWeir(this Weir2D weir, IGatedWeirFormula gatedWeirFormula, WaterFlowFMModel model)
        {
            weir.ValidateDoorHeight(gatedWeirFormula);
            weir.ValidateHorizontalDoorOpeningWidth(gatedWeirFormula, model);
            weir.ValidateLowerEdgeLevel(gatedWeirFormula, model);
        }

        private static void ValidateGeneralStructure(this Weir2D weir, GeneralStructureWeirFormula generalStructureFormula)
        {
            weir.ValidateHorizontalDoorOpeningDirection(generalStructureFormula);
            weir.ValidateCrestWidth(generalStructureFormula.WidthStructureLeftSide, "Upstream 2 Width");
            weir.ValidateCrestWidth(generalStructureFormula.WidthLeftSideOfStructure, "Upstream 1 Width");
            weir.ValidateCrestWidth(generalStructureFormula.WidthStructureRightSide, "Downstream 1 Width");
            weir.ValidateCrestWidth(generalStructureFormula.WidthRightSideOfStructure, "Downstream 2 Width");
        }

        private static void ValidateHorizontalDoorOpeningDirection(this Weir2D weir, GeneralStructureWeirFormula generalStructureFormula)
        {
            if (generalStructureFormula.HorizontalDoorOpeningDirection
                != GateOpeningDirection.Symmetric)
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Error,
                    $"'{weir.Name}': only symmetric horizontal door opening direction is supported for general structures.",
                    weir));
            }
        }

        private static void ValidateLowerEdgeLevel(this Weir2D weir, IGatedWeirFormula gatedWeirFormula, WaterFlowFMModel model)
        {
            if (!gatedWeirFormula.UseLowerEdgeLevelTimeSeries) return;

            var lowerEdgeLevelTimeSeries = gatedWeirFormula.LowerEdgeLevelTimeSeries;
            if (lowerEdgeLevelTimeSeries.Time.Values.Any())
            {
                var startTime = lowerEdgeLevelTimeSeries.Time.Values.First();
                var stopTime = lowerEdgeLevelTimeSeries.Time.Values.Last();

                if (startTime > model.StartTime || stopTime < model.StopTime)
                {
                    issues.Add(new ValidationIssue(weir,
                        ValidationSeverity.Error,
                        $"'{weir.Name}': lower edge level time series does not span the model run interval.",
                        weir));
                }
            }
            else
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Error,
                    $"'{weir.Name}': lower edge level time series does not contain any values.",
                    weir));
            }
        }

        private static void ValidateHorizontalDoorOpeningWidth(this Weir2D weir, IGatedWeirFormula gatedWeirFormula, WaterFlowFMModel model)
        {
            if (gatedWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries)
            {
                var doorOpeningTimeSeries =
                    gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries;
                if (doorOpeningTimeSeries.Components[0].Values.Cast<object>()
                    .Any(value => (double) value < 0.0))
                {
                    issues.Add(new ValidationIssue(weir,
                        ValidationSeverity.Error,
                        $"'{weir.Name}': opening width time series values must be greater than or equal to 0.",
                        weir));
                }

                if (doorOpeningTimeSeries.Time.Values.Any())
                {
                    var startTime = doorOpeningTimeSeries.Time.Values.First();
                    var stopTime = doorOpeningTimeSeries.Time.Values.Last();

                    if (startTime > model.StartTime || stopTime < model.StopTime)
                    {
                        issues.Add(new ValidationIssue(weir,
                            ValidationSeverity.Error,
                            $"'{weir.Name}': opening width time series does not span the model run interval.",
                            weir));
                    }
                }
                else
                {
                    issues.Add(new ValidationIssue(weir,
                        ValidationSeverity.Error,
                        $"'{weir.Name}': opening width time series does not contain any values.",
                        weir));
                }
            }
            else if (gatedWeirFormula.HorizontalDoorOpeningWidth < 0.0)
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Error,
                    $"'{weir.Name}': opening width must be greater than or equal to 0.",
                    weir));
            }
        }

        private static void ValidateDoorHeight(this Weir2D weir, IGatedWeirFormula gatedWeirFormula)
        {
            if (gatedWeirFormula.DoorHeight < 0.0)
            {
                issues.Add(new ValidationIssue(weir,
                    ValidationSeverity.Error,
                    $"'{weir.Name}': door height must be greater than or equal to 0.",
                    weir));
            }
        }
    }
}
