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

            var waveBoundaryGeomDef = new WaveBoundaryGeometricDefinition(expectedStartingIndex, expectedEndingIndex, gridSide);
            var waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(waveBoundaryGeomDef);

            // Call
            ILineString result = factory.ConstructBoundaryLineGeometry(waveBoundary);

            // Assert
            const int expectedSize = expectedEndingIndex - expectedStartingIndex + 1;
            Assert.That(result.NumPoints, Is.EqualTo(expectedSize));

            var coordinateComparer = new Coordinate2DEqualityComparer();
            for (var i = 0; i < expectedSize; i++)
            {

                Assert.That(coordinateComparer.Equals(result.GetCoordinateN(i), 
                                                      coordinates[i + expectedStartingIndex]));
            }
        }

        [Test]
        public void ConstructBoundaryEndPoints_WaveBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            // Call
            void Call() => factory.ConstructBoundaryEndPoints(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundary"));
        }

        [Test]
        public void ConstructBoundaryEndPoints_GridBoundaryNull_ReturnsEmptyEnumerable()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            IGridBoundary gridBoundary = null;

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            var waveBoundary = Substitute.For<IWaveBoundary>();

            // Call
            IEnumerable<IPoint> result = factory.ConstructBoundaryEndPoints(waveBoundary);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ConstructBoundaryEndPoints_ValidInput_ReturnsCorrectPoints()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var gridBoundary = Substitute.For<IGridBoundary>();

            gridBoundaryProvider.GetGridBoundary().Returns(gridBoundary);
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            var waveBoundary = Substitute.For<IWaveBoundary>();

            const int firstIndex = 5;
            const int lastIndex = 10;
            var gridSide = random.NextEnumValue<GridSide>();
            
            var geometricDefinition = new WaveBoundaryGeometricDefinition(firstIndex, 
                                                                          lastIndex, 
                                                                          gridSide);
            waveBoundary.GeometricDefinition.Returns(geometricDefinition);

            var gridBoundaryCoordinates = Enumerable.Range(0, lastIndex * 2)
                                                    .Select(x => new GridBoundaryCoordinate(gridSide, x))
                                                    .ToArray();
            gridBoundary[gridSide].Returns(gridBoundaryCoordinates);

            var expectedFirstCoordinate = new Coordinate(20.0, 40.0);
            var expectedLastCoordinate = new Coordinate(50.0, 100.0);
            gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinates[firstIndex])
                        .Returns(expectedFirstCoordinate);
            gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinates[lastIndex])
                        .Returns(expectedLastCoordinate);

            // Call
            IEnumerable<IPoint> result = factory.ConstructBoundaryEndPoints(waveBoundary);

            // Assert
            List<IPoint> resultList = result.ToList();
            Assert.That(resultList, Is.Not.Null);
            Assert.That(resultList, Has.Count.EqualTo(2));

            var equalityComparer = new Coordinate2DEqualityComparer();
            Assert.That(equalityComparer.Equals(resultList[0]?.Coordinate, expectedFirstCoordinate), 
                        "Expected the first points coordinate to be equal to the expected coordinate.");
            Assert.That(equalityComparer.Equals(resultList[1]?.Coordinate, expectedLastCoordinate), 
                        "Expected the last points coordinate to be equal to the expected coordinate.");
            gridBoundary.Received(1).GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinates[firstIndex]);
            gridBoundary.Received(1).GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinates[lastIndex]);
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
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void ConstructBoundarySupportPoint_ReturnsCorrectPoint()
        {
            // Setup
            var gridBoundaryProvider = Substitute.For<IGridBoundaryProvider>();
            var calculatorProvider = Substitute.For<IBoundarySnappingCalculatorProvider>();
            var factory = new WaveBoundaryGeometryFactory(gridBoundaryProvider, calculatorProvider);

            double distance = random.NextDouble();

            SupportPoint supportPoint = CreateSupportPoint(distance);

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

        private SupportPoint CreateSupportPoint(double distance)
        {
            var gridSide = random.NextEnumValue<GridSide>();
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.GridSide.Returns(gridSide);

            return new SupportPoint(distance, geometricDefinition);
        }
    }
}