using DelftTools.Functions;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Converter for converting a collection of boundary <see cref="DelftIniCategory"/>
    /// to a collection of <see cref="IWaveBoundary"/>.
    /// </summary>
    public class WaveBoundaryConverter
    {
        private readonly IImportBoundaryConditionDataComponentFactory importDataComponentFactory;
        private readonly IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveBoundaryConverter"/> class.
        /// </summary>
        /// <param name="importDataComponentFactory">The import data component factory.</param>
        /// <param name="geometricDefinitionFactory">The geometric definition factory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="importDataComponentFactory"/> or
        /// <paramref name="geometricDefinitionFactory"/> is <c>null</c>.
        /// </exception>
        public WaveBoundaryConverter(IImportBoundaryConditionDataComponentFactory importDataComponentFactory,
                                     IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory)
        {
            Ensure.NotNull(importDataComponentFactory, nameof(importDataComponentFactory));
            Ensure.NotNull(geometricDefinitionFactory, nameof(geometricDefinitionFactory));

            this.importDataComponentFactory = importDataComponentFactory;
            this.geometricDefinitionFactory = geometricDefinitionFactory;
        }

        /// <summary>
        /// Converts the specified <paramref name="boundaryCategories"/> to
        /// their respective <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <param name="boundaryCategories">The boundary categories.</param>
        /// <param name="timeSeriesData">The time series data from the .bcw file. </param>
        /// <returns>
        /// The converted collection of <see cref="IWaveBoundary"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryCategories"/> or
        /// <paramref name="timeSeriesData"/> is <c>null</c>.
        /// </exception>
        public IEnumerable<IWaveBoundary> Convert(IEnumerable<DelftIniCategory> boundaryCategories,
                                                  IDictionary<string, List<IFunction>> timeSeriesData)
        {
            Ensure.NotNull(boundaryCategories, nameof(boundaryCategories));
            Ensure.NotNull(timeSeriesData, nameof(timeSeriesData));

            return CreateWaveBoundaries(boundaryCategories, timeSeriesData);
        }

        private IEnumerable<IWaveBoundary> CreateWaveBoundaries(IEnumerable<DelftIniCategory> boundaryCategories,
                                                                IDictionary<string, List<IFunction>> timeSeriesData)
        {
            foreach (DelftIniCategory category in boundaryCategories)
            {
                BoundaryMdwBlock boundaryBlock = BoundaryCategoryConverter.Convert(category);
                if (boundaryBlock.DefinitionType != DefinitionType.Coordinates)
                {
                    continue;
                }

                timeSeriesData.TryGetValue(boundaryBlock.Name, out List<IFunction> timeSeries);

                IWaveBoundaryGeometricDefinition geometricDefinition = GetGeometricDefinition(boundaryBlock);
                IWaveBoundaryConditionDefinition conditionDefinition = GetConditionDefinition(boundaryBlock,
                                                                                              timeSeries,
                                                                                              geometricDefinition);

                yield return new WaveBoundary(boundaryBlock.Name, geometricDefinition, conditionDefinition);
            }
        }

        private IWaveBoundaryGeometricDefinition GetGeometricDefinition(BoundaryMdwBlock boundaryBlock)
        {
            var startCoordinate = new Coordinate(boundaryBlock.XStartCoordinate, boundaryBlock.YStartCoordinate);
            var endCoordinate = new Coordinate(boundaryBlock.XEndCoordinate, boundaryBlock.YEndCoordinate);

            IWaveBoundaryGeometricDefinition geometricDefinition = geometricDefinitionFactory
                .ConstructWaveBoundaryGeometricDefinition(startCoordinate, endCoordinate);

            CreateSupportPoints(boundaryBlock, geometricDefinition);

            return geometricDefinition;
        }

        private IWaveBoundaryConditionDefinition GetConditionDefinition(BoundaryMdwBlock boundaryBlock,
                                                                        IList<IFunction> timeSeriesData,
                                                                        IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            IBoundaryConditionShape shape = GetShape(boundaryBlock);
            BoundaryConditionPeriodType periodType = GetPeriodType(boundaryBlock);
            IBoundaryConditionDataComponent dataComponent = CreateDataComponent(boundaryBlock,
                                                                                timeSeriesData,
                                                                                geometricDefinition);

            return new WaveBoundaryConditionDefinition(shape, periodType, dataComponent);
        }

        private IBoundaryConditionDataComponent CreateParametrizedDataComponent(BoundaryMdwBlock boundaryBlock,
                                                                                IList<IFunction> timeSeriesData,
                                                                                IWaveBoundaryGeometricDefinition
                                                                                    geometricDefinition)
        {
            switch (boundaryBlock.SpreadingType)
            {
                case SpreadingType.Power:
                    return CreateDataComponent<PowerDefinedSpreading>(boundaryBlock, timeSeriesData,
                                                                      geometricDefinition);
                case SpreadingType.Degrees:
                    return CreateDataComponent<DegreesDefinedSpreading>(boundaryBlock, timeSeriesData,
                                                                        geometricDefinition);
                default:
                    throw new NotSupportedException($"Value '{boundaryBlock.SpreadingType}' is not a valid spreading type.");
            }
        }

        private IBoundaryConditionDataComponent CreateDataComponent(BoundaryMdwBlock boundaryBlock,
                                                                    IList<IFunction> timeSeriesData,
                                                                    IWaveBoundaryGeometricDefinition
                                                                        geometricDefinition)
        {
            if (boundaryBlock.SpectrumType == SpectrumType.Parametrized)
            {
                return CreateParametrizedDataComponent(boundaryBlock, timeSeriesData, geometricDefinition);
            }

            throw new NotImplementedException();
        }

        private IBoundaryConditionDataComponent CreateDataComponent<TSpreading>(BoundaryMdwBlock boundaryBlock,
                                                                                IList<IFunction> functions,
                                                                                IWaveBoundaryGeometricDefinition geometricDefinition)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            if (IsSpatiallyVariant(boundaryBlock))
            {
                IEnumerable<SupportPoint> supportPoints = boundaryBlock.Distances
                                                                       .Select(d => GetByDistance(geometricDefinition, d));

                if (IsTimeDependent(functions))
                {
                    IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<TSpreading>>> data =
                        supportPoints.Zip(functions.Select(FromFunction<TSpreading>), Tuple.Create);
                    return importDataComponentFactory.CreateSpatiallyVaryingTimeDependentComponent(data);
                }
                else
                {
                    IEnumerable<Tuple<SupportPoint, ParametersBlock>> data =
                        supportPoints.Zip(GetParametersBlocks(boundaryBlock), Tuple.Create);
                    return importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<TSpreading>(data);
                }
            }

            if (IsTimeDependent(functions))
            {
                IWaveEnergyFunction<TSpreading> data = FromFunction<TSpreading>(functions.First());
                return importDataComponentFactory.CreateUniformTimeDependentComponent(data);
            }
            else
            {
                ParametersBlock data = GetParametersBlocks(boundaryBlock).First();
                return importDataComponentFactory.CreateUniformConstantComponent<TSpreading>(data);
            }
        }

        private static IEnumerable<ParametersBlock> GetParametersBlocks(BoundaryMdwBlock boundaryBlock)
        {
            for (var i = 0; i < boundaryBlock.WaveHeights.Length; i++)
            {
                yield return new ParametersBlock(boundaryBlock.WaveHeights[i],
                                                 boundaryBlock.Periods[i],
                                                 boundaryBlock.Directions[i],
                                                 boundaryBlock.DirectionalSpreadings[i]);
            }
        }

        private static IWaveEnergyFunction<TSpreading> FromFunction<TSpreading>(IFunction function)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            IEnumerable<DateTime> times = GetFunctionValues<DateTime>(
                function.Arguments, WaveParametersConstants.TimeVariableName);
            IEnumerable<double> waveHeights = GetFunctionValues<double>(
                function.Components, WaveParametersConstants.HeightVariableName);
            IEnumerable<double> periods = GetFunctionValues<double>(
                function.Components, WaveParametersConstants.PeriodVariableName);
            IEnumerable<double> spreadings = GetFunctionValues<double>(
                function.Components, WaveParametersConstants.SpreadingVariableName);
            IEnumerable<double> directions = GetFunctionValues<double>(
                function.Components, WaveParametersConstants.DirectionVariableName);

            var waveFunction = new WaveEnergyFunction<TSpreading>();
            waveFunction.TimeArgument.SetValues(times);
            waveFunction.HeightComponent.SetValues(waveHeights);
            waveFunction.PeriodComponent.SetValues(periods);
            waveFunction.DirectionComponent.SetValues(directions);
            waveFunction.SpreadingComponent.SetValues(spreadings);

            return waveFunction;
        }

        private static IEnumerable<T> GetFunctionValues<T>(IEnumerable<IVariable> components, string componentName)
        {
            return components.GetByName(componentName).Values.OfType<T>();
        }

        private static void CreateSupportPoints(BoundaryMdwBlock boundaryBlock,
                                                IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            IEnumerable<double> existingDistances = geometricDefinition.SupportPoints.Select(s => s.Distance);

            IEnumerable<SupportPoint> newSupportPoints = boundaryBlock
                                                         .Distances.Where(d => !Exists(existingDistances, d))
                                                         .Select(d => new SupportPoint(d, geometricDefinition));

            geometricDefinition.SupportPoints.AddRange(newSupportPoints);
        }

        private static bool IsSpatiallyVariant(BoundaryMdwBlock boundaryBlock)
        {
            return boundaryBlock.Distances.Any();
        }

        private static bool IsTimeDependent(IList<IFunction> functions)
        {
            return functions != null;
        }

        private static BoundaryConditionPeriodType GetPeriodType(BoundaryMdwBlock boundaryBlock)
        {
            switch (boundaryBlock.PeriodType)
            {
                case PeriodType.Mean:
                    return BoundaryConditionPeriodType.Mean;
                case PeriodType.Peak:
                    return BoundaryConditionPeriodType.Peak;
                default:
                    throw new NotSupportedException($"Value '{boundaryBlock.PeriodType}' is not a valid period type.");
            }
        }

        private static IBoundaryConditionShape GetShape(BoundaryMdwBlock boundaryBlock)
        {
            switch (boundaryBlock.ShapeType)
            {
                case ShapeType.Gauss:
                    return new GaussShape {GaussianSpread = boundaryBlock.Spreading};
                case ShapeType.Jonswap:
                    return new JonswapShape {PeakEnhancementFactor = boundaryBlock.PeakEnhancementFactor};
                case ShapeType.PiersonMoskowitz:
                    return new PiersonMoskowitzShape();
                default:
                    throw new NotSupportedException($"Value '{boundaryBlock.ShapeType}' is not a valid shape type.");
            }
        }

        private static SupportPoint GetByDistance(IWaveBoundaryGeometricDefinition geometricDefinition, double d)
        {
            return geometricDefinition.SupportPoints.FirstOrDefault(s => DoubleEquals(s.Distance, d));
        }

        private static bool DoubleEquals(double valueA, double valueB)
        {
            return Math.Abs(valueA - valueB) < 0.00001;
        }

        private static bool Exists(IEnumerable<double> values, double value)
        {
            return values.Any(d => DoubleEquals(d, value));
        }
    }
}