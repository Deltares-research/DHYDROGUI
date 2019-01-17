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
        /// <summary>
        /// Validate the weirs and return any encountered issues.
        /// </summary>
        /// <param name="model">The model to which the pumps belong.</param>
        /// <param name="weirs">The set of weirs to be evaluated.</param>
        /// <returns> A set of validation issues encountered. </returns>
        public static IEnumerable<ValidationIssue> Validate(WaterFlowFMModel model, IEnumerable<Weir2D> weirs)
        {
            var issues = new List<ValidationIssue>();

            foreach (var weir in weirs)
            {
                if (!model.SnapsToGrid(weir.Geometry))
                {
                    issues.Add(new ValidationIssue(weir,
                                                   ValidationSeverity.Warning,
                                                   $"{weir.Name} is not within grid extend.",
                                                   weirs));
                }

                var result = weir.Validate();
                if (!result.IsValid)
                {
                    issues.Add(new ValidationIssue(weir,
                                                   ValidationSeverity.Error,
                                                   $"{weir.Name}: {result.ValidationException.Messages}",
                                                   weir));
                }

                if (weir.UseCrestLevelTimeSeries)
                {
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

                if (weir.WeirFormula is SimpleWeirFormula weirFormula &&
                    weirFormula.LateralContraction < 0.0)
                {
                    issues.Add(new ValidationIssue(weir,
                                                   ValidationSeverity.Error,
                                                   $"'{weir.Name}': lateral contraction coefficient must be greater than or equal to zero.",
                                                   weir));
                }

                if (weir.WeirFormula is IGatedWeirFormula gatedWeirFormula)
                {
                    // DoorHeight
                    if (gatedWeirFormula.DoorHeight < 0.0)
                    {
                        issues.Add(new ValidationIssue(weir,
                                                       ValidationSeverity.Error,
                                                       $"'{weir.Name}': door height must be greater than or equal to 0.",
                                                       weir));
                    }

                    // HorizontalDoorOpeningWidth
                    if (gatedWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries)
                    {
                        var doorOpeningTimeSeries =
                            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries;
                        if (doorOpeningTimeSeries.Components[0].Values.Cast<object>()
                                                 .Any(value => (double)value < 0.0))
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

                    // LowerEdgeLevel
                    if (gatedWeirFormula.UseLowerEdgeLevelTimeSeries)
                    {
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
                }

                issues.AddIssueIfInvalidCrestWidthValue(weir, weir.CrestWidth, "Crest Width");

                if (weir.WeirFormula is GeneralStructureWeirFormula generalStructureFormula)
                {
                    if (generalStructureFormula.HorizontalDoorOpeningDirection !=
                        GateOpeningDirection.Symmetric)
                    {
                        issues.Add(new ValidationIssue(weir,
                                                       ValidationSeverity.Error,
                                                       $"'{weir.Name}': only symmetric horizontal door opening direction is supported for general structures.",
                                                       weir));
                    }

                    // CrestWidth
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthStructureLeftSide, "Upstream 2 Width");
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthLeftSideOfStructure, "Upstream 1 Width");
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthStructureRightSide, "Downstream 1 Width");
                    issues.AddIssueIfInvalidCrestWidthValue(weir, generalStructureFormula.WidthRightSideOfStructure, "Downstream 2 Width");
                }
            }

            return issues;
        }

        /// <summary>
        /// Add an issue to this issues if any is encountered for the specified <paramref name="crestWidthValue"/>.
        /// </summary>
        /// <param name="issues">The issues to which any encountered issues is added.</param>
        /// <param name="subjectWeir">The weir to which the crest width property belongs.</param>
        /// <param name="crestWidthValue">The crest width value to be evaluated.</param>
        /// <param name="crestWidthPropertyName">The name of the crest width property to be evaluated.</param>
        /// <remarks> Issues is not null. </remarks>
        private static void AddIssueIfInvalidCrestWidthValue(this ICollection<ValidationIssue> issues,
            IWeir subjectWeir,
            double crestWidthValue,
            string crestWidthPropertyName)
        {
            if (double.IsNaN(crestWidthValue))
                issues.Add(new ValidationIssue(subjectWeir,
                    ValidationSeverity.Info,
                    $"{crestWidthPropertyName} for '{subjectWeir.Name}' structure type: {subjectWeir.WeirFormula.GetName2D()}, will be calculated by the computational core.",
                    subjectWeir));
            else if (crestWidthValue <= 0.0)
                issues.Add(new ValidationIssue(subjectWeir,
                    ValidationSeverity.Error,
                    $"{crestWidthPropertyName} for '{subjectWeir.Name}' structure type: {subjectWeir.WeirFormula.GetName2D()}, must be greater than 0.",
                    subjectWeir));
        }
    }
}
