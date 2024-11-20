using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Geometries;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Factories
{
    [TestFixture]
    public class WaveBoundaryGeometryFactoryTest
    {
        private readonly Random random = new Random(37);

        [Test]
        public void Constructor_GridBoundaryProviderNull_ThrowsArgumentNullException()
        {
            // Setup
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();

            // Call
            void Call() => new WaveBoundaryGeometryFactory(null, calculatorProvider);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("gridBoundaryProvider"));
        }

        [Test]
        public void Constructor_BoundarySnappingCalculatorProviderNull_ThrowsArgumentNullException()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();

            // Call
            void Call() => new WaveBoundaryGeometryFactory(gridBoundaryProvider, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("snappingCalculatorProvider"));
        }

        [Test]
        public void ConstructBoundaryLineGeometry_GridBoundaryNull_ReturnsNull()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            var waveBoundary = Substitute.For<IWaveBoundary>();

            // Call
            ILineString result = factory.ConstructBoundaryLineGeometry(waveBoundary);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ConstructBoundaryLineGeometry_CallingArgumentNameNull_ThrowsArgumentNullException()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            // Call
            void Call() => factory.ConstructBoundaryLineGeometry(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void ConstructBoundaryLineGeometry_ValidInput_ReturnsCorrectLineString()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var gridBoundary = Substitute.For<IGridBoundary>();

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);

            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();
            const int expectedStartingIndex = 3;
            const int expectedEndingIndex = 6;

            const int gridBoundarySize = 9;
            List<Coordinate> coordinates = Enumerable.Range(0, gridBoundarySize)
                                                     .Select(x => new Coordinate(x + 0.5, x * x))
                                                     .ToList();
            List<GridBoundaryCoordinate> gridCoordinates = Enumerable.Range(0, gridBoundarySize)
                                                                     .Select(x => new GridBoundaryCoordinate(gridSide, x))
                                                                     .ToList();

            gridBoundary[gridSide].Returns(gridCoordinates);
            for (var i = 0; i < gridBoundarySize; i++)
            {
                gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridCoordinates[i]).Returns(coordinates[i]);
            }

            var waveBoundaryGeomDef = new WaveBoundaryGeometricDefinition(expectedStartingIndex, expectedEndingIndex, gridSide, length);
            var waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(waveBoundaryGeomDef);

            // Call
            ILineString result = factory.ConstructBoundaryLineGeometry(waveBoundary);

            // Assert
            const int expectedSize = (expectedEndingIndex - expectedStartingIndex) + 1;
            Assert.That(result.NumPoints, Is.EqualTo(expectedSize));

            var coordinateComparer = new Coordinate2DEqualityComparer();
            for (var i = 0; i < expectedSize; i++)
            {
                Assert.That(coordinateComparer.Equals(result.GetCoordinateN(i),
                                                      coordinates[i + expectedStartingIndex]));
            }
        }

        [Test]
        public void ConstructBoundaryEndPoint_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            // Call
            void Call() => factory.ConstructBoundaryEndPoint(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void ConstructBoundaryStartPoint_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            // Call
            void Call() => factory.ConstructBoundaryStartPoint(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void ConstructBoundaryEndPoint_GridBoundaryNull_ReturnsNull()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            var waveBoundary = Substitute.For<IWaveBoundary>();

            // Call
            IPoint result = factory.ConstructBoundaryEndPoint(waveBoundary);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void ConstructBoundaryStartPoint_GridBoundaryNull_ReturnsNull()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            var waveBoundary = Substitute.For<IWaveBoundary>();

            // Call
            IPoint result = factory.ConstructBoundaryStartPoint(waveBoundary);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetConstructBoundaryEndPointsTestData))]
        public void ConstructBoundaryEndPoint_ValidInput_ReturnsCorrectPoint(int firstIndex, int lastIndex, int indexOfInterest, Func<WaveBoundaryGeometryFactory, IWaveBoundary, IPoint> callFunc)
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var gridBoundary = Substitute.For<IGridBoundary>();

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            var waveBoundary = Substitute.For<IWaveBoundary>();

            var gridSide = random.NextEnumValue<GridSide>();
            double length = random.NextDouble();

            var geometricDefinition = new WaveBoundaryGeometricDefinition(firstIndex,
                                                                          lastIndex,
                                                                          gridSide,
                                                                          length);
            waveBoundary.GeometricDefinition.Returns(geometricDefinition);

            GridBoundaryCoordinate[] gridBoundaryCoordinates = Enumerable.Range(0, lastIndex * 2)
                                                                         .Select(x => new GridBoundaryCoordinate(gridSide, x))
                                                                         .ToArray();
            gridBoundary[gridSide].Returns(gridBoundaryCoordinates);

            var expectedCoordinate = new Coordinate(50.0, 100.0);
            gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinates[indexOfInterest])
                        .Returns(expectedCoordinate);

            // Call
            IPoint result = callFunc.Invoke(factory, waveBoundary);

            // Assert
            Assert.That(result, Is.Not.Null);

            var equalityComparer = new Coordinate2DEqualityComparer();
            Assert.That(equalityComparer.Equals(result.Coordinate, expectedCoordinate),
                        "Expected the last points coordinate to be equal to the expected coordinate.");
            gridBoundary.Received(1).GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinates[indexOfInterest]);
        }

        [Test]
        public void ConstructBoundarySupportPoint_SupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();

            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            // Call
            void Call() => factory.ConstructBoundarySupportPoint(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void ConstructBoundarySupportPoint_ReturnsCorrectPoint()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            SupportPoint supportPoint = CreateSupportPoint();

            double x = random.NextDouble();
            double y = random.NextDouble();

            var snappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            calculatorProvider.GetBoundarySnappingCalculator().Returns(snappingCalculator);
            snappingCalculator.CalculateCoordinateFromSupportPoint(supportPoint)
                              .Returns(new Coordinate(x, y));

            // Call
            IPoint point = factory.ConstructBoundarySupportPoint(supportPoint);

            // Assert
            Assert.That(point.X, Is.EqualTo(x).Within(1E-15));
            Assert.That(point.Y, Is.EqualTo(y).Within(1E-15));
        }

        [Test]
        public void ConstructBoundarySupportPoint_WhenCalculatorIsNull_ReturnsNull()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            calculatorProvider.GetBoundarySnappingCalculator().ReturnsNull();

            // Call
            IPoint point = factory.ConstructBoundarySupportPoint(CreateSupportPoint());

            // Assert
            Assert.That(point, Is.Null);
        }

        private static IEnumerable<TestCaseData> GetConstructBoundaryEndPointsTestData()
        {
            IPoint CallStart(WaveBoundaryGeometryFactory factory, IWaveBoundary waveBoundary) =>
                factory.ConstructBoundaryStartPoint(waveBoundary);

            yield return new TestCaseData(5, 10, 5, (Func<WaveBoundaryGeometryFactory, IWaveBoundary, IPoint>) CallStart);

            IPoint CallEnd(WaveBoundaryGeometryFactory factory, IWaveBoundary waveBoundary) =>
                factory.ConstructBoundaryEndPoint(waveBoundary);

            yield return new TestCaseData(5, 10, 10, (Func<WaveBoundaryGeometryFactory, IWaveBoundary, IPoint>) CallEnd);
        }

        private SupportPoint CreateSupportPoint()
        {
            double distance = random.NextDouble();
            var gridSide = random.NextEnumValue<GridSide>();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.GridSide.Returns(gridSide);

            return new SupportPoint(distance, geometricDefinition);
        }
    }
}