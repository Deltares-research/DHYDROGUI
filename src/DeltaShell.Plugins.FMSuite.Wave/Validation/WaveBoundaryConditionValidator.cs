using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    public static class WaveBoundaryConditionValidator
    {
        /// <summary>
        /// Validates the specified wave boundary conditions. Wave boundary conditions that are equal
        /// to null will not be validated.
        /// </summary>
        /// <param name="waveBoundaryConditions"> The wave boundary conditions to validate. </param>
        /// <returns> A validation report about the wave boundary conditions. </returns>
        /// <remarks> <paramref name="waveBoundaryConditions" /> should not be null. </remarks>
        public static ValidationReport Validate(IEnumerable<WaveBoundaryCondition> waveBoundaryConditions)
        {
            IEnumerable<ValidationReport> subReports =
                waveBoundaryConditions.Where(bc => bc != null)
                                      .Select(bc => new ValidationReport(bc.Name, ValidateBoundaryCondition(bc)));

            return new ValidationReport("Waves Model Boundary Conditions", subReports);
        }

        private static IEnumerable<ValidationIssue> ValidateBoundaryCondition(WaveBoundaryCondition boundaryCondition)
        {
            return ValidateDataPoints(boundaryCondition)
                   .Concat(ValidateGeometry(boundaryCondition))
                   .Concat(ValidateSpectralData(boundaryCondition))
                   .Concat(ValidateSpectrumParameters(boundaryCondition))
                   .Concat(ValidateTimeSeriesValues(boundaryCondition))
                   .Concat(ValidateTimePoints(boundaryCondition));
        }

        private static IEnumerable<ValidationIssue> ValidateDataPoints(WaveBoundaryCondition boundaryCondition)
        {
            if (!boundaryCondition.DataPointIndices.Any())
            {
                yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error,
                                                 Resources
                                                     .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_does_not_contain_a_boundary_condition,
                                                 boundaryCondition);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateGeometry(WaveBoundaryCondition boundaryCondition)
        {
            if (boundaryCondition.IsHorizontallyUniform && boundaryCondition.Feature.Geometry.Coordinates.Length > 2)
            {
                yield return
                    new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Info,
                                        Resources
                                            .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_internal_geometry_points,
                                        boundaryCondition);
            }
            else if (!boundaryCondition.IsHorizontallyUniform &&
                     Enumerable.Range(1, boundaryCondition.Feature.Geometry.Coordinates.Length - 2)
                               .Except(boundaryCondition.DataPointIndices).Any())
            {
                yield return
                    new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Info,
                                        Resources
                                            .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_unactivated_support_points,
                                        boundaryCondition);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateSpectralData(WaveBoundaryCondition boundaryCondition)
        {
            if (boundaryCondition.ShapeType == WaveSpectrumShapeType.Jonswap
                && boundaryCondition.PeakEnhancementFactor.IsOutsideOfRange(1.0, 10.0))
            {
                yield return new ValidationIssue(null, ValidationSeverity.Error,
                                                 Resources
                                                     .WaveBoundaryConditionValidator_ValidateSpectralData_Peak_Enhancement_Factor_must_be_a_value_within_the_range_1___10_,
                                                 boundaryCondition);
            }
        }

        private static IEnumerable<ValidationIssue> ValidateSpectrumParameters(WaveBoundaryCondition boundaryCondition)
        {
            if (boundaryCondition.DataType != BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                yield break;
            }

            foreach (KeyValuePair<int, WaveBoundaryParameters> spectrumParameters in boundaryCondition
                .SpectrumParameters)
            {
                WaveBoundaryParameters spectrumValues = spectrumParameters.Value;

                string precedingText = string.Empty;
                if (boundaryCondition.SpatialDefinitionType ==
                    WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)
                {
                    int pointIndex = spectrumParameters.Key + 1;
                    precedingText = $"Point {pointIndex}: ";
                }

                if (spectrumValues.Height <= 0.0 || spectrumValues.Height - 25.0 >= double.Epsilon)
                {
                    yield return new ValidationIssue(boundaryCondition.VariableDescription,
                                                     ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Height__must_be_greater_than_0_and_smaller_or_equal_to_25_,
                                                     boundaryCondition);
                }

                if (spectrumValues.Period.IsOutsideOfRange(0.1, 20.0))
                {
                    yield return new ValidationIssue(boundaryCondition.VariableDescription,
                                                     ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Period__must_be_a_value_within_the_range_,
                                                     boundaryCondition);
                }

                if (spectrumValues.Direction.IsOutsideOfRange(-360.0, 360.0))
                {
                    yield return new ValidationIssue(boundaryCondition.VariableDescription,
                                                     ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateSpectrumParameters_Parameter__Direction__must_be_a_value_within_the_range__360___360_,
                                                     boundaryCondition);
                }

                if (boundaryCondition.DirectionalSpreadingType == WaveDirectionalSpreadingType.Power &&
                    spectrumValues.Spreading.IsOutsideOfRange(1.0, 800.0))
                {
                    yield return new ValidationIssue(boundaryCondition.VariableDescription,
                                                     ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Spreading__must_be_a_value_within_the_range_1_800,
                                                     boundaryCondition);
                }

                if (boundaryCondition.DirectionalSpreadingType == WaveDirectionalSpreadingType.Degrees &&
                    spectrumValues.Spreading.IsOutsideOfRange(2.0, 180.0))
                {
                    yield return new ValidationIssue(boundaryCondition.VariableDescription,
                                                     ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Spreading__must_be_a_value_within_the_range_2_180,
                                                     boundaryCondition);
                }
            }
        }

        private static IEnumerable<ValidationIssue> ValidateTimeSeriesValues(WaveBoundaryCondition boundaryCondition)
        {
            if (boundaryCondition.DataType != BoundaryConditionDataType.ParameterizedSpectrumTimeseries)
            {
                yield break;
            }

            IEnumerable<PointDataFunctionMapping> functionMapping = CreateSortedPointDataFunctionMapping(boundaryCondition);
            var precedingText = string.Empty;
            foreach (PointDataFunctionMapping mapping in functionMapping)
            {
                IFunction function = mapping.PointDataFunction;
                if (boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying)
                {
                    precedingText = $"Point {mapping.PointDataIndex + 1}: ";
                }

                IVariable timeArgument = function.Arguments.FirstOrDefault(a => a.Name == WaveBoundaryCondition.TimeVariableName);
                if (timeArgument?.Values.Count == 0)
                {
                    yield return new ValidationIssue(null, ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_does_not_contain_a_boundary_condition,
                                                     boundaryCondition);
                    continue;
                }

                IMultiDimensionalArray<double> heightComponentValues =
                    GetComponentValues(function, WaveBoundaryCondition.HeightVariableName);
                if (heightComponentValues != null &&
                    heightComponentValues.Any(v => v <= 0.0 || v - 25.0 >= double.Epsilon))
                {
                    yield return new ValidationIssue(null, ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Hs__in_the_time_series_table_must_be_within_expected_range,
                                                     boundaryCondition);
                }

                IMultiDimensionalArray<double> periodComponentValues =
                    GetComponentValues(function, WaveBoundaryCondition.PeriodVariableName);
                if (periodComponentValues != null && periodComponentValues.Any(v => v.IsOutsideOfRange(0.1, 20.0)))
                {
                    yield return new ValidationIssue(null, ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Tp__in_the_time_series_table_must_be_within_expected_range,
                                                     boundaryCondition);
                }

                IMultiDimensionalArray<double> directionComponentValues =
                    GetComponentValues(function, WaveBoundaryCondition.DirectionVariableName);
                if (directionComponentValues != null &&
                    directionComponentValues.Any(v => v.IsOutsideOfRange(-360.0, 360.0)))
                {
                    yield return new ValidationIssue(null, ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Direction__in_the_time_series_table_must_be_within_expected_range,
                                                     boundaryCondition);
                }

                IMultiDimensionalArray<double> spreadingComponentValues =
                    GetComponentValues(function, WaveBoundaryCondition.SpreadingVariableName);
                if (boundaryCondition.DirectionalSpreadingType == WaveDirectionalSpreadingType.Power &&
                    spreadingComponentValues != null &&
                    spreadingComponentValues.Any(v => v.IsOutsideOfRange(1.0, 800.0)))
                {
                    yield return new ValidationIssue(null, ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Spreading__in_the_time_series_table_must_be_a_value_within_the_range_1_800,
                                                     boundaryCondition);
                }

                if (boundaryCondition.DirectionalSpreadingType == WaveDirectionalSpreadingType.Degrees &&
                    spreadingComponentValues != null &&
                    spreadingComponentValues.Any(v => v.IsOutsideOfRange(2.0, 180.0)))
                {
                    yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error,
                                                     precedingText + Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Spreading__in_the_time_series_table_must_be_a_value_within_the_range_2_180,
                                                     boundaryCondition);
                }
            }
        }

        private static IMultiDimensionalArray<double> GetComponentValues(IFunction function, string componentName)
        {
            IVariable component = function.Components.FirstOrDefault(c => c.Name == componentName);
            return component?.Values as IMultiDimensionalArray<double>;
        }

        private static IEnumerable<ValidationIssue> ValidateTimePoints(WaveBoundaryCondition boundaryCondition)
        {
            if (boundaryCondition.DataType != BoundaryConditionDataType.ParameterizedSpectrumTimeseries ||
                boundaryCondition.PointData.Count <= 1 ||
                boundaryCondition.SpatialDefinitionType == WaveBoundaryConditionSpatialDefinitionType.Uniform)
            {
                yield break;
            }

            IMultiDimensionalArray<DateTime> times = boundaryCondition.PointData[0].Arguments[0].GetValues<DateTime>();
            foreach (IFunction f in boundaryCondition.PointData.Skip(1))
            {
                List<DateTime> compareTimes = f.Arguments[0].GetValues<DateTime>().ToList();
                if (!times.SequenceEqual(compareTimes))
                {
                    yield return new ValidationIssue(boundaryCondition.VariableDescription, ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Time_points_are_not_synchronized_on_boundary___0_,
                                                         boundaryCondition.Name), boundaryCondition);
                }
            }
        }

        private static bool IsOutsideOfRange(this double value, double lowerLimit, double upperLimit)
        {
            return value - lowerLimit <= -double.Epsilon || value - upperLimit >= double.Epsilon;
        }

        /// <summary>
        /// Create a mapping between the point data index and the point data function.
        /// </summary>
        /// <param name="boundaryCondition">The <see cref="WaveBoundaryCondition"/> to create the mapping for.</param>
        /// <returns>A mapping between the index and point data functions in an ascending order by the index.</returns>
        private static IEnumerable<PointDataFunctionMapping> CreateSortedPointDataFunctionMapping(WaveBoundaryCondition boundaryCondition)
        {
            var mapping = new PointDataFunctionMapping[boundaryCondition.PointData.Count];
            for (var i = 0; i < mapping.Length; i++)
            {
                mapping[i] = new PointDataFunctionMapping(boundaryCondition.DataPointIndices[i], boundaryCondition.PointData[i]);
            }

            return mapping.OrderBy(map => map.PointDataIndex);
        }

        /// <summary>
        /// Class to hold the mapping between PointData and its corresponding point index.
        /// </summary>
        private class PointDataFunctionMapping
        {
            /// <summary>
            /// Gets the index of the point data.
            /// </summary>
            public int PointDataIndex { get; }

            /// <summary>
            /// Gets the point data function.
            /// </summary>
            public IFunction PointDataFunction { get; }

            /// <summary>
            /// Creates a new instance of <see cref="PointDataFunctionMapping"/>.
            /// </summary>
            /// <param name="pointDataIndex">The index of the <paramref name="pointDataFunction"/>.</param>
            /// <param name="pointDataFunction">The <see cref="IFunction"/> associated with a point data.</param>
            public PointDataFunctionMapping(int pointDataIndex, IFunction pointDataFunction)
            {
                PointDataIndex = pointDataIndex;
                PointDataFunction = pointDataFunction;
            }
        }
    }
}