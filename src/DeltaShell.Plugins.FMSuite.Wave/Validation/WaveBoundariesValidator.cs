using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.Properties;


namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// Validator for the boundary container
    /// </summary>
    public static class WaveBoundariesValidator
    {
        /// <summary>
        /// Validates all boundaries of the boundary container
        /// </summary>
        /// <param name="boundaries"> The boundaries of the boundary
        /// container in the model definition.</param>
        /// <returns> A <see cref="ValidationReport"/></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaries"/>
        /// is <c>null</c>.
        /// </exception>
        public static ValidationReport Validate(IEventedList<IWaveBoundary> boundaries)
        {
            Ensure.NotNull(boundaries, nameof(boundaries));
            IList<ValidationReport> subReports = new List<ValidationReport>();
               
            foreach (IWaveBoundary boundary in boundaries)
            {
                List<ValidationIssue> validationIssues = CollectAllValidationIssues(boundary);
                var report = new ValidationReport(boundary.Name, validationIssues);
                subReports.Add(report);
            }

            return new ValidationReport("Waves Model Boundaries", subReports);
        }

        private static List<ValidationIssue> CollectAllValidationIssues(IWaveBoundary boundary)
        {
            var visitor = new ValidatorVisitor(boundary);
            boundary.ConditionDefinition.AcceptVisitor(visitor);
            ValidateAllTimeSeriesOfBoundary(visitor, boundary.Name);
            return visitor.ValidationIssues;
        }

        private static void ValidateAllTimeSeriesOfBoundary(ValidatorVisitor visitor, string boundaryName)
        {
            if (visitor.DateTimesPerFunction.Count <= 1)
            {
                return;
            }

            List<DateTime> times = visitor.DateTimesPerFunction[0].Values.ToList();
            foreach (IVariable<DateTime> f in visitor.DateTimesPerFunction.Skip(1))
            {
                List<DateTime> compareTimes = f.Values.ToList();
                if (!times.SequenceEqual(compareTimes))
                {
                    visitor.ValidationIssues.Add(new ValidationIssue(VariableDescription, ValidationSeverity.Error,
                                                     string.Format(
                                                         Resources
                                                             .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Time_points_are_not_synchronized_on_boundary___0_,
                                                         boundaryName), boundaryName));
                }
            }
        }

        private static string VariableDescription = "Wave Energy Density";
        
        private class ValidatorVisitor :  IBoundaryConditionVisitor, ISpatiallyDefinedDataComponentVisitor, IForcingTypeDefinedParametersVisitor, IShapeVisitor, ISpreadingVisitor
        {
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

            private IWaveBoundary Boundary { get; }

            private IEventedList<SupportPoint> AllDefinedSupportPoints { get; set; }
            
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
            /// Visit method for doing nothing, since there are not validation rules for this shape.
            /// Must be defined for visitor pattern.
            /// </summary>
            /// <param name="gaussShape"> The visited <see cref="GaussShape"/></param>
            public void Visit(GaussShape gaussShape) { }

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
                                                                 .WaveBoundaryConditionValidator_ValidateSpectralData_Peak_Enhancement_Factor_must_be_a_value_within_the_range_1___10_,
                                                             Boundary));
                }

            }

            /// <summary>
            /// Visit method for doing nothing, since there are not validation rules for this shape.
            /// Must be defined for visitor pattern.
            /// </summary>
            /// <param name="piersonMoskowitzShape"> The visited <see cref="PiersonMoskowitzShape"/></param>
            public void Visit(PiersonMoskowitzShape piersonMoskowitzShape) { }

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
                uniformDataComponent.Data.AcceptVisitor(this);
            }

            /// <summary>
            /// Visit method for validating if there are inactive support points. Calls the next AcceptVisitors methods of the stored data
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
                 List<SupportPoint> activeSupportPoints = spatiallyVaryingDataComponent.Data.Keys.OrderBy(sp => sp.Distance).ToList();

                if (activeSupportPoints.Count < AllDefinedSupportPoints.Count)
                {
                    ValidationIssues.Add(
                        new ValidationIssue(VariableDescription, ValidationSeverity.Info,
                                            Resources
                                                .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_condition_contains_unactivated_support_points,
                                            Boundary));
                }


                IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedDictionary = spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);
                foreach (KeyValuePair<SupportPoint, T> supportPointKeyValuePair in sortedDictionary)
                {
                    supportPointKeyValuePair.Value.AcceptVisitor(this);
                }
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
                if (IsOutsideOfRange(constantParameters.Height, 1, 25))
                {
                    ValidationIssues.Add( new ValidationIssue(VariableDescription,
                                                     ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Height__must_be_greater_than_0_and_smaller_or_equal_to_25_,
                                                     Boundary));
                }

                if (IsOutsideOfRange(constantParameters.Period,0.1, 20.0))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                     ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Period__must_be_a_value_within_the_range_,
                                                     Boundary));
                }

                if (IsOutsideOfRange(constantParameters.Direction,- 360.0, 360.0))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                     ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateSpectrumParameters_Parameter__Direction__must_be_a_value_within_the_range__360___360_,
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
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="timeDependentParameters"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new()
            {
                Ensure.NotNull(timeDependentParameters, nameof(timeDependentParameters));

                IVariable<DateTime> timeArgument = timeDependentParameters.WaveEnergyFunction.TimeArgument;
                
                if (timeArgument?.Values == null || timeArgument.Values.Count == 0)
                {
                    ValidationIssues.Add( new ValidationIssue(null, ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition_Boundary_does_not_contain_a_boundary_condition,
                                                     Boundary));

                }

                IVariable<double> heightComponent = timeDependentParameters.WaveEnergyFunction.HeightComponent;
                
                if (heightComponent?.Values != null && heightComponent.Values.Any(v=> IsOutsideOfRange(v, 0.0, 25.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Hs__in_the_time_series_table_must_be_within_expected_range,
                                                     Boundary));
                }

                IVariable<double> periodComponent = timeDependentParameters.WaveEnergyFunction.PeriodComponent;

                if (periodComponent?.Values != null && periodComponent.Values.Any(v => IsOutsideOfRange(v,0.1, 20.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Tp__in_the_time_series_table_must_be_within_expected_range,
                                                     Boundary));
                }

                IVariable<double> directionComponent = timeDependentParameters.WaveEnergyFunction.DirectionComponent;

                if (directionComponent?.Values != null && directionComponent.Values.Any(v => IsOutsideOfRange(v,-360.0, 360.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Direction__in_the_time_series_table_must_be_within_expected_range,
                                                     Boundary));
                }

                IVariable<double> spreadingComponent = timeDependentParameters.WaveEnergyFunction.SpreadingComponent;

                if (typeof(T) == typeof(PowerDefinedSpreading) && spreadingComponent?.Values != null &&
                    spreadingComponent.Values.Any(v => IsOutsideOfRange(v,1.0, 800.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(null, ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Spreading__in_the_time_series_table_must_be_a_value_within_the_range_1_800,
                                                     Boundary));
                }

                if (typeof(T) == typeof(DegreesDefinedSpreading) && spreadingComponent?.Values != null &&
                    spreadingComponent.Values.Any(v => IsOutsideOfRange(v,2.0, 180.0)))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription, ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Values_in_column__Spreading__in_the_time_series_table_must_be_a_value_within_the_range_2_180,
                                                     Boundary));
                }

                DateTimesPerFunction.Add(timeDependentParameters.WaveEnergyFunction.TimeArgument);
            }

            /// <summary>
            /// Visit method for doing nothing, since there are not validation rules for <see cref="FileBasedParameters"/>.
            /// Must be defined for visitor pattern.
            /// </summary>
            /// <param name="fileBasedParameters"> The visited <see cref="FileBasedParameters"/>. </param>
            public void Visit(FileBasedParameters fileBasedParameters) {}

            /// <summary>
            /// Visit method for validating <see cref="DegreesDefinedSpreading"/>. 
            /// </summary>
            /// <param name="degreesDefinedSpreading"> The visited <see cref="DegreesDefinedSpreading"/></param>
            /// <exception cref="System.ArgumentNullException">
            /// Thrown when <paramref name="degreesDefinedSpreading"/>
            /// is <c>null</c>.
            /// </exception>
            public void Visit(DegreesDefinedSpreading degreesDefinedSpreading)
            {
                Ensure.NotNull(degreesDefinedSpreading, nameof(degreesDefinedSpreading));
                if (IsOutsideOfRange(degreesDefinedSpreading.DegreesSpreading,2.0, 180.0))
                {
                    ValidationIssues.Add(new ValidationIssue(VariableDescription,
                                                     ValidationSeverity.Error,
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Spreading__must_be_a_value_within_the_range_2_180,
                                                     Boundary));
                }
            }

            /// <summary>
            /// Visit method for validating <see cref="PowerDefinedSpreading"/>. 
            /// </summary>
            /// <param name="powerDefinedSpreading"> The visited <see cref="PowerDefinedSpreading"/></param>
            /// <exception cref="System.ArgumentNullException">
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
                                                     Resources
                                                         .WaveBoundaryConditionValidator_ValidateBoundaryCondition__Parameter__Spreading__must_be_a_value_within_the_range_1_800,
                                                     Boundary)); }
            }

            private static bool IsOutsideOfRange(double value, double lowerLimit, double upperLimit) => value - lowerLimit <= -double.Epsilon || value - upperLimit >= double.Epsilon;
        }
    }
}