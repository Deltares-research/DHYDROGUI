using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Calculators
{
    [TestFixture]
    public class BoundarySnappingCalculatorTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            IDiscreteGridPointCoverage grid = GridBoundaryTestHelper.GetValidGridMock(2, 2);
            var gridBoundary = new GridBoundary(grid);

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
        public void SetGridBoundary_ValidGridBoundary_SetCorrectBoundary()
        {
            // Setup
            var calculator = new BoundarySnappingCalculator(new GridBoundary(GridBoundaryTestHelper.GetValidGridMock(2, 2)));
            var newGridBoundary = new GridBoundary(GridBoundaryTestHelper.GetValidGridMock(2, 2));

            // Call
            calculator.GridBoundary = newGridBoundary;

            // Assert
            Assert.That(calculator.GridBoundary, Is.SameAs(newGridBoundary));
        }

        [Test]
        public void SetGridBoundary_Null_ThrowsArgumentNullException()
        {
            // Setup
            var grid = GridBoundaryTestHelper.GetValidGridMock(2, 2);
            var gridBoundary = new GridBoundary(grid);
            var calculator = new BoundarySnappingCalculator(gridBoundary);

            // Call
            void Call() => calculator.GridBoundary = null;

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("value"));
        }

        [Test]
        public void SnapCoordinateToGridBoundaryCoordinate_CoordinateToSnapNull_ThrowsArgumentNullException()
        {
            // Setup 
            IDiscreteGridPointCoverage grid = GridBoundaryTestHelper.GetValidGridMock(2, 2);
            var gridBoundary = new GridBoundary(grid);
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
            IDiscreteGridPointCoverage grid = GridBoundaryTestHelper.GetValidGridMock(2, 2);
            var gridBoundary = new GridBoundary(grid);
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
            IEnumerable<GridBoundaryCoordinate> result = calculator.SnapCoordinateToGridBoundaryCoordinate(coordinateRef, 10.0);

            // Assert
            var expectedResult = new List<GridBoundaryCoordinate>()
            {
                new GridBoundaryCoordinate(GridSide.East, 0),
                new GridBoundaryCoordinate(GridSide.North, 4),
            };

            Assert.That(result, Is.EquivalentTo(expectedResult));
        }
    }
}