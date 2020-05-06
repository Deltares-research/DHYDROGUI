using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Converter for converting a collection of boundary <see cref="DelftIniCategory"/>
    /// to a collection of <see cref="IWaveBoundary"/>.
    /// </summary>
    public class WaveBoundaryConverter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaveBoundaryConverter));

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
        /// <param name="mdwDirPath">The path to the directory where the .mdw file is located.</param>
        /// <returns>
        /// The converted collection of <see cref="IWaveBoundary"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public IEnumerable<IWaveBoundary> Convert(IEnumerable<DelftIniCategory> boundaryCategories,
                                                  IDictionary<string, List<IFunction>> timeSeriesData,
                                                  string mdwDirPath)
        {
            Ensure.NotNull(boundaryCategories, nameof(boundaryCategories));
            Ensure.NotNull(timeSeriesData, nameof(timeSeriesData));
            Ensure.NotNull(mdwDirPath, nameof(mdwDirPath));

            return CreateWaveBoundaries(boundaryCategories, timeSeriesData, mdwDirPath);
        }

        private IEnumerable<IWaveBoundary> CreateWaveBoundaries(IEnumerable<DelftIniCategory> boundaryCategories,
                                                                IDictionary<string, List<IFunction>> timeSeriesData, 
                                                                string mdwDirPath)
        {
            foreach (DelftIniCategory category in boundaryCategories)
            {
                BoundaryMdwBlock boundaryBlock = BoundaryCategoryConverter.Convert(category, mdwDirPath);
                
                IWaveBoundaryGeometricDefinition geometricDefinition = GetGeometricDefinition(boundaryBlock);

                if (geometricDefinition == null)
                {
                    continue;
                }

                timeSeriesData.TryGetValue(boundaryBlock.Name, out List<IFunction> timeSeries);
                IWaveBoundaryConditionDefinition conditionDefinition = GetConditionDefinition(boundaryBlock,
                                                                                              timeSeries,
                                                                                              geometricDefinition);

                yield return new WaveBoundary(boundaryBlock.Name, geometricDefinition, conditionDefinition);
            }
        }

        private IWaveBoundaryGeometricDefinition GetGeometricDefinition(BoundaryMdwBlock boundaryBlock)
        {
            switch (boundaryBlock.DefinitionType)
            {
                case DefinitionImportType.Coordinates:
                    return GetGeometricDefinitionFromCoordinates(boundaryBlock);
                case DefinitionImportType.Oriented:
                    return GetGeometricDefinitionFromOrientation(boundaryBlock);
                case DefinitionImportType.SpectrumFile:
                default:
                    return null;
            }
        }

        private IWaveBoundaryGeometricDefinition GetGeometricDefinitionFromCoordinates(BoundaryMdwBlock boundaryBlock)
        {
            var startCoordinate = new Coordinate(boundaryBlock.XStartCoordinate, boundaryBlock.YStartCoordinate);
            var endCoordinate = new Coordinate(boundaryBlock.XEndCoordinate, boundaryBlock.YEndCoordinate);

            IWaveBoundaryGeometricDefinition geometricDefinition = geometricDefinitionFactory
                .ConstructWaveBoundaryGeometricDefinition(startCoordinate, endCoordinate);

            if (geometricDefinitionFactory.HasInvertedOrderingCoordinates(geometricDefinition, startCoordinate))
            {
                InvertSupportPointDistances(boundaryBlock, geometricDefinition.Length);
            }

            CreateSupportPoints(boundaryBlock, geometricDefinition);

            return geometricDefinition;
        }

        private IWaveBoundaryGeometricDefinition GetGeometricDefinitionFromOrientation(BoundaryMdwBlock boundaryBlock)
        {
            if (boundaryBlock.OrientationType == null)
            {
                return null;
            }

            log.WarnFormat("Converting boundary '{0}', from {1} to {2}, this may lead to unexpected results, please inspect your boundaries.",
                           boundaryBlock.Name, 
                           DefinitionImportType.Oriented.GetDescription(), 
                           DefinitionImportType.Coordinates.GetDescription());

            IWaveBoundaryGeometricDefinition geometricDefinition = geometricDefinitionFactory
                .ConstructWaveBoundaryGeometricDefinition(boundaryBlock.OrientationType.Value);

            if (boundaryBlock.DistanceDirType == DistanceDirType.Clockwise)
            {
                InvertSupportPointDistances(boundaryBlock, geometricDefinition.Length);
            }

            CreateSupportPoints(boundaryBlock, geometricDefinition);

            return geometricDefinition;
        }

        private static void InvertSupportPointDistances(BoundaryMdwBlock boundaryBlock,
                                                        double geometricDefinitionLength)
        {
            log.WarnFormat("Boundary '{0}' is defined in a clockwise fashion. This boundary will be converted to a " +
                           "counter-clockwise, any support points distances will be adjusted accordingly. This may " +
                           "lead to unexpected results, please inspect your support points.",
                           boundaryBlock.Name);
            boundaryBlock.Distances = boundaryBlock.Distances.Select(d => geometricDefinitionLength - d).ToArray();
        }

        private IWaveBoundaryConditionDefinition GetConditionDefinition(BoundaryMdwBlock boundaryBlock,
                                                                        IList<IFunction> timeSeriesData,
                                                                        IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            IBoundaryConditionShape shape = GetShape(boundaryBlock);
            BoundaryConditionPeriodType periodType = GetPeriodType(boundaryBlock);
            ISpatiallyDefinedDataComponent dataComponent = CreateDataComponent(boundaryBlock,
                                                                               timeSeriesData,
                                                                               geometricDefinition);

            return new WaveBoundaryConditionDefinition(shape, periodType, dataComponent);
        }

        private ISpatiallyDefinedDataComponent CreateParametrizedDataComponent(BoundaryMdwBlock boundaryBlock,
                                                                               IList<IFunction> timeSeriesData,
                                                                               IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            switch (boundaryBlock.SpreadingType)
            {
                case SpreadingImportType.Power:
                    return CreateDataComponent<PowerDefinedSpreading>(boundaryBlock, timeSeriesData, geometricDefinition);
                case SpreadingImportType.Degrees:
                    return CreateDataComponent<DegreesDefinedSpreading>(boundaryBlock, timeSeriesData, geometricDefinition);
                default:
                    throw new NotSupportedException($"Value '{boundaryBlock.SpreadingType}' is not a valid spreading type.");
            }
        }

        private ISpatiallyDefinedDataComponent CreateFileBasedDataComponent(BoundaryMdwBlock boundaryBlock, IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            if (IsSpatiallyVariant(boundaryBlock))
            {
                IEnumerable<SupportPoint> supportPoints = boundaryBlock.Distances.Select(d => GetSupportPointWithDistance(geometricDefinition, d));
                return importDataComponentFactory.CreateSpatiallyVaryingFileBasedComponent(supportPoints.Zip(boundaryBlock.SpectrumFiles, Tuple.Create));
            }

            return importDataComponentFactory.CreateUniformFileBasedComponent(boundaryBlock.SpectrumFiles.FirstOrDefault());
        }


        private ISpatiallyDefinedDataComponent CreateDataComponent(BoundaryMdwBlock boundaryBlock,
                                                                   IList<IFunction> timeSeriesData,
                                                                   IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            switch (boundaryBlock.SpectrumType)
            {
                case SpectrumImportExportType.Parametrized:
                    return CreateParametrizedDataComponent(boundaryBlock, timeSeriesData, geometricDefinition);
                case SpectrumImportExportType.FromFile:
                    return CreateFileBasedDataComponent(boundaryBlock, geometricDefinition);
                default:
                    throw new NotSupportedException($"Spectrum type {boundaryBlock.SpectrumType} is not supported.");
            }
        }

        private ISpatiallyDefinedDataComponent CreateDataComponent<TSpreading>(BoundaryMdwBlock boundaryBlock,
                                                                               IList<IFunction> functions,
                                                                               IWaveBoundaryGeometricDefinition geometricDefinition)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            if (IsSpatiallyVariant(boundaryBlock))
            {
                IEnumerable<SupportPoint> supportPoints = boundaryBlock.Distances
                                                                       .Select(d => GetSupportPointWithDistance(geometricDefinition, d));

                if (IsTimeDependent(functions))
                {
                    IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<TSpreading>>> data =
                        supportPoints.Zip(functions.Select(CreateWaveEnergyFunction<TSpreading>), Tuple.Create);
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
                IWaveEnergyFunction<TSpreading> data = CreateWaveEnergyFunction<TSpreading>(functions.First());
                return importDataComponentFactory.CreateUniformTimeDependentComponent(data);
            }
            else
            {
                ParametersBlock data = GetParametersBlocks(boundaryBlock).FirstOrDefault();
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

        private static IWaveEnergyFunction<TSpreading> CreateWaveEnergyFunction<TSpreading>(IFunction function)
            where TSpreading : class, IBoundaryConditionSpreading, new()
        {
            IEnumerable<DateTime> times = GetFunctionValues<DateTime>(
                function.Arguments, WaveTimeDependentParametersConstants.TimeVariableName);
            IEnumerable<double> waveHeights = GetFunctionValues<double>(
                function.Components, WaveTimeDependentParametersConstants.HeightVariableName);
            IEnumerable<double> periods = GetFunctionValues<double>(
                function.Components, WaveTimeDependentParametersConstants.PeriodVariableName);
            IEnumerable<double> spreadings = GetFunctionValues<double>(
                function.Components, WaveTimeDependentParametersConstants.SpreadingVariableName);
            IEnumerable<double> directions = GetFunctionValues<double>(
                function.Components, WaveTimeDependentParametersConstants.DirectionVariableName);

            var waveFunction = new WaveEnergyFunction<TSpreading>();
            waveFunction.TimeArgument.SetValues(times);
            waveFunction.HeightComponent.SetValues(waveHeights);
            waveFunction.PeriodComponent.SetValues(periods);
            waveFunction.DirectionComponent.SetValues(directions);
            waveFunction.SpreadingComponent.SetValues(spreadings);

            return waveFunction;
        }

        private static IEnumerable<T> GetFunctionValues<T>(IEnumerable<IVariable> components, string componentName) => components.GetByName(componentName).Values.OfType<T>();

        private static void CreateSupportPoints(BoundaryMdwBlock boundaryBlock, IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            IEnumerable<double> existingDistances = geometricDefinition.SupportPoints.Select(s => s.Distance);

            IEnumerable<SupportPoint> newSupportPoints = boundaryBlock
                                                         .Distances.Where(d => !Exists(existingDistances, d))
                                                         .Select(d => new SupportPoint(d, geometricDefinition));

            geometricDefinition.SupportPoints.AddRange(newSupportPoints);
        }

        private static bool IsSpatiallyVariant(BoundaryMdwBlock boundaryBlock) => boundaryBlock.Distances.Any();

        private static bool IsTimeDependent(IList<IFunction> functions) => functions != null;

        private static BoundaryConditionPeriodType GetPeriodType(BoundaryMdwBlock boundaryBlock)
        {
            switch (boundaryBlock.PeriodType)
            {
                case PeriodImportExportType.Mean:
                    return BoundaryConditionPeriodType.Mean;
                case PeriodImportExportType.Peak:
                    return BoundaryConditionPeriodType.Peak;
                default:
                    throw new NotSupportedException($"Value '{boundaryBlock.PeriodType}' is not a valid period type.");
            }
        }

        private static IBoundaryConditionShape GetShape(BoundaryMdwBlock boundaryBlock)
        {
            switch (boundaryBlock.ShapeType)
            {
                case ShapeImportType.Gauss:
                    return new GaussShape {GaussianSpread = boundaryBlock.Spreading};
                case ShapeImportType.Jonswap:
                    return new JonswapShape {PeakEnhancementFactor = boundaryBlock.PeakEnhancementFactor};
                case ShapeImportType.PiersonMoskowitz:
                    return new PiersonMoskowitzShape();
                default:
                    throw new NotSupportedException($"Value '{boundaryBlock.ShapeType}' is not a valid shape type.");
            }
        }

        private static SupportPoint GetSupportPointWithDistance(IWaveBoundaryGeometricDefinition geometricDefinition, double d)
        {
            return geometricDefinition.SupportPoints.FirstOrDefault(s => SpatialDouble.AreEqual(s.Distance, d));
        }

        private static bool Exists(IEnumerable<double> values, double value)
        {
            return values.Any(d => SpatialDouble.AreEqual(d, value));
        }
    }
}