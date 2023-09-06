using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DHYDRO.Common.Logging;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class WaveBoundaryConverterTest<T> where T : class, IBoundaryConditionSpreading, new()
    {
        private const double doublePrecision = 1E-7;
        private const string expectedInvalidDistanceMessage = "Boundary 'boundary_name' contains a support point at distance {0}, which is located outside the geometry. This support point will not be imported.";
        private static readonly Random random = new Random(39);
        private readonly SpreadingImportType spreadingType = GetSpreadingImportType();

        private readonly ShapeEqualityComparer shapeComparer = new ShapeEqualityComparer();
        private readonly IForcingTypeDefinedParametersFactory parametersFactory = new ForcingTypeDefinedParametersFactory();
        private static double RandomDouble => Math.Round(random.NextDouble(), 7);

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
        public void Convert_BoundarySectionsNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new WaveBoundaryConverter(Substitute.For<IImportBoundaryConditionDataComponentFactory>(),
                                                      Substitute.For<IWaveBoundaryGeometricDefinitionFactory>());

            // Call
            void Call() => converter.Convert(null, Substitute.For<IDictionary<string, List<IFunction>>>(), "path", Substitute.For<ILogHandler>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundarySections"));
        }

        [Test]
        public void Convert_TimeSeriesDataNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new WaveBoundaryConverter(Substitute.For<IImportBoundaryConditionDataComponentFactory>(),
                                                      Substitute.For<IWaveBoundaryGeometricDefinitionFactory>());

            // Call
            void Call() => converter.Convert(Substitute.For<IEnumerable<IniSection>>(), null, "path", Substitute.For<ILogHandler>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("timeSeriesData"));
        }

        [Test]
        public void Convert_MdwDirPathNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new WaveBoundaryConverter(Substitute.For<IImportBoundaryConditionDataComponentFactory>(),
                                                      Substitute.For<IWaveBoundaryGeometricDefinitionFactory>());

            // Call
            void Call() => converter.Convert(Substitute.For<IEnumerable<IniSection>>(), Substitute.For<IDictionary<string, List<IFunction>>>(), null, Substitute.For<ILogHandler>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("mdwDirPath"));
        }

        [Test]
        public void Convert_LogHandlerNull_ThrowsArgumentNullException()
        {
            // Setup
            var converter = new WaveBoundaryConverter(Substitute.For<IImportBoundaryConditionDataComponentFactory>(),
                                                      Substitute.For<IWaveBoundaryGeometricDefinitionFactory>());

            // Call
            void Call() => converter.Convert(Substitute.For<IEnumerable<IniSection>>(), Substitute.For<IDictionary<string, List<IFunction>>>(), "path", null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("logHandler"));
        }

        [Test]
        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_IniSection_WithUniformConstantData_ReturnsCorrectUniformConstantWaveBoundary(
            ShapeImportType shapeType, PeriodImportExportType periodType,
            IBoundaryConditionShape expectedShape,
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

            IniSection[] sections =
            {
                GetUniformConstantSection(shapeType, periodType, mdwValues)
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
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

        [Test]
        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_IniSection_WithUniformTimeDependentData_ReturnsCorrectUniformTimeDependentWaveBoundary(
            ShapeImportType shapeType, PeriodImportExportType periodType,
            IBoundaryConditionShape expectedShape,
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

            IniSection[] sections =
            {
                GetBoundarySection(shapeType, periodType, mdwValues)
            };
            Dictionary<string, List<IFunction>> timeSeriesData = CreateUniformTimeSeriesData(bcwValues);
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, timeSeriesData, "path", Substitute.For<ILogHandler>())
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

        [Test]
        public void Convert_IniSection_WithUniformFileBasedData_ReturnsCorrectUniformConstantWaveBoundary()
        {
            // Setup
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpreading = RandomDouble;
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var uniformDataComponent = new UniformDataComponent<FileBasedParameters>(parametersFactory.ConstructDefaultFileBasedParameters());
            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();

            importDataComponentFactory.CreateUniformFileBasedComponent(@"C:\path\" + mdwValues.SpectrumFiles.First())
                                      .Returns(uniformDataComponent);

            IniSection[] sections =
            {
                GetUniformFileBasedSection(mdwValues)
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), @"C:\path", Substitute.For<ILogHandler>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.Name, Is.EqualTo("boundary_name"));
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));
            Assert.That(geometricDefinition.SupportPoints, Is.Empty);

            IWaveBoundaryConditionDefinition conditionDefinition = waveBoundary.ConditionDefinition;
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(uniformDataComponent));
        }

        [Test]
        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_IniSection_WithSpatiallyVaryingConstantData_ReturnsCorrectSpatiallyVaryingConstantWaveBoundary(
            ShapeImportType shapeType, PeriodImportExportType periodType,
            IBoundaryConditionShape expectedShape,
            BoundaryConditionPeriodType expectedPeriod,
            double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(1);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[0], geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[1], geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<T>>();
            importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>(
                                                                                      p => MatchesSpatiallyVaryingParameters(p, mdwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            IniSection[] sections =
            {
                GetSpatiallyVaryingConstantSection(shapeType, periodType, mdwValues)
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
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

        [Test]
        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_IniSection_WithSpatiallyVaryingTimeDependentData_ReturnsCorrectSpatiallyVaryingTimeDependentWaveBoundary(
            ShapeImportType shapeType, PeriodImportExportType periodType,
            IBoundaryConditionShape expectedShape,
            BoundaryConditionPeriodType expectedPeriod,
            double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);
            var bcwValues = new BcwTestValues();

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(1);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[0], geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[1], geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<T>>();
            importDataComponentFactory.CreateSpatiallyVaryingTimeDependentComponent(Arg.Is<IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<T>>>>(
                                                                                        p => MatchesSpatiallyVaryingWaveEnergyFunctions(p, mdwValues, bcwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            IniSection[] sections =
            {
                GetSpatiallyVaryingTimeDependentSection(shapeType, periodType, mdwValues)
            };
            Dictionary<string, List<IFunction>> timeSeriesData = GetSpatiallyVaryingTimeSeries(bcwValues);
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, timeSeriesData, "path", Substitute.For<ILogHandler>())
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

        [Test]
        public void Convert_IniSection_WithSpatiallyVaryingFileBasedData_ReturnsCorrectSpatiallyVaryingConstantWaveBoundary()
        {
            // Setup
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpreading = RandomDouble;
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(1);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[0], geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(mdwValues.Distances[1], geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            importDataComponentFactory.CreateSpatiallyVaryingFileBasedComponent(Arg.Is<IEnumerable<Tuple<SupportPoint, string>>>(
                                                                                    p => MatchesSpatiallyVaryingParameters(p, mdwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            IniSection[] sections =
            {
                GetSpatiallyVaryingFileBasedSection(mdwValues)
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), @"C:\path", Substitute.For<ILogHandler>())
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
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(spatiallyVaryingDataComponent));
        }

        [Test]
        [TestCase("non_parametric")]
        public void Convert_IniSection_WithNonParametricData_ThrowsNotSupportedException(string spectrumSpec)
        {
            // Setup
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();

            IniSection[] sections =
            {
                GetBoundarySection(random.NextEnumValue<ShapeImportType>(),
                                    random.NextEnumValue<PeriodImportExportType>(),
                                    mdwValues, spectrumSpec)
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            void Call() => converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>()).ToList();

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void Convert_IniSection_WithDefinitionThatIsNotXYCoordinates_IsSkipped()
        {
            // Setup
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();

            IniSection[] sections =
            {
                GetBoundarySection(random.NextEnumValue<ShapeImportType>(),
                                    random.NextEnumValue<PeriodImportExportType>(),
                                    mdwValues, "parametric", DefinitionImportType.SpectrumFile.GetDescription())
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
                                                  .ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_IniSections_WithSpatiallyVaryingConstantData_WithActiveAndInactiveSupportPoints_ReturnsCorrectSpatiallyVaryingConstantWaveBoundary(
            ShapeImportType shapeType, PeriodImportExportType periodType,
            IBoundaryConditionShape expectedShape,
            BoundaryConditionPeriodType expectedPeriod,
            double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(1);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);
            geometricDefinition.SupportPoints.Add(new SupportPoint(0, geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(10, geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<T>>();
            importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>(
                                                                                      p => MatchesSpatiallyVaryingParameters(p, mdwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            IniSection[] sections =
            {
                GetSpatiallyVaryingConstantSection(shapeType, periodType, mdwValues)
            };
            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.Name, Is.EqualTo("boundary_name"));
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));

            IEventedList<SupportPoint> supportPoints = geometricDefinition.SupportPoints;
            Assert.That(supportPoints, Has.Count.EqualTo(5));
            Assert.That(supportPoints[0].Distance, Is.EqualTo(0));
            Assert.That(supportPoints[1].Distance, Is.EqualTo(10));
            Assert.That(supportPoints[2].Distance, Is.EqualTo(mdwValues.Distances[0]));
            Assert.That(supportPoints[3].Distance, Is.EqualTo(mdwValues.Distances[1]));
            Assert.That(supportPoints[4].Distance, Is.EqualTo(mdwValues.Distances[2]));

            IWaveBoundaryConditionDefinition conditionDefinition = waveBoundary.ConditionDefinition;
            Assert.That(conditionDefinition.Shape, Is.EqualTo(expectedShape).Using(shapeComparer));
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(expectedPeriod));
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(spatiallyVaryingDataComponent));
        }

        [Test]
        public void Convert_TwoIniSections_ReturnsTwoWaveBoundaries()
        {
            // Setup
            var firstMdwValues = new MdwTestValues(RandomDouble, RandomDouble);
            var secondMdwValues = new MdwTestValues(RandomDouble, RandomDouble);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, firstMdwValues);
            geometricDefinitionFactory.ConstructWaveBoundaryGeometricDefinition(
                                          Arg.Is<Coordinate>(c => MatchesCoordinate(c, secondMdwValues.StartX, secondMdwValues.StartY)),
                                          Arg.Is<Coordinate>(c => MatchesCoordinate(c, secondMdwValues.EndX, secondMdwValues.EndY)))
                                      .Returns(geometricDefinition);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            importDataComponentFactory.CreateUniformConstantComponent<T>(Arg.Is<ParametersBlock>(p => MatchesParameters(p, secondMdwValues, 0)))
                                      .Returns(new UniformDataComponent<ConstantParameters<T>>(parametersFactory.ConstructDefaultConstantParameters<T>()));
            importDataComponentFactory.CreateUniformConstantComponent<T>(Arg.Is<ParametersBlock>(p => MatchesParameters(p, firstMdwValues, 0)))
                                      .Returns(new UniformDataComponent<ConstantParameters<T>>(parametersFactory.ConstructDefaultConstantParameters<T>()));

            IniSection firstSection = GetUniformConstantSection(random.NextEnumValue<ShapeImportType>(),
                                                                random.NextEnumValue<PeriodImportExportType>(),
                                                                firstMdwValues);
            const string firstBoundaryName = "boundary_name_1";
            firstSection.AddOrUpdateProperty(KnownWaveProperties.Name, firstBoundaryName);
            IniSection secondSection = GetUniformConstantSection(random.NextEnumValue<ShapeImportType>(),
                                                                 random.NextEnumValue<PeriodImportExportType>(),
                                                                 secondMdwValues);
            const string secondBoundaryName = "boundary_name_2";
            secondSection.AddOrUpdateProperty(KnownWaveProperties.Name, secondBoundaryName);
            IniSection[] sections =
            {
                firstSection,
                secondSection
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo(firstBoundaryName));
            Assert.That(result[1].Name, Is.EqualTo(secondBoundaryName));
        }

        [Test]
        [TestCaseSource(nameof(ShapePeriodTestCases))]
        public void Convert_OrientedBoundary_ExpectedResults(
            ShapeImportType shapeType, PeriodImportExportType periodType,
            IBoundaryConditionShape expectedShape,
            BoundaryConditionPeriodType expectedPeriod,
            double gaussianSpreading, double peakEnhancementFactor)
        {
            // Setup
            var mdwValues = new MdwTestValues(gaussianSpreading, peakEnhancementFactor);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactoryOriented(geometricDefinition, mdwValues);

            var uniformDataComponent = new UniformDataComponent<ConstantParameters<T>>(parametersFactory.ConstructDefaultConstantParameters<T>());
            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            importDataComponentFactory.CreateUniformConstantComponent<T>(Arg.Is<ParametersBlock>(p => MatchesParameters(p, mdwValues, 0)))
                                      .Returns(uniformDataComponent);

            IniSection section = GetBoundarySection(shapeType,
                                                    periodType,
                                                    mdwValues,
                                                    definition: KnownWaveBoundariesFileConstants.OrientationDefinitionType);
            section.AddProperty(KnownWaveProperties.Orientation, mdwValues.OrientationType.GetDescription());
            AddParametersToSection(mdwValues, section, 0);

            IniSection[] sections =
            {
                section
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.Name, Is.EqualTo("boundary_name"));
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));
            Assert.That(geometricDefinition.SupportPoints, Is.Empty);

            geometricDefinitionFactory.Received(1).ConstructWaveBoundaryGeometricDefinition(mdwValues.OrientationType);

            IWaveBoundaryConditionDefinition conditionDefinition = waveBoundary.ConditionDefinition;
            Assert.That(conditionDefinition.Shape, Is.EqualTo(expectedShape).Using(shapeComparer));
            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(expectedPeriod));
            Assert.That(conditionDefinition.DataComponent, Is.SameAs(uniformDataComponent));
        }

        [Test]
        public void GivenAClockwiseOrientedBoundary_WhenConvertIsCalled_ThenTheSupportPointDistancesAreInverted()
        {
            // Setup
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactoryOriented(geometricDefinition, mdwValues);

            geometricDefinition.SupportPoints.Add(new SupportPoint(0, geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(10, geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<T>>();
            importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>(
                                                                                      p => MatchesSpatiallyVaryingParametersInverted(p, mdwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            IniSection section = GetBoundarySection(ShapeImportType.Gauss,
                                                    PeriodImportExportType.Mean,
                                                    mdwValues,
                                                    definition: KnownWaveBoundariesFileConstants.OrientationDefinitionType);

            section.AddProperty(KnownWaveProperties.Orientation, mdwValues.OrientationType.GetDescription());
            section.AddProperty(KnownWaveProperties.DistanceDir, DistanceDirType.Clockwise.GetDescription());
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(mdwValues.Distances[0]));
            AddParametersToSection(mdwValues, section, 0);
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(mdwValues.Distances[1]));
            AddParametersToSection(mdwValues, section, 1);
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(mdwValues.Distances[2]));
            AddParametersToSection(mdwValues, section, 2);

            IniSection[] sections =
            {
                section
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));

            IEventedList<SupportPoint> supportPoints = geometricDefinition.SupportPoints;
            Assert.That(supportPoints, Has.Count.EqualTo(5));
            Assert.That(supportPoints[0].Distance, Is.EqualTo(0));
            Assert.That(supportPoints[1].Distance, Is.EqualTo(10));
            Assert.That(supportPoints[2].Distance, Is.EqualTo(10 - mdwValues.Distances[0]));
            Assert.That(supportPoints[3].Distance, Is.EqualTo(10 - mdwValues.Distances[1]));
            Assert.That(supportPoints[4].Distance, Is.EqualTo(10 - mdwValues.Distances[2]));
        }

        [Test]
        public void GivenAClockwiseCoordinatesBoundary_WhenConvertIsCalled_ThenTheSupportPointDistancesAreInverted()
        {
            // Setup
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);
            geometricDefinitionFactory.HasInvertedOrderingCoordinates(null, null).ReturnsForAnyArgs(true);

            geometricDefinition.SupportPoints.Add(new SupportPoint(0, geometricDefinition));
            geometricDefinition.SupportPoints.Add(new SupportPoint(10, geometricDefinition));

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<T>>();
            importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>(
                                                                                      p => MatchesSpatiallyVaryingParametersInverted(p, mdwValues)))
                                      .Returns(spatiallyVaryingDataComponent);

            IniSection[] section =
            {
                GetSpatiallyVaryingConstantSection(ShapeImportType.Gauss, PeriodImportExportType.Mean, mdwValues)
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(section, new Dictionary<string, List<IFunction>>(), "path", Substitute.For<ILogHandler>())
                                                  .ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));
            IWaveBoundary waveBoundary = result[0];
            Assert.That(waveBoundary.GeometricDefinition, Is.SameAs(geometricDefinition));

            IEventedList<SupportPoint> supportPoints = geometricDefinition.SupportPoints;
            Assert.That(supportPoints, Has.Count.EqualTo(5));
            Assert.That(supportPoints[0].Distance, Is.EqualTo(0));
            Assert.That(supportPoints[1].Distance, Is.EqualTo(10));
            Assert.That(supportPoints[2].Distance, Is.EqualTo(10 - mdwValues.Distances[0]));
            Assert.That(supportPoints[3].Distance, Is.EqualTo(10 - mdwValues.Distances[1]));
            Assert.That(supportPoints[4].Distance, Is.EqualTo(10 - mdwValues.Distances[2]));
        }

        [Test]
        [TestCaseSource(nameof(InvalidDistanceTestCases))]
        public void Convert_InvalidDistanceAndConstantOrientedBoundary_ExpectedResults(double invalidDistance)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactoryOriented(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<T>(Arg.Any<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>())
                                      .Returns(new SpatiallyVaryingDataComponent<ConstantParameters<T>>());

            IniSection section = GetBoundarySection(ShapeImportType.Gauss,
                                                    PeriodImportExportType.Mean,
                                                    mdwValues,
                                                    definition: KnownWaveBoundariesFileConstants.OrientationDefinitionType);
            section.AddProperty(KnownWaveProperties.Orientation, mdwValues.OrientationType.GetDescription());
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(invalidDistance));

            AddParametersToSection(mdwValues, section, 0);

            IniSection[] sections =
            {
                section
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", logHandler).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            Assert.That(geometricDefinition.SupportPoints, Is.Empty);
            logHandler.Received().ReportWarning(string.Format(expectedInvalidDistanceMessage, invalidDistance));
            importDataComponentFactory.Received().CreateSpatiallyVaryingConstantComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>(a => !a.Any()));
        }

        [Test]
        [TestCaseSource(nameof(InvalidDistanceTestCases))]
        public void Convert_InvalidDistanceAndConstantCoordinatesBoundary_ExpectedResults(double invalidDistance)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0 + 1e-8);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            importDataComponentFactory.CreateSpatiallyVaryingConstantComponent<T>(Arg.Any<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>())
                                      .Returns(new SpatiallyVaryingDataComponent<ConstantParameters<T>>());

            IniSection section = GetBoundarySection(ShapeImportType.Gauss,
                                                    PeriodImportExportType.Mean,
                                                    mdwValues);
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(invalidDistance));

            AddParametersToSection(mdwValues, section, 0);

            IniSection[] sections =
            {
                section
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", logHandler).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            Assert.That(geometricDefinition.SupportPoints, Is.Empty);
            logHandler.Received().ReportWarning(string.Format(expectedInvalidDistanceMessage, invalidDistance));
            importDataComponentFactory.Received().CreateSpatiallyVaryingConstantComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, ParametersBlock>>>(a => !a.Any()));
        }

        [Test]
        [TestCaseSource(nameof(InvalidDistanceTestCases))]
        public void Convert_InvalidDistanceAndFileBasedBoundary_ExpectedResults(double invalidDistance)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            importDataComponentFactory.CreateSpatiallyVaryingFileBasedComponent(Arg.Any<IEnumerable<Tuple<SupportPoint, string>>>())
                                      .Returns(new SpatiallyVaryingDataComponent<FileBasedParameters>());

            IniSection section = GetBoundarySection(ShapeImportType.Gauss,
                                                    PeriodImportExportType.Mean,
                                                    mdwValues,
                                                    "from file");
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(invalidDistance));

            AddParametersToSection(mdwValues, section, 0);

            IniSection[] sections =
            {
                section
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            List<IWaveBoundary> result = converter.Convert(sections, new Dictionary<string, List<IFunction>>(), "path", logHandler).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            Assert.That(geometricDefinition.SupportPoints, Is.Empty);
            logHandler.Received().ReportWarning(string.Format(expectedInvalidDistanceMessage, invalidDistance));
            importDataComponentFactory.Received().CreateSpatiallyVaryingFileBasedComponent(Arg.Is<IEnumerable<Tuple<SupportPoint, string>>>(a => !a.Any()));
        }

        [Test]
        [TestCaseSource(nameof(InvalidDistanceTestCases))]
        public void Convert_InvalidDistanceAndTimeDependentBoundary_ExpectedResults(double invalidDistance)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var mdwValues = new MdwTestValues(RandomDouble, RandomDouble);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(10.0);
            IWaveBoundaryGeometricDefinitionFactory geometricDefinitionFactory = GetMockedGeometricDefinitionFactory(geometricDefinition, mdwValues);

            var importDataComponentFactory = Substitute.For<IImportBoundaryConditionDataComponentFactory>();
            importDataComponentFactory.CreateSpatiallyVaryingTimeDependentComponent<T>(Arg.Any<IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<T>>>>())
                                      .Returns(new SpatiallyVaryingDataComponent<TimeDependentParameters<T>>());

            IniSection section = GetBoundarySection(ShapeImportType.Gauss,
                                                    PeriodImportExportType.Mean,
                                                    mdwValues);
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(invalidDistance));

            AddParametersToSection(mdwValues, section, 0);

            IniSection[] sections =
            {
                section
            };

            var converter = new WaveBoundaryConverter(importDataComponentFactory, geometricDefinitionFactory);

            // Call
            Dictionary<string, List<IFunction>> timeSeriesData = GetSpatiallyVaryingTimeSeries(new BcwTestValues());
            List<IWaveBoundary> result = converter.Convert(sections, timeSeriesData, "path", logHandler).ToList();

            // Assert
            Assert.That(result, Has.Count.EqualTo(1));

            Assert.That(geometricDefinition.SupportPoints, Is.Empty);
            logHandler.Received().ReportWarning(string.Format(expectedInvalidDistanceMessage, invalidDistance));
            importDataComponentFactory.Received().CreateSpatiallyVaryingTimeDependentComponent<T>(Arg.Is<IEnumerable<Tuple<SupportPoint, IWaveEnergyFunction<T>>>>(a => !a.Any()));
        }

        private static SpreadingImportType GetSpreadingImportType()
        {
            switch (typeof(T))
            {
                case var a when a == typeof(DegreesDefinedSpreading):
                    return SpreadingImportType.Degrees;
                case var a when a == typeof(PowerDefinedSpreading):
                    return SpreadingImportType.Power;
                default:
                    throw new NotSupportedException();
            }
        }

        private static IEnumerable<TestCaseData> ShapePeriodTestCases()
        {
            double peakEnhancementFactor = RandomDouble;
            double gaussianSpreading = RandomDouble;

            var expectedGaussShape = new GaussShape {GaussianSpread = gaussianSpreading};
            var expectedJonswapShape = new JonswapShape {PeakEnhancementFactor = peakEnhancementFactor};

            yield return new TestCaseData(ShapeImportType.Gauss, PeriodImportExportType.Mean,
                                          expectedGaussShape, BoundaryConditionPeriodType.Mean,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeImportType.Gauss, PeriodImportExportType.Peak,
                                          expectedGaussShape, BoundaryConditionPeriodType.Peak,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeImportType.Jonswap, PeriodImportExportType.Mean,
                                          expectedJonswapShape, BoundaryConditionPeriodType.Mean,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeImportType.Jonswap, PeriodImportExportType.Peak,
                                          expectedJonswapShape, BoundaryConditionPeriodType.Peak,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeImportType.PiersonMoskowitz, PeriodImportExportType.Mean,
                                          new PiersonMoskowitzShape(), BoundaryConditionPeriodType.Mean,
                                          gaussianSpreading, peakEnhancementFactor);
            yield return new TestCaseData(ShapeImportType.PiersonMoskowitz, PeriodImportExportType.Peak,
                                          new PiersonMoskowitzShape(), BoundaryConditionPeriodType.Peak,
                                          gaussianSpreading, peakEnhancementFactor);
        }

        private static IEnumerable<TestCaseData> InvalidDistanceTestCases()
        {
            yield return new TestCaseData(-100.0);
            yield return new TestCaseData(0.0 - 1e-7);
            yield return new TestCaseData(10.0 + 1e-7);
            yield return new TestCaseData(100.0);
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

        private static IWaveBoundaryGeometricDefinitionFactory GetMockedGeometricDefinitionFactoryOriented(
            IWaveBoundaryGeometricDefinition geometricDefinition, MdwTestValues mdw)
        {
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>());

            var geometricDefinitionFactory = Substitute.For<IWaveBoundaryGeometricDefinitionFactory>();
            geometricDefinitionFactory.ConstructWaveBoundaryGeometricDefinition(mdw.OrientationType)
                                      .Returns(geometricDefinition);

            return geometricDefinitionFactory;
        }

        private static bool MatchesCoordinate(Coordinate c, double x, double y) => DoubleEquals(c.X, x) && DoubleEquals(c.Y, y);

        private static bool MatchesParameters(ParametersBlock p, MdwTestValues mdw, int i) =>
            DoubleEquals(p.WaveHeight, mdw.WaveHeights[i]) &&
            DoubleEquals(p.Period, mdw.Periods[i]) &&
            DoubleEquals(p.Direction, mdw.Directions[i]) &&
            DoubleEquals(p.DirectionalSpreading, mdw.DirSpreadings[i]);

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

        private static bool MatchesSpatiallyVaryingParametersInverted(IEnumerable<Tuple<SupportPoint, ParametersBlock>> p, MdwTestValues mdw)
        {
            Tuple<SupportPoint, ParametersBlock> firstPair = p.ElementAt(0);
            Tuple<SupportPoint, ParametersBlock> secondPair = p.ElementAt(1);
            Tuple<SupportPoint, ParametersBlock> thirdPair = p.ElementAt(2);

            return DoubleEquals(firstPair.Item1.Distance, 10.0 - mdw.Distances[0]) &&
                   MatchesParameters(firstPair.Item2, mdw, 0) &&
                   DoubleEquals(secondPair.Item1.Distance, 10.0 - mdw.Distances[1]) &&
                   MatchesParameters(secondPair.Item2, mdw, 1) &&
                   DoubleEquals(thirdPair.Item1.Distance, 10.0 - mdw.Distances[2]) &&
                   MatchesParameters(thirdPair.Item2, mdw, 2);
        }

        private static bool MatchesSpatiallyVaryingParameters(IEnumerable<Tuple<SupportPoint, string>> p, MdwTestValues mdw)
        {
            Tuple<SupportPoint, string> firstPair = p.ElementAt(0);
            Tuple<SupportPoint, string> secondPair = p.ElementAt(1);
            Tuple<SupportPoint, string> thirdPair = p.ElementAt(2);

            return DoubleEquals(firstPair.Item1.Distance, mdw.Distances[0]) &&
                   MatchesParameters(firstPair.Item2, mdw, 0) &&
                   DoubleEquals(secondPair.Item1.Distance, mdw.Distances[1]) &&
                   MatchesParameters(secondPair.Item2, mdw, 1) &&
                   DoubleEquals(thirdPair.Item1.Distance, mdw.Distances[2]) &&
                   MatchesParameters(thirdPair.Item2, mdw, 2);
        }

        private static bool MatchesParameters(string p, MdwTestValues mdw, int i) => p == @"C:\path\" + mdw.SpectrumFiles[i];

        private static bool MatchesWaveEnergyFunction(IWaveEnergyFunction<T> f, BcwTestValues t, int i) =>
            f.DirectionComponent.Values.SequenceEqual(t.Directions[i]) &&
            f.HeightComponent.Values.SequenceEqual(t.WaveHeights[i]) &&
            f.PeriodComponent.Values.SequenceEqual(t.Periods[i]) &&
            f.SpreadingComponent.Values.SequenceEqual(t.DirSpreadings[i]);

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

        private static bool DoubleEquals(double valueA, double valueB) => Math.Abs(valueA - valueB) < doublePrecision;

        private static IFunction CreateTimeSeriesFunction(BcwTestValues values, int i)
        {
            var function = new Function(WaveTimeDependentParametersConstants.WaveQuantityName);
            var timeArgument = new Variable<DateTime>(WaveTimeDependentParametersConstants.TimeVariableName);
            function.Arguments.Add(timeArgument);
            timeArgument.SetValues(new[]
            {
                DateTime.Today,
                DateTime.Today.AddDays(1)
            });
            var heightComponent = new Variable<double>(WaveTimeDependentParametersConstants.HeightVariableName);
            function.Components.Add(heightComponent);
            heightComponent.SetValues(values.WaveHeights[i]);

            var periodComponent = new Variable<double>(WaveTimeDependentParametersConstants.PeriodVariableName);
            function.Components.Add(periodComponent);
            periodComponent.SetValues(values.Periods[i]);

            var directionComponent = new Variable<double>(WaveTimeDependentParametersConstants.DirectionVariableName);
            function.Components.Add(directionComponent);
            directionComponent.SetValues(values.Directions[i]);

            var spreadingComponent = new Variable<double>(WaveTimeDependentParametersConstants.SpreadingVariableName);
            function.Components.Add(spreadingComponent);
            spreadingComponent.SetValues(values.DirSpreadings[i]);

            return function;
        }

        private IniSection GetBoundarySection(ShapeImportType shapeType, PeriodImportExportType periodType, MdwTestValues values,
                                              string spectrumSpec = "parametric",
                                              string definition = "xy-coordinates")
        {
            IniSection section = GetBoundarySection(values, spectrumSpec, definition);

            section.AddProperty(KnownWaveProperties.ShapeType, shapeType.GetDescription());
            section.AddProperty(KnownWaveProperties.PeriodType, periodType.GetDescription());
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingType, spreadingType.GetDescription());

            return section;
        }

        private static IniSection GetBoundarySection(MdwTestValues values,
                                                     string spectrumSpec = "parametric",
                                                     string definition = "xy-coordinates")
        {
            var section = new IniSection(KnownWaveSections.BoundarySection);
            section.AddProperty(KnownWaveProperties.Name, "boundary_name");
            section.AddProperty(KnownWaveProperties.Definition, definition);

            section.AddProperty(KnownWaveProperties.StartCoordinateX, ToString(values.StartX));
            section.AddProperty(KnownWaveProperties.StartCoordinateY, ToString(values.StartY));
            section.AddProperty(KnownWaveProperties.EndCoordinateX, ToString(values.EndX));
            section.AddProperty(KnownWaveProperties.EndCoordinateY, ToString(values.EndY));

            section.AddProperty(KnownWaveProperties.SpectrumSpec, spectrumSpec);

            section.AddProperty(KnownWaveProperties.PeakEnhancementFactor, ToString(values.PeakEnhancementFactor));
            section.AddProperty(KnownWaveProperties.GaussianSpreading, ToString(values.GaussianSpreading));
            return section;
        }

        private IniSection GetUniformConstantSection(ShapeImportType shapeType, PeriodImportExportType periodType, MdwTestValues values)
        {
            IniSection section = GetBoundarySection(shapeType, periodType, values);

            AddParametersToSection(values, section, 0);

            return section;
        }

        private static IniSection GetUniformFileBasedSection(MdwTestValues values)
        {
            IniSection section = GetBoundarySection(values, "from file");

            AddParametersToSection(values, section, 0);

            return section;
        }

        private IniSection GetSpatiallyVaryingConstantSection(ShapeImportType shapeType, PeriodImportExportType periodType, MdwTestValues values)
        {
            IniSection section = GetBoundarySection(shapeType, periodType, values);

            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[0]));
            AddParametersToSection(values, section, 0);

            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[1]));
            AddParametersToSection(values, section, 1);

            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[2]));
            AddParametersToSection(values, section, 2);

            return section;
        }

        private IniSection GetSpatiallyVaryingTimeDependentSection(ShapeImportType shapeType, PeriodImportExportType periodType, MdwTestValues values)
        {
            IniSection section = GetBoundarySection(shapeType, periodType, values);

            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[0]));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[1]));
            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[2]));

            return section;
        }

        private static IniSection GetSpatiallyVaryingFileBasedSection(MdwTestValues values)
        {
            IniSection section = GetBoundarySection(values, "from file");

            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[0]));
            AddParametersToSection(values, section, 0);

            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[1]));
            AddParametersToSection(values, section, 1);

            section.AddProperty(KnownWaveProperties.CondSpecAtDist, ToString(values.Distances[2]));
            AddParametersToSection(values, section, 2);

            return section;
        }

        private static void AddParametersToSection(MdwTestValues values, IniSection section, int i)
        {
            section.AddProperty(KnownWaveProperties.WaveHeight, ToString(values.WaveHeights[i]));
            section.AddProperty(KnownWaveProperties.Period, ToString(values.Periods[i]));
            section.AddProperty(KnownWaveProperties.Direction, ToString(values.Directions[i]));
            section.AddProperty(KnownWaveProperties.DirectionalSpreadingValue, ToString(values.DirSpreadings[i]));
            section.AddProperty(KnownWaveProperties.Spectrum, values.SpectrumFiles[i]);
        }

        private static Dictionary<string, List<IFunction>> CreateUniformTimeSeriesData(BcwTestValues bcwValues) =>
            new Dictionary<string, List<IFunction>> {{"boundary_name", new List<IFunction> {CreateTimeSeriesFunction(bcwValues, 0)}}};

        private static Dictionary<string, List<IFunction>> GetSpatiallyVaryingTimeSeries(BcwTestValues bcwValues) =>
            new Dictionary<string, List<IFunction>>
            {
                {
                    "boundary_name", new List<IFunction>
                    {
                        CreateTimeSeriesFunction(bcwValues, 0),
                        CreateTimeSeriesFunction(bcwValues, 1),
                        CreateTimeSeriesFunction(bcwValues, 2)
                    }
                }
            };

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
            public readonly double StartX = RandomDouble;
            public readonly double StartY = RandomDouble;
            public readonly double EndX = RandomDouble;
            public readonly double EndY = RandomDouble;
            public readonly double PeakEnhancementFactor;
            public readonly double GaussianSpreading;

            public readonly BoundaryOrientationType OrientationType =
                random.NextEnumValue<BoundaryOrientationType>();

            public readonly double[] Distances = GetDataForThreeLocations();

            public readonly double[] WaveHeights = GetDataForThreeLocations();

            public readonly double[] Periods = GetDataForThreeLocations();

            public readonly double[] Directions = GetDataForThreeLocations();

            public readonly double[] DirSpreadings = GetDataForThreeLocations();

            public readonly string[] SpectrumFiles = new[]
            {
                "file 1",
                "file 2",
                "file 3"
            };

            public MdwTestValues(double gaussianSpreading, double peakEnhancementFactor)
            {
                GaussianSpreading = gaussianSpreading;
                PeakEnhancementFactor = peakEnhancementFactor;
            }

            private static double[] GetDataForThreeLocations()
            {
                return new[]
                {
                    RandomDouble,
                    RandomDouble,
                    RandomDouble
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

            public int GetHashCode(IBoundaryConditionShape obj) => throw new NotImplementedException();
        }
    }
}