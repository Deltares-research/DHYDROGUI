using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Calculators
{
    [TestFixture]
    public class BoundarySnappingCalculatorTest
    {
        private static readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var gridBoundary = Substitute.For<IGridBoundary>();

            // Call
            var calculator = new BoundarySnappingCalculator(gridBoundary);

            // Assert
            Assert.That(calculator, Is.InstanceOf<IBoundarySnappingCalculator>());
            Assert.That(calculator.GridBoundary, Is.SameAs(gridBoundary));
            Assert.That(calculator.DistanceCalculator, Is.Not.Null);
        }

        [Test]
        public void Constructor_GridBoundaryNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BoundarySnappingCalculator(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("gridBoundary"));
        }

        [Test]
        public void SnapCoordinateToGridBoundaryCoordinate_CoordinateToSnapNull_ThrowsArgumentNullException()
        {
            // Setup 
            var gridBoundary = Substitute.For<IGridBoundary>();
            var calculator = new BoundarySnappingCalculator(gridBoundary);

            // Call
            void Call() => calculator.SnapCoordinateToGridBoundaryCoordinate(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("coordinateToSnap"));
        }

        [Test]
        [TestCase(double.NaN, 1.0)]
        [TestCase(double.PositiveInfinity, 1.0)]
        [TestCase(double.NegativeInfinity, 1.0)]
        [TestCase(1.0, double.NaN)]
        [TestCase(1.0, double.PositiveInfinity)]
        [TestCase(1.0, double.NegativeInfinity)]
        [TestCase(double.NaN, double.NaN)]
        [TestCase(double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity, double.NegativeInfinity)]
        public void SnapCoordinateToGridBoundaryCoordinate_CoordinateUndefined_ReturnsEmptyEnumerable(double x, double y)
        {
            // Setup 
            var gridBoundary = Substitute.For<IGridBoundary>();
            var calculator = new BoundarySnappingCalculator(gridBoundary);
            var coordinate = new Coordinate(x, y);

            // Call
            IEnumerable<GridBoundaryCoordinate> result = calculator.SnapCoordinateToGridBoundaryCoordinate(coordinate);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GivenABoundarySnappingCalculator_WhenSnapCoordinateToGridBoundaryIsCalled_ThenTheCorrectCoordinateIsReturned()
        {
            // Setup
            const int x = 5;
            const int y = 5;
            IDiscreteGridPointCoverage grid = GridBoundaryTestHelper.GetValidGridMock(x, y);

            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    grid.X.Values[i, j] = i;
                    grid.Y.Values[i, j] = j;
                }
            }

            var gridBoundary = new GridBoundary(grid);
            var calculator = new BoundarySnappingCalculator(gridBoundary);
            var coordinateRef = new Coordinate(5.0, 2.0);

            // Call
            IEnumerable<GridBoundaryCoordinate> result = calculator.SnapCoordinateToGridBoundaryCoordinate(coordinateRef);

            // Assert
            var expectedResult = new List<GridBoundaryCoordinate>() {new GridBoundaryCoordinate(GridSide.East, 2)};

            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GivenABoundarySnappingCalculator_WhenSnapCoordinateToGridBoundaryIsCalledThatIsClosestToMultipleGridBoudnaryCoordinates_ThenTheCorrectCoordinatesAreReturned()
        {
            // Setup
            const int x = 5;
            const int y = 5;
            IDiscreteGridPointCoverage grid = GridBoundaryTestHelper.GetValidGridMock(x, y);

            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    grid.X.Values[i, j] = i;
                    grid.Y.Values[i, j] = j;
                }
            }

            var gridBoundary = new GridBoundary(grid);
            var calculator = new BoundarySnappingCalculator(gridBoundary);
            var coordinateRef = new Coordinate(5.0, 5.0);

            // Call
            IEnumerable<GridBoundaryCoordinate> result = calculator.SnapCoordinateToGridBoundaryCoordinate(coordinateRef);

            // Assert
            var expectedResult = new List<GridBoundaryCoordinate>()
            {
                new GridBoundaryCoordinate(GridSide.East, 4),
                new GridBoundaryCoordinate(GridSide.North, 0)
            };

            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void GivenABoundarySnappingCalculator_WhenSnapCoordinateToGridBoundaryIsCalledWithAToleranceAndASourcePointOutsideOfTheTolerance_ThenAnEmptyCoordinateSetIsReturned()
        {
            // Setup
            const int x = 5;
            const int y = 5;
            IDiscreteGridPointCoverage grid = GridBoundaryTestHelper.GetValidGridMock(x, y);

            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    grid.X.Values[i, j] = i;
                    grid.Y.Values[i, j] = j;
                }
            }

            var gridBoundary = new GridBoundary(grid);
            var calculator = new BoundarySnappingCalculator(gridBoundary);
            var coordinateRef = new Coordinate(8.0, 8.0);

            // Call
            IEnumerable<GridBoundaryCoordinate> result = calculator.SnapCoordinateToGridBoundaryCoordinate(coordinateRef, 0.5);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GivenABoundarySnappingCalculator_WhenSnapCoordinateToGridBoundaryIsCalledWithAToleranceAndASourcePointInsideOfTheTolerance_ThenTheCorrectSetIsReturned()
        {
            // Setup
            const int x = 5;
            const int y = 5;
            IDiscreteGridPointCoverage grid = GridBoundaryTestHelper.GetValidGridMock(x, y);

            var gridBoundary = new GridBoundary(grid);
            var calculator = new BoundarySnappingCalculator(gridBoundary);
            var coordinateRef = new Coordinate(8.0, 8.0);

            // Call
            IEnumerable<GridBoundaryCoordinate> result = calculator.SnapCoordinateToGridBoundaryCoordinate(coordinateRef, 10.0);

            // Assert
            var expectedResult = new List<GridBoundaryCoordinate>()
            {
                new GridBoundaryCoordinate(GridSide.East, 4),
                new GridBoundaryCoordinate(GridSide.North, 0)
            };

            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [Test]
        public void CalculateCoordinateFromSupportPoint_SupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var calculator = new BoundarySnappingCalculator(Substitute.For<IGridBoundary>());

            // Call
            void Call() => calculator.CalculateCoordinateFromSupportPoint(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("supportPoint"));
        }

        [Test]
        public void CalculateDistanceBetweenBoundaryIndices_CorrectValueIsReturned()
        {
            // Setup
            const int x = 10;
            const int y = 10;

            GridBoundary gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(x, y, out IDiscreteGridPointCoverage _);

            var calculator = new BoundarySnappingCalculator(gridBoundary);

            int indexA = random.Next(9);
            int indexB = random.Next(9);

            // Call
            double value = calculator.CalculateDistanceBetweenBoundaryIndices(indexA, indexB, random.NextEnumValue<GridSide>());

            // Assert
            Assert.That(value, Is.EqualTo(Math.Abs(indexA - indexB)));
        }

        [Test]
        public void CalculateDistanceBetweenBoundaryIndices_UndefinedGridSide_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var gridBoundary = Substitute.For<IGridBoundary>();
            var calculator = new BoundarySnappingCalculator(gridBoundary);

            // Call
            void Call() => calculator.CalculateDistanceBetweenBoundaryIndices(random.Next(),
                                                                              random.Next(),
                                                                              0);

            // Assert
            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("gridSide"));
        }

        [TestCaseSource(nameof(CoordinateFromDistanceTestData))]
        public void CalculateCoordinateFromSupportPoint_CorrectResultIsReturned(SupportPoint supportPoint,
                                                                                Coordinate expectedCoordinate)
        {
            // Setup
            const int x = 10;
            const int y = 10;

            GridBoundary gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(x, y, out IDiscreteGridPointCoverage _);

            var calculator = new BoundarySnappingCalculator(gridBoundary);

            // Call
            Coordinate result = calculator.CalculateCoordinateFromSupportPoint(supportPoint);

            // Assert
            Assert.That(result.Equals2D(expectedCoordinate, 1E-15), $"Expected: {expectedCoordinate} \n" +
                                                                    $"But was:  {result}.");
        }

        [TestCase(2, -1, "indexB")]
        [TestCase(2, 5, "indexB")]
        [TestCase(-1, 2, "indexA")]
        [TestCase(5, 2, "indexA")]
        public void CalculateDistanceBetweenBoundaryIndices_IndexOutOfRange_ThrowsArgumentOutOfRangeException(int indexA, int indexB, string paramName)
        {
            // Setup
            const int x = 4;
            const int y = 4;

            GridBoundary gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(x, y, out IDiscreteGridPointCoverage _);
            var calculator = new BoundarySnappingCalculator(gridBoundary);

            // Call
            void Call() => calculator.CalculateDistanceBetweenBoundaryIndices(indexA,
                                                                              indexB,
                                                                              random.NextEnumValue<GridSide>());

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(paramName));
        }

        /// <remarks>
        /// Assumes grid of 10x10;
        /// </remarks>
        private static IEnumerable<TestCaseData> CoordinateFromDistanceTestData()
        {
            const int maxIndex = 9;

            int startIndex = random.Next(maxIndex - 1);
            int endIndex = random.Next(startIndex + 1, maxIndex);
            double distance = random.NextDouble() * (endIndex - startIndex);

            yield return new TestCaseData(CreateSupportPoint(distance, GridSide.North, startIndex, endIndex),
                                          new Coordinate(maxIndex - (distance + startIndex), 9));
            yield return new TestCaseData(CreateSupportPoint(distance, GridSide.East, startIndex, endIndex),
                                          new Coordinate(maxIndex, distance + startIndex));
            yield return new TestCaseData(CreateSupportPoint(distance, GridSide.South, startIndex, endIndex),
                                          new Coordinate(distance + startIndex, 0));
            yield return new TestCaseData(CreateSupportPoint(distance, GridSide.West, startIndex, endIndex),
                                          new Coordinate(0, maxIndex - (distance + startIndex)));
        }

        private static SupportPoint CreateSupportPoint(double distance, GridSide side, int start, int end)
        {
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            geometricDefinition.GridSide.Returns(side);
            geometricDefinition.StartingIndex.Returns(start);
            geometricDefinition.EndingIndex.Returns(end);

            return new SupportPoint(distance, geometricDefinition);
        }
    }
}