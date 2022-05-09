using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMBoundaryConditionValidator    
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            IEnumerable<string> duplicateBoundaryNames = model.Boundaries.Duplicates(b => b.Name);

            issues.AddRange(duplicateBoundaryNames.Select(g => new ValidationIssue("Boundaries", ValidationSeverity.Warning,
                                                                          string.Format(
                                                                              "Boundary name {0} occurs multiple times, this can cause unexpected results",
                                                                              g), model.Boundaries)));

            foreach (var boundaryConditionSet in model.BoundaryConditionSets)
            {
                var flowBoundaryConditions = boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToList();

                if (!flowBoundaryConditions.Any())
                {
                    issues.Add(new ValidationIssue(boundaryConditionSet.Name, ValidationSeverity.Error,
                        string.Format(Resources.WaterFlowFMBoundaryConditionValidator_Validate_Boundary___0___does_not_contain_a_boundary_condition, boundaryConditionSet.Feature.Name),
                        boundaryConditionSet));
                    continue;
                }

                issues.AddRange(ValidateSupportPointNames(boundaryConditionSet));
                issues.AddRange(ValidateMorphologyBoundaryHaveHydroBoundaries(boundaryConditionSet));
                issues.AddRange(ValidateSedimentConcentrationBoundaryHaveHydroBoundaries(boundaryConditionSet));

                var quantities = flowBoundaryConditions.Select(fbc => fbc.FlowQuantity);

                var constrainedQuantities =
                    quantities.Except(FlowBoundaryCondition.AlwaysAllowedQuantities).OrderBy(q => q).ToList();

                if (constrainedQuantities.Any())
                {
                    if (FlowBoundaryCondition.ValidBoundaryConditionCombinations.Select(l => l.OrderBy(q => q))
                        .Any(l => l.SequenceEqual(constrainedQuantities)))
                    {
                        continue;
                    }
                    issues.Add(new ValidationIssue(boundaryConditionSet.Name, ValidationSeverity.Error,
                        "Invalid combination of boundary condition quantities detected.",
                        boundaryConditionSet));
                }
            }

            ValidateFlowBoundaryConditions(model, issues);
            return new ValidationReport("Water flow FM model boundary conditions", issues);
        }

        private static IEnumerable<ValidationIssue> ValidateMorphologyBoundaryHaveHydroBoundaries(BoundaryConditionSet boundaryConditionSet)
        {
            if (boundaryConditionSet.BoundaryConditions.All(bc => FlowBoundaryCondition.IsMorphologyBoundary(bc)))
                yield return new ValidationIssue(boundaryConditionSet, ValidationSeverity.Error,
                    Resources.WaterFlowFMBoundaryConditionValidator_ValidateMorphologyBoundaryHaveHydroBoundaries_Morphology_boundary_condition_must_have_a_Hydro_boundary_condition_, boundaryConditionSet);
        }

        private static IEnumerable<ValidationIssue> ValidateSedimentConcentrationBoundaryHaveHydroBoundaries(BoundaryConditionSet boundaryConditionSet)
        {
            //Check if any other snapped boundary at this location have a flow boundary condition in it.
            yield break;
            var flowBoundaryConditions = boundaryConditionSet.BoundaryConditions.Cast<FlowBoundaryCondition>().ToList();
            if (flowBoundaryConditions.Count == boundaryConditionSet.BoundaryConditions.Count && flowBoundaryConditions.All(bc => bc.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration))
                yield return new ValidationIssue(boundaryConditionSet, ValidationSeverity.Error,
                    Resources.WaterFlowFMBoundaryConditionValidator_ValidateSedimentConcentrationBoundaryHaveHydroBoundaries_Sediment_concentration_boundary_condition_must_have_a_Hydro_boundary_condition_);
        }

        private static void ValidateFlowBoundaryConditions(WaterFlowFMModel model, List<ValidationIssue> issues)
        {
            foreach (var bcSet in model.BoundaryConditionSets)
            {
                if ( bcSet != null && 
                    bcSet.BoundaryConditions.Where( bc => FlowBoundaryCondition.IsMorphologyBoundary(bc)).ToList().Count > 1)
                {
                    issues.Add(new ValidationIssue(bcSet, ValidationSeverity.Error,
                        Resources.WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_A_morphology_boundary_condition_cannot_have_more_than_one_timeseries_per_boundary_, bcSet));
                }
            }

            foreach (var boundaryCondition in model.BoundaryConditions.OfType<FlowBoundaryCondition>().Where(fbc => fbc.DataType != BoundaryConditionDataType.Empty))
            {
                var boundaryConditionName = boundaryCondition.VariableDescription;

                var boundaryConditionSet =
                    model.BoundaryConditionSets.First(bcs => bcs.BoundaryConditions.Contains(boundaryCondition));

                if (!boundaryCondition.DataPointIndices.Any())
                {
                    issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                        string.Format(Resources.WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_No_data_defined_for_boundary_condition___0___at_boundary___1__, boundaryConditionName, boundaryCondition.FeatureName),
                        boundaryCondition));
                }
                else
                {
                    if (FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(boundaryCondition))
                    {
                        if (boundaryCondition.PointData.Count(pd => pd.GetValues().Count > 0) > 1)
                        {
                            issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                Resources.WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_A_morphology_boundary_condition_cannot_have_more_than_one_point_with_generated_data_, boundaryCondition));
                        }
                    }
                    else
                    {
                        ValidateBoundaryConditionPointIndex(model, issues, boundaryCondition, boundaryConditionSet, boundaryConditionName);
                    }
                }
            }
        }

        private static void ValidateBoundaryConditionPointIndex(WaterFlowFMModel model, List<ValidationIssue> issues,
            FlowBoundaryCondition boundaryCondition, BoundaryConditionSet boundaryConditionSet, string boundaryConditionName)
        {
            foreach (var pointIndex in boundaryCondition.DataPointIndices)
            {
                var function = boundaryCondition.GetDataAtPoint(pointIndex);

                var supportPointName = boundaryConditionSet.SupportPointNames[pointIndex];

                if (boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
                {
                    var timeValues =
                        function.Arguments.First(a => a.ValueType == typeof(DateTime)).GetValues<DateTime>();
                    if (timeValues.Any())
                    {
                        var lowerBound = timeValues.First();
                        var upperBound = timeValues.Last();

                        if (lowerBound > model.StartTime || upperBound < model.StopTime)
                        {
                            issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                string.Format("Time series does not span model run interval for {0} at point {1}.",
                                    boundaryConditionName, supportPointName), boundaryCondition));
                        }
                        if (boundaryCondition.StrictlyPositive)
                        {
                            foreach (var component in function.Components)
                            {
                                if ((double) component.MinValue < 0.0)
                                {
                                    issues.Add(new ValidationIssue(boundaryConditionName,
                                        ValidationSeverity.Error,
                                        string.Format(
                                            Resources.WaterFlowFMBoundaryConditionValidator_ValidateBoundaryConditionPointIndex_Time_series_contains_forbidden_negative_values_for__0__at_point__1_,
                                            boundaryConditionName, supportPointName), boundaryCondition));
                                }
                            }
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                            string.Format(Resources.WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_No_data_defined_for_boundary_condition___0___at_boundary___1__,
                                boundaryConditionName, supportPointName),
                            boundaryCondition));
                    }
                }

                if (boundaryCondition.DataType == BoundaryConditionDataType.AstroComponents ||
                    boundaryCondition.DataType == BoundaryConditionDataType.AstroCorrection)
                {
                    foreach (var astroComponent in function.Arguments[0].Values.Cast<string>())
                    {
                        if (HarmonicComponent.DefaultAstroComponentsRadPerHour.Keys.Contains(astroComponent))
                        {
                            continue;
                        }

                        issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Warning,
                            string.Format("Unknown astronomical component {0} detected for {1} at point {2}",
                                astroComponent, boundaryConditionName, supportPointName)));
                    }
                }

                var depthProfile = boundaryCondition.GetDepthLayerDefinitionAtPoint(pointIndex);

                if (depthProfile == null) continue;

                issues.AddRange(VerticalProfileValidator.ValidateVerticalProfile(boundaryConditionName,
                    depthProfile, boundaryCondition,
                    supportPointName));
            }
        }

        // Remove whenever the ec-module supports custom boundary point names
        private static IEnumerable<ValidationIssue> ValidateSupportPointNames(BoundaryConditionSet boundaryConditionSet)
        {
            for (var i = 0; i < boundaryConditionSet.SupportPointNames.Count; i++)
            {
                var boundaryConditions = (i == 0)
                    ? boundaryConditionSet.BoundaryConditions
                    : boundaryConditionSet.BoundaryConditions.Where(bc => !bc.IsHorizontallyUniform);
                
                if (boundaryConditions.Any(bc => bc.GetDataAtPoint(i) != null))
                {
                    var expectedName = BoundaryConditionSet.DefaultLocationName(boundaryConditionSet.Feature, i);
                    if (boundaryConditionSet.SupportPointNames[i] != expectedName)
                    {
                        yield return
                            new ValidationIssue(boundaryConditionSet, ValidationSeverity.Error,
                                string.Format(
                                    Resources.WaterFlowFMBoundaryConditionValidator_ValidateSupportPointNames_Custom_support_point_names_are_not_supported_by_gui,
                                    i + 1, boundaryConditionSet.SupportPointNames[i], expectedName),
                                boundaryConditionSet);
                    }
                }
            }
        }
    }
}
