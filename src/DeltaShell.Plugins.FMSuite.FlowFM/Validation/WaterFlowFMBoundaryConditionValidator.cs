using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
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

            var featureGroups = model.Boundaries.GroupBy(f => f.Name).Where(g => g.Count() > 1);

            issues.AddRange(featureGroups.Select(g => new ValidationIssue("Boundaries", ValidationSeverity.Warning,
                                                                          string.Format(
                                                                              "Boundary name {0} occurs multiple times, this can cause unexpected results",
                                                                              g.Key), model.Boundaries)));

            foreach (var boundaryConditionSet in model.BoundaryConditionSets)
            {
                var flowBoundaryConditions = boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToList();

                if (!flowBoundaryConditions.Any())
                {
                    issues.Add(new ValidationIssue(boundaryConditionSet.Name, ValidationSeverity.Warning,
                        "No boundary condition associated to geometry: this feature will be ignored upon run/save model",
                        boundaryConditionSet));
                    continue;
                }

                issues.AddRange(ValidateSupportPointNames(boundaryConditionSet));
                issues.AddRange(ValidateMorphologyBoundaryHaveHydroBoundaries(boundaryConditionSet));

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
                    Resources.WaterFlowFMBoundaryConditionValidator_ValidateMorphologyBoundaryHaveHydroBoundaries_Morphology_boundary_condition_must_have_a_Hydro_boundary_condition_);
        }

        private static void ValidateFlowBoundaryConditions(WaterFlowFMModel model, List<ValidationIssue> issues)
        {
            foreach (var bcSet in model.BoundaryConditionSets)
            {
                if ( bcSet != null && 
                    bcSet.BoundaryConditions.Where( bc => FlowBoundaryCondition.IsMorphologyBoundary(bc)).ToList().Count > 1)
                {
                    issues.Add(new ValidationIssue(bcSet, ValidationSeverity.Error,
                        "A morphology boundary condition cannot have more than one timeseries per boundary.", bcSet));
                }
            }

            foreach (var boundaryCondition in model.BoundaryConditions.OfType<FlowBoundaryCondition>().Where(fbc => fbc.DataType != BoundaryConditionDataType.Empty))
            {
                var boundaryConditionName = boundaryCondition.VariableDescription;

                var boundaryConditionSet =
                    model.BoundaryConditionSets.First(bcs => bcs.BoundaryConditions.Contains(boundaryCondition));

                if (!boundaryCondition.DataPointIndices.Any())
                {
                    issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Warning,
                        "No point data defined for boundary condition.", boundaryCondition));
                }
                else
                {
                    if (FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(boundaryCondition))
                    {
                        if (boundaryCondition.PointData.Count(pd => pd.GetValues().Count > 0) > 1)
                        {
                            issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                "A morphology boundary condition cannot have more than one point with generated data.", boundaryCondition));
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
                                            "Time series contains forbidden negative values for {0} at point {1}",
                                            boundaryConditionName, supportPointName), boundaryCondition));
                                }
                            }
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                            string.Format("No data defined for {0} at point {1}.", boundaryConditionName,
                                supportPointName), boundaryCondition));
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
                                    "Custom support point name {0} is not yet supported by the dflow-fm kernel, please change it to {1}",
                                    boundaryConditionSet.SupportPointNames[i], expectedName),
                                boundaryConditionSet);
                    }
                }
            }
        }
    }
}
