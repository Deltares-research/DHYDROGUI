using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveBoundaryConditionValidator
    {
        public static ValidationReport Validate(WaveModel model)
        {
            var subReports = model.BoundaryConditions
                .Select(bc => new ValidationReport(bc.Name, ValidateBoundaryCondition(bc)))
                .ToList();

            return new ValidationReport("Waves Model Boundary Conditions", subReports);
        }

        private static IEnumerable<ValidationIssue> ValidateBoundaryCondition(WaveBoundaryCondition boundaryCondition)
        {
            if (!boundaryCondition.DataPointIndices.Any())
            {
                yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error,
                    Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_has_no_data_defined, boundaryCondition);
            }

            if (boundaryCondition.IsHorizontallyUniform && boundaryCondition.Feature.Geometry.Coordinates.Length > 2)
            {
                yield return
                    new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Info,
                        Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_internal_geometry_points,
                        boundaryCondition);
            }
            else if (!boundaryCondition.IsHorizontallyUniform && Enumerable.Range(1, boundaryCondition.Feature.Geometry.Coordinates.Length - 2).Except(boundaryCondition.DataPointIndices).Any())

            {
                yield return
                    new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Info,
                        Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_unactivated_support_points,
                        boundaryCondition);
            }

            if (boundaryCondition.DataType == BoundaryConditionDataType.ParametrizedSpectrumConstant)
            {
                foreach (var spectrumParameters in boundaryCondition.SpectrumParameters)
                {
                    var spectrumValues = spectrumParameters.Value;

                    var precedingText = string.Empty;
                    if (boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)
                    {
                        var pointIndex = spectrumParameters.Key + 1;
                        precedingText = $"Point {pointIndex}: ";
                    }

                    if (spectrumValues.Height <= 0)
                    {
                        yield return new ValidationIssue(boundaryCondition.VariableDescription,
                            ValidationSeverity.Error,
                            precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Height__must_be_greater_than_0_,
                            boundaryCondition);
                    }

                    if (spectrumValues.Period <= 0)
                    {
                        yield return new ValidationIssue(boundaryCondition.VariableDescription,
                            ValidationSeverity.Error,
                            precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Period__must_be_greater_than_0_,
                            boundaryCondition);
                    }

                    if (spectrumValues.Spreading <= 0)
                    {
                        yield return new ValidationIssue(boundaryCondition.VariableDescription,
                            ValidationSeverity.Error,
                            precedingText + Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Spreading__must_be_greater_than_0_,
                            boundaryCondition);
                    }
                }
            }

            if (boundaryCondition.DataType == BoundaryConditionDataType.ParametrizedSpectrumTimeseries && boundaryCondition.PointData.Count != 0)
            {
                for(var i = 0; i < boundaryCondition.PointData.Count; i++)
                {
                    var function = boundaryCondition.PointData[i];
                    var pointIndex = boundaryCondition.DataPointIndices[i] + 1;

                    var heightComponent = function.Components.FirstOrDefault(c => c.Name == WaveBoundaryCondition.HeightVariableName);
                    var heightComponentValues = heightComponent?.Values as IMultiDimensionalArray<double>;
                    if (heightComponentValues != null && heightComponentValues.Any(v => v <= 0.0))
                    {
                        yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error, 
                            string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0__Values_in_column__Hs__in_the_time_series_table_must_be_greater_than_0_, pointIndex),
                            boundaryCondition);
                    }

                    var periodComponent = function.Components.FirstOrDefault(c => c.Name == WaveBoundaryCondition.PeriodVariableName);
                    var periodComponentValues = periodComponent?.Values as IMultiDimensionalArray<double>;
                    if (periodComponentValues != null && periodComponentValues.Any(v => v <= 0.0))
                    {
                        yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error,
                            string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0__Values_in_column__Tp__in_the_time_series_table_must_be_greater_than_0_, pointIndex),
                            boundaryCondition);
                    }

                    var spreadingComponent = function.Components.FirstOrDefault(c => c.Name == WaveBoundaryCondition.SpreadingVariableName);
                    var spreadingComponentValues = spreadingComponent?.Values as IMultiDimensionalArray<double>;
                    if (spreadingComponentValues != null && spreadingComponentValues.Any(v => v <= 0.0))
                    {
                        yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error,
                            string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Point__0__Values_in_column__Spreading__in_the_time_series_table_must_be_greater_than_0_, pointIndex),
                            boundaryCondition);
                    }
                }
            }

            if (boundaryCondition.DataType == BoundaryConditionDataType.ParametrizedSpectrumTimeseries && boundaryCondition.PointData.Count > 1 &&
                boundaryCondition.SpatialDefinitionType != WaveBoundaryConditionSpatialDefinitionType.Uniform)
            {
                var times = boundaryCondition.PointData[0].Arguments[0].GetValues<DateTime>();
                foreach (var f in boundaryCondition.PointData.Skip(1))
                {
                    var compareTimes = f.Arguments[0].GetValues<DateTime>().ToList();
                    if (!times.SequenceEqual(compareTimes))
                    {
                        yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error,
                            string.Format(Resources.WaveBoundaryConditionValidator_ValidateBoundaryCondition_Time_points_are_not_synchronized_on_boundary___0_, boundaryCondition.Name), boundaryCondition);
                    }
                }
            }
        }
    }
}