using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Converter for converting a collection of boundary <see cref="IniSection"/>
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
        /// Converts the specified <paramref name="boundarySections"/> to
        /// their respective <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <param name="boundarySections">The boundary sections.</param>
        /// <param name="timeSeriesData">The time series data from the .bcw file. </param>
        /// <param name="mdwDirPath">The path to the directory where the .mdw file is located.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <returns>
        /// The converted collection of <see cref="IWaveBoundary"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public IEnumerable<IWaveBoundary> Convert(IEnumerable<IniSection> boundarySections,
                                                  IDictionary<string, List<IFunction>> timeSeriesData,
                                                  string mdwDirPath,
                                                  ILogHandler logHandler)
        {
            Ensure.NotNull(boundarySections, nameof(boundarySections));
            Ensure.NotNull(timeSeriesData, nameof(timeSeriesData));
            Ensure.NotNull(mdwDirPath, nameof(mdwDirPath));
            Ensure.NotNull(logHandler, nameof(logHandler));

            return CreateWaveBoundaries(boundarySections, timeSeriesData, mdwDirPath, logHandler);
        }

        private IEnumerable<IWaveBoundary> CreateWaveBoundaries(IEnumerable<IniSection> boundarySections,
                                                                IDictionary<string, List<IFunction>> timeSeriesData,
                                                                string mdwDirPath,
                                                                ILogHandler logHandler)
        {
            foreach (IniSection section in boundarySections)
            {
                BoundaryMdwBlock boundaryBlock = BoundarySectionConverter.Convert(section, mdwDirPath);

                IWaveBoundaryGeometricDefinition geometricDefinition = GetGeometricDefinition(boundaryBlock, logHandler);

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

        private IWaveBoundaryGeometricDefinition GetGeometricDefinition(BoundaryMdwBlock boundaryBlock, ILogHandler logHandler)
        {
            switch (boundaryBlock.DefinitionType)
            {
                case DefinitionImportType.Coordinates:
                    return GetGeometricDefinitionFromCoordinates(boundaryBlock, logHandler);
                case DefinitionImportType.Oriented:
                    return GetGeometricDefinitionFromOrientation(boundaryBlock, logHandler);
                case DefinitionImportType.SpectrumFile:
                    return null;
                default:
                    throw new InvalidOperationException($"{boundaryBlock.DefinitionType} is an invalid Definition Type.");
            }
        }

        private IWaveBoundaryGeometricDefinition GetGeometricDefinitionFromCoordinates(BoundaryMdwBlock boundaryBlock, ILogHandler logHandler)
        {
            var startCoordinate = new Coordinate(boundaryBlock.XStartCoordinate, boundaryBlock.YStartCoordinate);
            var endCoordinate = new Coordinate(boundaryBlock.XEndCoordinate, boundaryBlock.YEndCoordinate);

            IWaveBoundaryGeometricDefinition geometricDefinition = geometricDefinitionFactory.ConstructWaveBoundaryGeometricDefinition(startCoordinate, endCoordinate);

            if (geometricDefinitionFactory.HasInvertedOrderingCoordinates(geometricDefinition, startCoordinate))
            {
                InvertSupportPointDistances(boundaryBlock, geometricDefinition.Length, logHandler);
            }

            CreateSupportPoints(boundaryBlock, geometricDefinition, logHandler);

            return geometricDefinition;
        }

        private IWaveBoundaryGeometricDefinition GetGeometricDefinitionFromOrientation(BoundaryMdwBlock boundaryBlock, ILogHandler logHandler)
        {
            if (boundaryBlock.OrientationType == null)
            {
                return null;
            }

            logHandler.ReportWarningFormat(Resources.WaveBoundaryConverter_Converting_boundary_this_may_lead_to_unexpected_results,
                                           boundaryBlock.Name,
                                           DefinitionImportType.Oriented.GetDescription(),
                                           DefinitionImportType.Coordinates.GetDescription());

            IWaveBoundaryGeometricDefinition geometricDefinition = geometricDefinitionFactory.ConstructWaveBoundaryGeometricDefinition(boundaryBlock.OrientationType.Value);

            if (boundaryBlock.DistanceDirType == DistanceDirType.Clockwise)
            {
                InvertSupportPointDistances(boundaryBlock, geometricDefinition.Length, logHandler);
            }

            CreateSupportPoints(boundaryBlock, geometricDefinition, logHandler);

            return geometricDefinition;
        }

        private static void InvertSupportPointDistances(BoundaryMdwBlock boundaryBlock,
                                                        double geometricDefinitionLength,
                                                        ILogHandler logHandler)
        {
            logHandler.ReportWarningFormat(Resources.WaveBoundaryConverter_Boundary_is_defined_in_a_clockwise_fashion_and_will_be_converted,
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
                IEnumerable<Tuple<SupportPoint, string>> dataPerSupportPoint = supportPoints.Zip(boundaryBlock.SpectrumFiles, Tuple.Create).Where(data => data.Item1 != null);
                return importDataComponentFactory.CreateSpatiallyVaryingFileBasedComponent(dataPerSupportPoint);
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
                        supportPoints.Zip(functions.Select(CreateWaveEnergyFunction<TSpreading>), Tuple.Create).Where(d => d.Item1 != null);
                    return importDataComponentFactory.CreateSpatiallyVaryingTimeDependentComponent(data);
                }
                else
                {
                    IEnumerable<Tuple<SupportPoint, ParametersBlock>> data =
                        supportPoints.Zip(GetParametersBlocks(boundaryBlock), Tuple.Create).Where(d => d.Item1 != null);
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

        private static void CreateSupportPoints(BoundaryMdwBlock boundaryBlock, IWaveBoundaryGeometricDefinition geometricDefinition, ILogHandler logHandler)
        {
            IEnumerable<double> existingDistances = geometricDefinition.SupportPoints.Select(s => s.Distance).ToArray();

            var newSupportPoints = new List<SupportPoint>();

            foreach (double distance in boundaryBlock.Distances)
            {
                if (Exists(existingDistances, distance))
                {
                    continue;
                }

                if (IsDistanceInsideGeometricDefinition(geometricDefinition, distance))
                {
                    newSupportPoints.Add(new SupportPoint(distance, geometricDefinition));
                }
                else
                {
                    logHandler.ReportWarning(string.Format(Resources.WaveBoundaryConverter_Support_point_outside_geometry_point_will_be_skipped, boundaryBlock.Name, distance));
                }
            }

            geometricDefinition.SupportPoints.AddRange(newSupportPoints);
        }

        private static bool IsDistanceInsideGeometricDefinition(IWaveBoundaryGeometricDefinition geometricDefinition, double distance)
        {
            return distance >= 0.0 && distance <= geometricDefinition.Length;
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