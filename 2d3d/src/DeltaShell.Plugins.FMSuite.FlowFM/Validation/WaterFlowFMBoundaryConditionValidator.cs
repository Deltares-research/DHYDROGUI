using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMBoundaryConditionValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            var issues = new List<ValidationIssue>();

            IEnumerable<IGrouping<string, Feature2D>> featureGroups =
                model.Boundaries.GroupBy(f => f.Name).Where(g => g.Count() > 1);

            issues.AddRange(featureGroups.Select(g => new ValidationIssue("Boundaries", ValidationSeverity.Warning,
                                                                          string.Format(
                                                                              "Boundary name {0} occurs multiple times, this can cause unexpected results",
                                                                              g.Key), model.Boundaries)));

            foreach (BoundaryConditionSet boundaryConditionSet in model.BoundaryConditionSets)
            {
                List<FlowBoundaryCondition> flowBoundaryConditions =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToList();

                if (!flowBoundaryConditions.Any())
                {
                    issues.Add(new ValidationIssue(boundaryConditionSet.Name, ValidationSeverity.Error,
                                                   string.Format(
                                                       Resources
                                                           .WaterFlowFMBoundaryConditionValidator_Validate_Boundary___0___does_not_contain_a_boundary_condition,
                                                       boundaryConditionSet.Feature.Name),
                                                   boundaryConditionSet));
                    continue;
                }

                issues.AddRange(ValidateSupportPointNames(boundaryConditionSet));
                issues.AddRange(ValidateMorphologyBoundaryHaveHydroBoundaries(boundaryConditionSet));

                IEnumerable<FlowBoundaryQuantityType> quantities =
                    flowBoundaryConditions.Select(fbc => fbc.FlowQuantity);

                List<FlowBoundaryQuantityType> constrainedQuantities =
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

        private static IEnumerable<ValidationIssue> ValidateMorphologyBoundaryHaveHydroBoundaries(
            BoundaryConditionSet boundaryConditionSet)
        {
            if (boundaryConditionSet.BoundaryConditions.All(bc => FlowBoundaryCondition.IsMorphologyBoundary(bc)))
            {
                yield return new ValidationIssue(boundaryConditionSet, ValidationSeverity.Error,
                                                 Resources
                                                     .WaterFlowFMBoundaryConditionValidator_ValidateMorphologyBoundaryHaveHydroBoundaries_Morphology_boundary_condition_must_have_a_Hydro_boundary_condition_);
            }
        }

        private static void ValidateFlowBoundaryConditions(WaterFlowFMModel model, List<ValidationIssue> issues)
        {
            foreach (BoundaryConditionSet bcSet in model.BoundaryConditionSets)
            {
                if (bcSet != null &&
                    bcSet.BoundaryConditions.Where(bc => FlowBoundaryCondition.IsMorphologyBoundary(bc)).ToList()
                         .Count > 1)
                {
                    issues.Add(new ValidationIssue(bcSet, ValidationSeverity.Error,
                                                   Resources
                                                       .WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_A_morphology_boundary_condition_cannot_have_more_than_one_timeseries_per_boundary_,
                                                   bcSet));
                }
            }

            foreach (FlowBoundaryCondition boundaryCondition in model
                                                                .BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                                .Where(fbc => fbc.DataType !=
                                                                              BoundaryConditionDataType.Empty))
            {
                string boundaryConditionName = boundaryCondition.VariableDescription;

                BoundaryConditionSet boundaryConditionSet =
                    model.BoundaryConditionSets.First(bcs => bcs.BoundaryConditions.Contains(boundaryCondition));

                if (!boundaryCondition.DataPointIndices.Any())
                {
                    issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                                   string.Format(
                                                       Resources
                                                           .WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_No_data_defined_for_boundary_condition___0___at_boundary___1__,
                                                       boundaryConditionName, boundaryCondition.FeatureName),
                                                   boundaryCondition));
                }
                else
                {
                    if (FlowBoundaryCondition.MorphologyBoundaryConditionHasGeneratedData(boundaryCondition))
                    {
                        if (boundaryCondition.PointData.Count(pd => pd.GetValues().Count > 0) > 1)
                        {
                            issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                                           Resources
                                                               .WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_A_morphology_boundary_condition_cannot_have_more_than_one_point_with_generated_data_,
                                                           boundaryCondition));
                        }
                    }
                    else
                    {
                        ValidateBoundaryConditionPointIndex(model, issues, boundaryCondition, boundaryConditionSet,
                                                            boundaryConditionName);
                    }
                }

                ValidateBoundaryConditionTimeZone(issues, boundaryCondition, boundaryConditionName);
            }
        }

        private static void ValidateBoundaryConditionTimeZone(List<ValidationIssue> issues, FlowBoundaryCondition boundaryCondition, string boundaryConditionName)
        {
            if(OutsideAllowedTimeZoneRange(boundaryCondition))
            {
                issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                               string.Format(Resources.WaterFlowFMBoundaryConditionValidator_ValidateBoundaryConditionTimeZone_Time_zone_of_boundary_condition___0___falls_outside_of_allowed_range__12_00_and__12_00, boundaryConditionName),
                                               boundaryCondition));
            }
        }

        private static bool OutsideAllowedTimeZoneRange(FlowBoundaryCondition boundaryCondition)
        {
            return boundaryCondition.TimeZone > new TimeSpan(12, 0, 0) || boundaryCondition.TimeZone < new TimeSpan(-12, 0, 0);
        }

        private static void ValidateBoundaryConditionPointIndex(WaterFlowFMModel model, List<ValidationIssue> issues,
                                                                FlowBoundaryCondition boundaryCondition,
                                                                BoundaryConditionSet boundaryConditionSet,
                                                                string boundaryConditionName)
        {
            foreach (int pointIndex in boundaryCondition.DataPointIndices)
            {
                IFunction function = boundaryCondition.GetDataAtPoint(pointIndex);

                string supportPointName = boundaryConditionSet.SupportPointNames[pointIndex];

                if (boundaryCondition.DataType == BoundaryConditionDataType.TimeSeries)
                {
                    IMultiDimensionalArray<DateTime> timeValues =
                        function.Arguments.First(a => a.ValueType == typeof(DateTime)).GetValues<DateTime>();
                    if (timeValues.Any())
                    {
                        if(!GivenTimeSeriesSpansModelTimeRange(model, boundaryCondition, timeValues))
                        {
                            issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                                           string.Format(
                                                               "Time series does not span model run interval for {0} at point {1}.",
                                                               boundaryConditionName, supportPointName),
                                                           boundaryCondition));
                        }

                        if (boundaryCondition.StrictlyPositive)
                        {
                            foreach (IVariable component in function.Components)
                            {
                                if ((double) component.MinValue < 0.0)
                                {
                                    issues.Add(new ValidationIssue(boundaryConditionName,
                                                                   ValidationSeverity.Error,
                                                                   string.Format(
                                                                       Resources
                                                                           .WaterFlowFMBoundaryConditionValidator_ValidateBoundaryConditionPointIndex_Time_series_contains_forbidden_negative_values_for__0__at_point__1_,
                                                                       boundaryConditionName, supportPointName),
                                                                   boundaryCondition));
                                }
                            }
                        }
                    }
                    else
                    {
                        issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Error,
                                                       string.Format(
                                                           Resources
                                                               .WaterFlowFMBoundaryConditionValidator_ValidateFlowBoundaryConditions_No_data_defined_for_boundary_condition___0___at_boundary___1__,
                                                           boundaryConditionName, supportPointName),
                                                       boundaryCondition));
                    }
                }

                if (boundaryCondition.DataType == BoundaryConditionDataType.AstroComponents ||
                    boundaryCondition.DataType == BoundaryConditionDataType.AstroCorrection)
                {
                    foreach (string astroComponent in function.Arguments[0].Values.Cast<string>())
                    {
                        if (HarmonicComponent.DefaultAstroComponentsRadPerHour.Keys.Contains(astroComponent))
                        {
                            continue;
                        }

                        issues.Add(new ValidationIssue(boundaryConditionName, ValidationSeverity.Warning,
                                                       string.Format(
                                                           "Unknown astronomical component {0} detected for {1} at point {2}",
                                                           astroComponent, boundaryConditionName, supportPointName)));
                    }
                }

                VerticalProfileDefinition depthProfile = boundaryCondition.GetDepthLayerDefinitionAtPoint(pointIndex);

                if (depthProfile == null)
                {
                    continue;
                }

                issues.AddRange(VerticalProfileValidator.ValidateVerticalProfile(boundaryConditionName,
                                                                                 depthProfile, boundaryCondition,
                                                                                 supportPointName));
            }
        }

        private static bool GivenTimeSeriesSpansModelTimeRange(WaterFlowFMModel model, FlowBoundaryCondition boundaryCondition, IMultiDimensionalArray<DateTime> timeValues)
        {
            TimeSpan modelTimeZone = GetModelTimeZone(model); 
            
            DateTime modelStartTime = model.StartTime.Subtract(modelTimeZone); 
            DateTime modelStopTime = model.StopTime.Subtract(modelTimeZone); 
            
            DateTime lowerBound = timeValues[0].Subtract(boundaryCondition.TimeZone); 
            DateTime upperBound = timeValues[timeValues.Count-1].Subtract(boundaryCondition.TimeZone); 
            
            return lowerBound <= modelStartTime && upperBound >= modelStopTime;
        }

        private static TimeSpan GetModelTimeZone(WaterFlowFMModel model)
        {
            var tZone = (double)model.ModelDefinition.GetModelProperty(KnownProperties.TZone).Value;
            TimeSpan timeZone = TimeSpan.FromHours(tZone);
            return timeZone;
        }

        // Remove whenever the ec-module supports custom boundary point names
        private static IEnumerable<ValidationIssue> ValidateSupportPointNames(BoundaryConditionSet boundaryConditionSet)
        {
            for (var i = 0; i < boundaryConditionSet.SupportPointNames.Count; i++)
            {
                IEnumerable<IBoundaryCondition> boundaryConditions = i == 0
                                                                         ? boundaryConditionSet.BoundaryConditions
                                                                         : boundaryConditionSet
                                                                           .BoundaryConditions
                                                                           .Where(bc => !bc.IsHorizontallyUniform);

                if (boundaryConditions.Any(bc => bc.GetDataAtPoint(i) != null))
                {
                    string expectedName = BoundaryConditionSet.DefaultLocationName(boundaryConditionSet.Feature, i);
                    if (boundaryConditionSet.SupportPointNames[i] != expectedName)
                    {
                        yield return
                            new ValidationIssue(boundaryConditionSet, ValidationSeverity.Error,
                                                string.Format(
                                                    Resources
                                                        .WaterFlowFMBoundaryConditionValidator_ValidateSupportPointNames_Custom_support_point_name__0__is_not_yet_supported_by_the_dflow_fm_kernel__please_change_it_to__1_,
                                                    boundaryConditionSet.SupportPointNames[i], expectedName),
                                                boundaryConditionSet);
                    }
                }
            }
        }
    }
}