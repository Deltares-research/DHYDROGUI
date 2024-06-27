using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// Validator for the boundary container
    /// </summary>
    public static class WaveBoundariesValidator
    {
        private static string VariableDescription = "Wave Energy Density";

        /// <summary>
        /// Validates all boundaries of the boundary container
        /// </summary>
        /// <param name="boundaries">
        /// The boundaries of the boundary
        /// container in the model definition.
        /// </param>
        /// <param name="modelStartTime"> Model start time. </param>
        /// <returns> A <see cref="ValidationReport"/></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaries"/>
        /// is <c>null</c>.
        /// </exception>
        public static ValidationReport Validate(IEventedList<IWaveBoundary> boundaries, DateTime modelStartTime)
        {
            Ensure.NotNull(boundaries, nameof(boundaries));
            IList<ValidationReport> subReports = new List<ValidationReport>();

            foreach (IWaveBoundary boundary in boundaries)
            {
                IEnumerable<ValidationIssue> validationIssues = CollectAllValidationIssues(boundary, modelStartTime);
                var report = new ValidationReport(boundary.Name, validationIssues);
                subReports.Add(report);
            }

            return new ValidationReport(Resources.WaveBoundariesValidator_Validate_Waves_Model_Boundaries, subReports);
        }

        private static IEnumerable<ValidationIssue> CollectAllValidationIssues(IWaveBoundary boundary, DateTime modelStartTime)
        {
            var visitor = new ValidatorVisitor(boundary);
            boundary.ConditionDefinition.AcceptVisitor(visitor);
            ValidateAllTimeSeriesOfBoundary(visitor, boundary, modelStartTime);
            return visitor.ValidationIssues;
        }

        private static void ValidateAllTimeSeriesOfBoundary(ValidatorVisitor visitor, INameable boundary, DateTime modelStartTime)
        {
            List<IVariable<DateTime>> dateTimesPerFunction = visitor.DateTimesPerFunction;
            List<ValidationIssue> validationIssues = visitor.ValidationIssues;

            // constant parameters
            if (dateTimesPerFunction.Count == 0)
            {
                return;
            }

            IList<DateTime> values = dateTimesPerFunction.SelectMany(v => v.Values).ToList();

            // empty time serie
            if (!values.Any())
            {
                return;
            }

            ValidateIfModelStartTimeIsNotAfterAllTimeArguments(boundary, modelStartTime, values, validationIssues);

            // nothing to compare
            if (dateTimesPerFunction.Count == 1)
            {
                return;
            }

            ValidateFunctionsIfTheyContainTheSameTimeArguments(boundary, dateTimesPerFunction, validationIssues);
        }

        private static void ValidateIfModelStartTimeIsNotAfterAllTimeArguments(INameable boundary, DateTime modelStartTime, IList<DateTime> values, List<ValidationIssue> validationIssues)
        {
            bool allTimePointsPrecedeModelStartTime = values.All(b => b < modelStartTime);

            if (allTimePointsPrecedeModelStartTime)
            {
                validationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                         string.Format(
                                                             Resources.WaveBoundariesValidator_Validate_ModelStartTime_Model_start_time_does_not_precede_any_of_Boundary_Condition_time_points_of__0__,
                                                             boundary.Name, boundary.Name), boundary));
            }
        }

        private static void ValidateFunctionsIfTheyContainTheSameTimeArguments(INameable boundary, List<IVariable<DateTime>> dateTimesPerFunction, List<ValidationIssue> validationIssues)
        {
            List<DateTime> times = dateTimesPerFunction[0].Values.ToList();
            foreach (IVariable<DateTime> f in dateTimesPerFunction.Skip(1))
            {
                List<DateTime> compareTimes = f.Values.ToList();
                if (!times.SequenceEqual(compareTimes))
                {
                    validationIssues.Add(new ValidationIssue(VariableDescription, ValidationSeverity.Error,
                                                             string.Format(
                                                                 Resources
                                                                     .WaveBoundariesValidator_Validate_Time_points_are_not_synchronized_on_boundary__0__,
                                                                 boundary.Name), boundary));
                }
            }
        }

        private class ValidatorVisitor : IBoundaryConditionVisitor, ISpatiallyDefinedDataComponentVisitor, IForcingTypeDefinedParametersVisitor, IShapeVisitor, ISpreadingVisitor
        {
            private bool isUniform = true;

            private int supportPointCounter;

            private string precedingSupportPointNumberText = string.Empty;

            /// <summary>
            /// The constructor should set the boundary, which is visited.
            /// </summary>
            /// <param name="boundary"> The visited <see cref="IWaveBoundary"/>.</param>
            public ValidatorVisitor(IWaveBoundary boundary)
            {
                Boundary = boundary;
                AllDefinedSupportPoints = boundary.GeometricDefinition.SupportPoints;
            }

            public List<ValidationIssue> ValidationIssues { get; } = new List<ValidationIssue>();

            public List<IVariable<DateTime>> DateTimesPerFunction { get; } = new List<IVariable<DateTime>>();

            /// <summary>
            /// Visit method for calling the next AcceptVisitor methods of the shape and data component.
            /// </summary>
            /// <param name="waveBoundaryConditionDefinition"> The visited <see cref="IWaveBoundaryConditionDefinition"/></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="waveBoundaryConditionDefinition"/> is <c>null</c>.
            /// </exception>
            public void Visit(IWaveBoundaryConditionDefinition waveBoundaryConditionDefinition)
            {
                Ensure.NotNull(waveBoundaryConditionDefinition, nameof(waveBoundaryConditionDefinition));
                waveBoundaryConditionDefinition.Shape.AcceptVisitor(this);
                waveBoundaryConditionDefinition.DataComponent.AcceptVisitor(this);
            }

            /// <summary>
            /// Visit method for validating <see cref="ConstantParameters{TSpreading}"/>. Calls the next
            /// AcceptVisitor method for the spreading.
            /// </summary>
            /// <param name="constantParameters"> The visited <see cref="ConstantParameters{TSpreading}"/></param>
            /// <exception cref="ArgumentNullException">
            /// <typeparam name="T"> The type of spreading.</typeparam>
            /// Thrown when <paramref name="constantParameters"/> is <c>null</c>.
            /// </exception>
            public void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new()
            {
                Ensure.NotNull(constantParameters, nameof(constantParameters));

                if (!isUniform)
                {
                    supportPointCounter++;
                    precedingSupportPointNumberText = $"Point {supportPointCounter}: ";
                }

                if (IsOutsideOfRange(constantParameters.Height, 0, 25))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                             ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Parameter_Height_must_be_greater_than_0_and_smaller_or_equal_to_25_,
                                                             Boundary));
                }

                if (IsOutsideOfRange(constantParameters.Period, 0.1, 20.0))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                             ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Parameter_Period_must_be_a_value_within_the_range_,
                                                             Boundary));
                }

                if (IsOutsideOfRange(constantParameters.Direction, -360.0, 360.0))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                             ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Parameter_Direction_must_be_a_value_within_the_range_360_360_,
                                                             Boundary));
                }

                constantParameters.Spreading.AcceptVisitor(this);
            }

            /// <summary>
            /// Visit method for validating <see cref="TimeDependentParameters{TSpreading}"/>
            /// including spreading values.
            /// </summary>
            /// <typeparam name="T"> The type of spreading. </typeparam>
            /// <param name="timeDependentParameters"> The visited <see cref="TimeDependentParameters{TSpreading}"/></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="timeDependentParameters"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new()
            {
                Ensure.NotNull(timeDependentParameters, nameof(timeDependentParameters));

                if (!isUniform)
                {
                    supportPointCounter++;
                    precedingSupportPointNumberText = $"Point {supportPointCounter}: ";
                }

                IVariable<DateTime> timeArgument = timeDependentParameters.WaveEnergyFunction.TimeArgument;

                if (timeArgument?.Values == null || timeArgument.Values.Count == 0)
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Boundary_does_not_contain_any_valid_boundary_data,
                                                             Boundary));
                }

                IVariable<double> heightComponent = timeDependentParameters.WaveEnergyFunction.HeightComponent;

                if (heightComponent?.Values != null && heightComponent.Values.Any(v => IsOutsideOfRange(v, 0.0, 25.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Values_in_column_Hs_in_the_time_series_table_must_be_within_expected_range,
                                                             Boundary));
                }

                IVariable<double> periodComponent = timeDependentParameters.WaveEnergyFunction.PeriodComponent;

                if (periodComponent?.Values != null && periodComponent.Values.Any(v => IsOutsideOfRange(v, 0.1, 20.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Values_in_column_Tp_in_the_time_series_table_must_be_within_expected_range,
                                                             Boundary));
                }

                IVariable<double> directionComponent = timeDependentParameters.WaveEnergyFunction.DirectionComponent;

                if (directionComponent?.Values != null && directionComponent.Values.Any(v => IsOutsideOfRange(v, -360.0, 360.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Values_in_column_Direction_in_the_time_series_table_must_be_within_expected_range,
                                                             Boundary));
                }

                IVariable<double> spreadingComponent = timeDependentParameters.WaveEnergyFunction.SpreadingComponent;

                if (typeof(T) == typeof(PowerDefinedSpreading) && spreadingComponent?.Values != null &&
                    spreadingComponent.Values.Any(v => IsOutsideOfRange(v, 1.0, 800.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Values_in_column_Spreading_in_the_time_series_table_must_be_a_value_within_the_range_1_800,
                                                             Boundary));
                }

                if (typeof(T) == typeof(DegreesDefinedSpreading) && spreadingComponent?.Values != null &&
                    spreadingComponent.Values.Any(v => IsOutsideOfRange(v, 2.0, 180.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription, ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Values_in_column_Spreading_in_the_time_series_table_must_be_a_value_within_the_range_2_180,
                                                             Boundary));
                }

                DateTimesPerFunction.Add(timeArgument);
            }

            /// <summary>
            /// Visit method for validating <see cref="FileBasedParameters"/>.
            /// </summary>
            /// <param name="fileBasedParameters"> The visited <see cref="FileBasedParameters"/>. </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="fileBasedParameters"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(FileBasedParameters fileBasedParameters)
            {
                Ensure.NotNull(fileBasedParameters, nameof(fileBasedParameters));

                if (string.IsNullOrWhiteSpace(fileBasedParameters.FilePath))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription, ValidationSeverity.Error,
                                                             Resources.WaveBoundariesValidator_Validate_FilePath_cannot_be_empty,
                                                             Boundary));
                }
            }

            /// <summary>
            /// Visit method for doing nothing, since there are not validation rules for this shape.
            /// Must be defined for visitor pattern.
            /// </summary>
            /// <param name="gaussShape"> The visited <see cref="GaussShape"/></param>
            public void Visit(GaussShape gaussShape)
            {
                // No validation rules.
            }

            /// <summary>
            /// Visit method for validating the jonswap shape"/>.
            /// </summary>
            /// <param name="jonswapShape"> The visited <see cref="JonswapShape"/></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="jonswapShape"/> is <c>null</c>.
            /// </exception>
            public void Visit(JonswapShape jonswapShape)
            {
                Ensure.NotNull(jonswapShape, nameof(jonswapShape));
                if (IsOutsideOfRange(jonswapShape.PeakEnhancementFactor, 1.0, 10.0))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                             Resources
                                                                 .WaveBoundariesValidator_Validate_Peak_Enhancement_Factor_must_be_a_value_within_the_range_1_10_,
                                                             Boundary));
                }
            }

            /// <summary>
            /// Visit method for doing nothing, since there are not validation rules for this shape.
            /// Must be defined for visitor pattern.
            /// </summary>
            /// <param name="piersonMoskowitzShape"> The visited <see cref="PiersonMoskowitzShape"/></param>
            public void Visit(PiersonMoskowitzShape piersonMoskowitzShape)
            {
                // No validation rules.
            }

            /// <summary>
            /// Visit method for calling the next AcceptVisitor method of the Data stored in <see cref="UniformDataComponent{T}"/>.
            /// </summary>
            /// <typeparam name="T"> The forcing type.</typeparam>
            /// <param name="uniformDataComponent"> The visited <see cref="UniformDataComponent{T}"/></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="uniformDataComponent"/> is <c>null</c>.
            /// </exception>
            public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IForcingTypeDefinedParameters
            {
                Ensure.NotNull(uniformDataComponent, nameof(uniformDataComponent));
                isUniform = true;
                uniformDataComponent.Data.AcceptVisitor(this);
            }

            /// <summary>
            /// Visit method for validating if there are inactive support points. Calls the next AcceptVisitors methods of the stored
            /// data
            /// for all support points in <see cref="SpatiallyVaryingDataComponent{T}"/>.
            /// </summary>
            /// <typeparam name="T"> The forcing type.</typeparam>
            /// <param name="spatiallyVaryingDataComponent"> The visited <see cref="SpatiallyVaryingDataComponent{T}"/></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="spatiallyVaryingDataComponent"/> is <c>null</c>.
            /// </exception>
            public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IForcingTypeDefinedParameters
            {
                Ensure.NotNull(spatiallyVaryingDataComponent, nameof(spatiallyVaryingDataComponent));

                isUniform = false;

                IEnumerable<SupportPoint> activeSupportPoints = spatiallyVaryingDataComponent.Data.Keys;

                if (activeSupportPoints.Count() < AllDefinedSupportPoints.Count)
                {
                    ValidationIssues.Add(
                        new ValidationIssue(VariableDescription, ValidationSeverity.Info,
                                            Resources
                                                .WaveBoundariesValidator_Validate_Boundary_condition_contains_unactivated_support_points,
                                            Boundary));
                }

                IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedDictionary = spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);
                foreach (KeyValuePair<SupportPoint, T> supportPointKeyValuePair in sortedDictionary)
                {
                    supportPointKeyValuePair.Value.AcceptVisitor(this);
                }
            }

            /// <summary>
            /// Visit method for validating <see cref="DegreesDefinedSpreading"/>.
            /// </summary>
            /// <param name="degreesDefinedSpreading"> The visited <see cref="DegreesDefinedSpreading"/></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="degreesDefinedSpreading"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(DegreesDefinedSpreading degreesDefinedSpreading)
            {
                Ensure.NotNull(degreesDefinedSpreading, nameof(degreesDefinedSpreading));

                if (IsOutsideOfRange(degreesDefinedSpreading.DegreesSpreading, 2.0, 180.0))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                             ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Parameter_Spreading_must_be_a_value_within_the_range_2_180,
                                                             Boundary));
                }
            }

            /// <summary>
            /// Visit method for validating <see cref="PowerDefinedSpreading"/>.
            /// </summary>
            /// <param name="powerDefinedSpreading"> The visited <see cref="PowerDefinedSpreading"/></param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="powerDefinedSpreading"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(PowerDefinedSpreading powerDefinedSpreading)
            {
                Ensure.NotNull(powerDefinedSpreading, nameof(powerDefinedSpreading));
                if (IsOutsideOfRange(powerDefinedSpreading.SpreadingPower, 1.0, 800.0))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                             ValidationSeverity.Error,
                                                             precedingSupportPointNumberText + Resources
                                                                 .WaveBoundariesValidator_Validate_Parameter_Spreading__must_be_a_value_within_the_range_1_800,
                                                             Boundary));
                }
            }

            private IWaveBoundary Boundary { get; }

            private IEventedList<SupportPoint> AllDefinedSupportPoints { get; set; }

            private static bool IsOutsideOfRange(double value, double lowerLimit, double upperLimit) => value - lowerLimit <= -double.Epsilon || value - upperLimit >= double.Epsilon;
        }
    }
}