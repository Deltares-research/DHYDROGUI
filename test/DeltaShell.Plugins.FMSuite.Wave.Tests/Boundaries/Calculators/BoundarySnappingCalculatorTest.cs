using System;
using System.Collections.Generic;
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
        private Random random = new Random();

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
            var expectedResult = new List<GridBoundaryCoordinate>() { new GridBoundaryCoordinate(GridSide.East, 2) };

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
                new GridBoundaryCoordinate(GridSide.East, 0),
                new GridBoundaryCoordinate(GridSide.North, 4),
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

            SetGridValues(grid, x, y);

            var gridBoundary = new GridBoundary(grid);
            var calculator = new BoundarySnappingCalculator(gridBoundary);
            var coordinateRef = new Coordinate(8.0, 8.0);

            // Call
            IEnumerable<GridBoundaryCoordinate> result = calculator.SnapCoordinateToGridBoundaryCoordinate(coordinateRef, 10.0);

            // Assert
            var expectedResult = new List<GridBoundaryCoordinate>()
            {
                new GridBoundaryCoordinate(GridSide.East, 0),
                new GridBoundaryCoordinate(GridSide.North, 4),
            };

            Assert.That(result, Is.EquivalentTo(expectedResult));
        }

        [TestCaseSource(nameof(CoordinateFromDistanceTestData))]
        public void CalculateCoordinateFromDistance_WithValidDistance_ThenCorrectResultIsReturned(GridSide gridSide, double distance, Coordinate expectedCoordinate)
        {
            // Setup
            const int x = 5;
            const int y = 5;

            GridBoundary gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(x, y, out IDiscreteGridPointCoverage grid);
            SetGridValues(grid, x, y);

            var calculator = new BoundarySnappingCalculator(gridBoundary);

            // Call
            Coordinate result = calculator.CalculateCoordinateFromDistance(distance, gridSide);

            // Assert
            Assert.That(result.Equals2D(expectedCoordinate, 1E-15), $"Expected: {expectedCoordinate} \n" +
                                                                    $"But was:  {result}.");
        }

        [Test]
        public void CalculateCoordinateFromDistance_WhenDistanceIsSmallerThanZero_ThenArgumentOutOfRangeExceptionIsThrown()
        {
            // Setup
            const int x = 5;
            const int y = 5;

            GridBoundary gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(x, y);

            var calculator = new BoundarySnappingCalculator(gridBoundary);

            double distance = random.NextDouble() * -1;

            // Call
            void Call() => calculator.CalculateCoordinateFromDistance(distance, random.NextEnumValue<GridSide>());

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("distance"));
            Assert.That(exception.Message, Is.StringStarting("Distance cannot be smaller than 0"));
        }

        private static void SetGridValues(IDiscreteGridPointCoverage grid, int x, int y)
        {
            for (var i = 0; i < x; i++)
            {
                for (var j = 0; j < y; j++)
                {
                    grid.X.Values[i, j] = i;
                    grid.Y.Values[i, j] = j;
                }
            }
        }

        private IEnumerable<TestCaseData> CoordinateFromDistanceTestData()
        {
            double distance = random.NextDouble() * 4;

            yield return new TestCaseData(GridSide.North, distance, new Coordinate(distance, 4));
            yield return new TestCaseData(GridSide.East, distance, new Coordinate(4, 4 - distance));
            yield return new TestCaseData(GridSide.South, distance, new Coordinate(4 - distance, 0));
            yield return new TestCaseData(GridSide.West, distance, new Coordinate(0, distance));
        }
    }
}