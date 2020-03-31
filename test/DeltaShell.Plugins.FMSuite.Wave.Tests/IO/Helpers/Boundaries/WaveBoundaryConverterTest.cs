using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.TestUtils;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Boundaries
{
    [TestFixture(typeof(DegreesDefinedSpreading), SpreadingType.Degrees)]
    [TestFixture(typeof(PowerDefinedSpreading), SpreadingType.Power)]
    public class WaveBoundaryConverterTest<T> where T : class, IBoundaryConditionSpreading, new()
    {
        private static readonly Random random = new Random();
        private static double RandomDouble => Math.Round(random.NextDouble(), 7);
        private readonly SpreadingType spreadingType;
        private readonly ShapeEqualityComparer shapeComparer = new ShapeEqualityComparer();
        private readonly IBoundaryParametersFactory parametersFactory = new BoundaryParametersFactory();

        public WaveBoundaryConverterTest(SpreadingType spreadingType)
        {
            this.spreadingType = spreadingType;
        }

        [Test]
        public void Constructor_ImportDataComponentFactoryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaveBoundaryConverter(null,
                                                     Substitute.For<IWaveBoundaryGeometricDefinitionFactory>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("importDataComponentFactory"));
        }

        [Test]
        public void Constructor_GeometricDefinitionFactoryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new WaveBoundaryConverter(Substitute.For<IImportBoundaryConditionDataComponentFactory>(),
                                                     null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometricDefinitionFactory"));
        }

        [Test]
        public void Convert_BoundaryCategoriesNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new WaveBoundaryConverter(Substitute.For<IImportBoundaryConditionDataComponentFactory>(),
                                                      Substitute.For<IWaveBoundaryGeometricDefinitionFactory>());

            // Call
            void Call() => converter.Convert(null, Substitute.For<IDictionary<string, List<IFunction>>>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaryCategories"));
        }

        [Test]
        public void Convert_TimeSeriesDataNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new WaveBoundaryConverter(Substitute.For<IImportBoundaryConditionDataComponentFactory>(),
                                                      Substitute.For<IWaveBoundaryGeometricDefinitionFactory>());

            // Call
            void Call() => converter.Convert(Substitute.For<IEnumerable<DelftIniCategory>>(), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("timeSeriesData"));
        }

        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_UniformConstantBoundaryData_ReturnsCorrectResult(ShapeType shapeType, PeriodType periodType, IBoundaryConditionShape expectedShape,
                                                                             BoundaryConditionPeriodType expectedPeriod,
                                                                             double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var uniformDataComponent = new UniformDataComponent<ConstantParameters<T>>(parametersFactory.ConstructDefaultConstantParameters<T>());
            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            importDataComponentFactory.CreateUniformConstantComponent<T>(Arg.Is<ParametersBlock>(p => MatchesParameters(p, mdwValues, 0)))
                                      .Returns(uniformDataComponent);

            DelftIniCategory[] categories = {GetUniformConstantCategory(shapeType, periodType, mdwValues)};
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(categories, new Dictionary<string, List<IFunction>>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.Name, Is.EqualTo("boundary_name"));
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));
            Assert.That(geometricDefinition.SupportPoints, Is.Empty);

            IWaveBoundaryConditionDefinition conditionDefinition = waveBoundary.ConditionDefinition;
            Assert.That(conditionDefinition.Shape, Is.EqualTo(expectedShape).Using(shapeComparer));
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(expectedPeriod));
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(uniformDataComponent));
        }

        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_UniformTimeDependentBoundaryData_ReturnsCorrectResult(ShapeType shapeType, PeriodType periodType, IBoundaryConditionShape expectedShape,
                                                                                  BoundaryConditionPeriodType expectedPeriod,
                                                                                  double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);
            var bcwValues = new BcwTestValues();

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var uniformDataComponent = new UniformDataComponent<TimeDependentParameters<T>>(parametersFactory.ConstructDefaultTimeDependentParameters<T>());
            importDataComponentFactory.CreateUniformTimeDependentComponent(Arg.Is<IWaveEnergyFunction<T>>(f => MatchesWaveEnergyFunction(f, bcwValues, 0)))
                                      .Returns(uniformDataComponent);

            DelftIniCategory[] categories = {GetBoundaryCategory(shapeType, periodType, mdwValues)};
            Dictionary<string, List<IFunction>> timeSeriesData = CreateUniformTimeSeriesData(bcwValues);
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(categories, timeSeriesData)
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.Name, Is.EqualTo("boundary_name"));
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));
            Assert.That(geometricDefinition.SupportPoints, Is.Empty);

            IWaveBoundaryConditionDefinition conditionDefinition = waveBoundary.ConditionDefinition;
            Assert.That(conditionDefinition.Shape, Is.EqualTo(expectedShape).Using(shapeComparer));
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(expectedPeriod));
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(uniformDataComponent));
        }

        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_SpatiallyVaryingConstantBoundaryData_ReturnsCorrectResult(ShapeType shapeType, PeriodType periodType, IBoundaryConditionShape expectedShape,
                                                                                      BoundaryConditionPeriodType expectedPeriod,
                                                                                      double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[0], geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[1], geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<T>>();
            importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>(
                                                                                      p => MatchesSpatiallyVaryingParameters(p, mdwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            DelftIniCategory[] categories = {GetSpatiallyVaryingConstantCategory(shapeType, periodType, mdwValues)};
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(categories, new Dictionary<string, List<IFunction>>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.Name, Is.EqualTo("boundary_name"));
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));

            IEventedList<SupportPoint> supportPoints = geometricDefinition.SupportPoints;
            Assert.That(supportPoints, Has.Count.EqualTo(3));
            Assert.That(supportPoints[0].Distance, Is.EqualTo(mdwValues.Distances[0]));
            Assert.That(supportPoints[1].Distance, Is.EqualTo(mdwValues.Distances[1]));
            Assert.That(supportPoints[2].Distance, Is.EqualTo(mdwValues.Distances[2]));

            IWaveBoundaryConditionDefinition conditionDefinition = waveBoundary.ConditionDefinition;
            Assert.That(conditionDefinition.Shape, Is.EqualTo(expectedShape).Using(shapeComparer));
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(expectedPeriod));
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(spatiallyVaryingDataComponent));
        }

        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_SpatiallyVaryingTimeDependentBoundaryData_ReturnsCorrectResult(ShapeType shapeType, PeriodType periodType, IBoundaryConditionShape expectedShape,
                                                                                           BoundaryConditionPeriodType expectedPeriod,
                                                                                           double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);
            var bcwValues = new BcwTestValues();

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[0], geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[1], geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<T>>();
            importDataComponentFactory.CreateSpatiallyVaryingTimeDependentComponent(Arg.Is<IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<T>>>>(
                                                                                        p => MatchesSpatiallyVaryingWaveEnergyFunctions(p, mdwValues, bcwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            DelftIniCategory[] categories = {GetSpatiallyVaryingTimeDependentCategory(shapeType, periodType, mdwValues)};
            Dictionary<string, List<IFunction>> timeSeriesData = GetSpatiallyVaryingTimeSeries(bcwValues);
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(categories, timeSeriesData)
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.Name, Is.EqualTo("boundary_name"));
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));

            IEventedList<SupportPoint> supportPoints = geometricDefinition.SupportPoints;
            Assert.That(supportPoints, Has.Count.EqualTo(3));
            Assert.That(supportPoints[0].Distance, Is.EqualTo(mdwValues.Distances[0]));
            Assert.That(supportPoints[1].Distance, Is.EqualTo(mdwValues.Distances[1]));
            Assert.That(supportPoints[2].Distance, Is.EqualTo(mdwValues.Distances[2]));

            IWaveBoundaryConditionDefinition conditionDefinition = waveBoundary.ConditionDefinition;
            Assert.That(conditionDefinition.Shape, Is.EqualTo(expectedShape).Using(shapeComparer));
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(expectedPeriod));
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(spatiallyVaryingDataComponent));
        }

        [TestCase("from file")]
        [TestCase("non_parametrized")]
        public void Convert_NonParametrizedBoundaryData_ThrowsNotImplementedException(string spectrumSpec)
        {
            // Setup
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();

            DelftIniCategory[] categories =
            {
                GetBoundaryCategory(random.NextEnumValue<ShapeType>(),
                                    random.NextEnumValue<PeriodType>(),
                                    mdwValues, spectrumSpec)
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            void Call() => converter.Convert(categories, new Dictionary<string, List<IFunction>>())
                                    .ToList();

            // Assert
            Assert.Throws<NotImplementedException>(Call);
        }

        private static IEnumerable<TestCaseData> ShapePeriodTestCases()
        {
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpreading = RandomDouble;

            var expectedGaussShape = new GaussShape {GaussianSpread = gaussianSpreading};
            var expectedJonswapShape = new JonswapShape {PeakEnhancementFactor = peakEnhancementFactor};

            yield return new TestCaseData(ShapeType.Gauss, PeriodType.Mean,
                                          expectedGaussShape, BoundaryConditionPeriodType.Mean,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeType.Gauss, PeriodType.Peak,
                                          expectedGaussShape, BoundaryConditionPeriodType.Peak,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeType.Jonswap, PeriodType.Mean,
                                          expectedJonswapShape, BoundaryConditionPeriodType.Mean,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeType.Jonswap, PeriodType.Peak,
                                          expectedJonswapShape, BoundaryConditionPeriodType.Peak,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeType.PiersonMoskowitz, PeriodType.Mean,
                                          new PiersonMoskowitzShape(), BoundaryConditionPeriodType.Mean,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeType.PiersonMoskowitz, PeriodType.Peak,
                                          new PiersonMoskowitzShape(), BoundaryConditionPeriodType.Peak,
                                          gaussianSpreading, peakEnhancementFactor);
        }

        private static IWaveBoundaryGeometricDefinitionFactory GetMockedGeometricDefinitionFactory(
            IWaveBoundaryGeometricDefinition geometricDefinition, MdwTestValues mdw)
        {
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>());

            var geometricDefinitionFactory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();
            geometricDefinitionFactory.ConstructWaveBoundaryGeometricDefinition(
                                          Arg.Is<Coordinate>(c => MatchesCoordinate(c, mdw.StartX, mdw.StartY)),
                                          Arg.Is<Coordinate>(c => MatchesCoordinate(c, mdw.EndX, mdw.EndY)))
                                      .Returns(geometricDefinition);

            return geometricDefinitionFactory;
        }

        private static bool MatchesCoordinate(Coordinate c, double x, double y)
        {
            return DoubleEquals(c.X, x) && DoubleEquals(c.Y, y);
        }

        private static bool MatchesParameters(ParametersBlock p, MdwTestValues mdw, int i)
        {
            return DoubleEquals(p.WaveHeight, mdw.WaveHeights[i]) &&
                   DoubleEquals(p.Period, mdw.Periods[i]) &&
                   DoubleEquals(p.Direction, mdw.Directions[i]) &&
                   DoubleEquals(p.DirectionalSpreading, mdw.DirSpreadings[i]);
        }

        private static bool MatchesSpatiallyVaryingParameters(IEnumerable<Tuple<SupportPoint, ParametersBlock>> p, MdwTestValues mdw)
        {
            Tuple<SupportPoint, ParametersBlock> firstPair = p.ElementAt(0);
            Tuple<SupportPoint, ParametersBlock> secondPair = p.ElementAt(1);
            Tuple<SupportPoint, ParametersBlock> thirdPair = p.ElementAt(2);

            return DoubleEquals(firstPair.Item1.Distance, mdw.Distances[0]) &&
                   MatchesParameters(firstPair.Item2, mdw, 0) &&
                   DoubleEquals(secondPair.Item1.Distance, mdw.Distances[1]) &&
                   MatchesParameters(secondPair.Item2, mdw, 1) &&
                   DoubleEquals(thirdPair.Item1.Distance, mdw.Distances[2]) &&
                   MatchesParameters(thirdPair.Item2, mdw, 2);
        }

        private static bool MatchesWaveEnergyFunction(IWaveEnergyFunction<T> f, BcwTestValues t, int i)
        {
            return f.DirectionComponent.Values.SequenceEqual(t.Directions[i]) &&
                   f.HeightComponent.Values.SequenceEqual(t.WaveHeights[i]) &&
                   f.PeriodComponent.Values.SequenceEqual(t.Periods[i]) &&
                   f.SpreadingComponent.Values.SequenceEqual(t.DirSpreadings[i]);
        }

        private static bool MatchesSpatiallyVaryingWaveEnergyFunctions(IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<T>>> p, MdwTestValues mdw, BcwTestValues bcw)
        {
            Tuple<SupportPoint, IWaveEnergyFunction<T>> firstPair = p.ElementAt(0);
            Tuple<SupportPoint, IWaveEnergyFunction<T>> secondPair = p.ElementAt(1);
            Tuple<SupportPoint, IWaveEnergyFunction<T>> thirdPair = p.ElementAt(2);

            return DoubleEquals(firstPair.Item1.Distance, mdw.Distances[0]) &&
                   MatchesWaveEnergyFunction(firstPair.Item2, bcw, 0) &&
                   DoubleEquals(secondPair.Item1.Distance, mdw.Distances[1]) &&
                   MatchesWaveEnergyFunction(secondPair.Item2, bcw, 1) &&
                   DoubleEquals(thirdPair.Item1.Distance, mdw.Distances[2]) &&
                   MatchesWaveEnergyFunction(thirdPair.Item2, bcw, 2);
        }

        private static bool DoubleEquals(double valueA, double valueB)
        {
            return Math.Abs(valueA - valueB) < 0.00001;
        }

        private static IFunction CreateTimeSeriesFunction(BcwTestValues values, int i)
        {
            var function = new Function(WaveParametersConstants.WaveQuantityName);
            var timeArgument = new Variable<DateTime>(WaveParametersConstants.TimeVariableName);
            function.Arguments.Add(timeArgument);
            timeArgument.SetValues(new[]
            {
                DateTime.Today,
                DateTime.Today.AddDays(1)
            });
            var heightComponent = new Variable<double>(WaveParametersConstants.HeightVariableName);
            function.Components.Add(heightComponent);
            heightComponent.SetValues(values.WaveHeights[i]);

            var periodComponent = new Variable<double>(WaveParametersConstants.PeriodVariableName);
            function.Components.Add(periodComponent);
            periodComponent.SetValues(values.Periods[i]);

            var directionComponent = new Variable<double>(WaveParametersConstants.DirectionVariableName);
            function.Components.Add(directionComponent);
            directionComponent.SetValues(values.Directions[i]);

            var spreadingComponent = new Variable<double>(WaveParametersConstants.SpreadingVariableName);
            function.Components.Add(spreadingComponent);
            spreadingComponent.SetValues(values.DirSpreadings[i]);

            return function;
        }

        private DelftIniCategory GetBoundaryCategory(ShapeType shapeType, PeriodType periodType, MdwTestValues values,
                                                     string spectrumSpec = "parametric")
        {
            var category = new DelftIniCategory(KnownWaveCategories.BoundaryCategory);
            category.AddProperty(KnownWaveProperties.Name, "boundary_name");
            category.AddProperty(KnownWaveProperties.Definition, "xy-coordinates");

            category.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(values.StartX));
            category.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(values.StartY));
            category.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(values.EndX));
            category.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(values.EndY));

            category.AddProperty(KnownWaveProperties.SpectrumSpec, spectrumSpec);

            category.AddProperty(KnownWaveProperties.ShapeType, shapeType.GetDescription());
            category.AddProperty(KnownWaveProperties.PeriodType, periodType.GetDescription());
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingType, spreadingType.GetDescription());

            category.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(values.PeakEnhancementFactor));
            category.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(values.GaussianSpreading));

            return category;
        }

        private DelftIniCategory GetUniformConstantCategory(ShapeType shapeType, PeriodType periodType, MdwTestValues values)
        {
            DelftIniCategory category = GetBoundaryCategory(shapeType, periodType, values);

            AddParametersToCategory(values, category, 0);

            return category;
        }

        private DelftIniCategory GetSpatiallyVaryingConstantCategory(ShapeType shapeType, PeriodType periodType, MdwTestValues values)
        {
            DelftIniCategory category = GetBoundaryCategory(shapeType, periodType, values);

            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[0]));
            AddParametersToCategory(values, category, 0);

            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[1]));
            AddParametersToCategory(values, category, 1);

            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[2]));
            AddParametersToCategory(values, category, 2);

            return category;
        }

        private DelftIniCategory GetSpatiallyVaryingTimeDependentCategory(ShapeType shapeType, PeriodType periodType, MdwTestValues values)
        {
            DelftIniCategory category = GetBoundaryCategory(shapeType, periodType, values);

            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[0]));
            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[1]));
            category.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[2]));

            return category;
        }

        private static void AddParametersToCategory(MdwTestValues values, DelftIniCategory category, int i)
        {
            category.AddProperty(KnownWaveProperties.WaveHeight, ToString(values.WaveHeights[i]));
            category.AddProperty(KnownWaveProperties.Period, ToString(values.Periods[i]));
            category.AddProperty(KnownWaveProperties.Direction, ToString(values.Directions[i]));
            category.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(values.DirSpreadings[i]));
        }

        private static Dictionary<string, List<IFunction>> CreateUniformTimeSeriesData(BcwTestValues bcwValues)
        {
            return new Dictionary<string, List<IFunction>>
            {
                {
                    "boundary_name", new List<IFunction>
                    {
                        CreateTimeSeriesFunction(bcwValues, 0),
                    }
                }
            };
        }

        private static Dictionary<string, List<IFunction>> GetSpatiallyVaryingTimeSeries(BcwTestValues bcwValues)
        {
            return new Dictionary<string, List<IFunction>>
            {
                {
                    "boundary_name", new List<IFunction>
                    {
                        CreateTimeSeriesFunction(bcwValues, 0),
                        CreateTimeSeriesFunction(bcwValues, 1),
                        CreateTimeSeriesFunction(bcwValues, 2),
                    }
                }
            };
        }

        private static string ToString(double value) => value.ToString(CultureInfo.InvariantCulture);

        private class BcwTestValues
        {
            public readonly double[][] WaveHeights = GetDataForThreeLocations();

            public readonly double[][] Periods = GetDataForThreeLocations();

            public readonly double[][] Directions = GetDataForThreeLocations();

            public readonly double[][] DirSpreadings = GetDataForThreeLocations();

            private static double[][] GetDataForThreeLocations()
            {
                return new[]
                {
                    new[]
                    {
                        RandomDouble,
                        RandomDouble
                    },
                    new[]
                    {
                        RandomDouble,
                        RandomDouble
                    },
                    new[]
                    {
                        RandomDouble,
                        RandomDouble
                    }
                };
            }
        }

        private class MdwTestValues
        {
            public MdwTestValues(double gaussianSpreading, double peakEnhancementFactor)
            {
                GaussianSpreading = gaussianSpreading;
                PeakEnhancementFactor = peakEnhancementFactor;
            }

            public readonly double StartX = RandomDouble;
            public readonly double StartY = RandomDouble;
            public readonly double EndX = RandomDouble;
            public readonly double EndY = RandomDouble;
            public readonly double PeakEnhancementFactor;
            public readonly double GaussianSpreading;

            public readonly double[] Distances = GetDataForThreeLocations();

            public readonly double[] WaveHeights = GetDataForThreeLocations();

            public readonly double[] Periods = GetDataForThreeLocations();

            public readonly double[] Directions = GetDataForThreeLocations();

            public readonly double[] DirSpreadings = GetDataForThreeLocations();

            private static double[] GetDataForThreeLocations()
            {
                return new[]
                {
                    RandomDouble,
                    RandomDouble,
                    RandomDouble,
                };
            }
        }

        private class ShapeEqualityComparer : IEqualityComparer<IBoundaryConditionShape>
        {
            public bool Equals(IBoundaryConditionShape x, IBoundaryConditionShape y)
            {
                switch (x)
                {
                    case GaussShape xGaussShape when y is GaussShape yGaussShape:
                        return DoubleEquals(xGaussShape.GaussianSpread, yGaussShape.GaussianSpread);
                    case PiersonMoskowitzShape _ when y is PiersonMoskowitzShape:
                        return true;
                    case JonswapShape xJonswapShape when y is JonswapShape yJonswapShape:
                        return DoubleEquals(xJonswapShape.PeakEnhancementFactor, yJonswapShape.PeakEnhancementFactor);
                    default:
                        return false;
                }
            }

            public int GetHashCode(IBoundaryConditionShape obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
