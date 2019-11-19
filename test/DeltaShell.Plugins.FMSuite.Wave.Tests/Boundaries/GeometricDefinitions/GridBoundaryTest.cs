using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridBoundaryTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const int expectedX = 5;
            const int expectedY = 6;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(expectedX);
            grid.Size2.Returns(expectedY);

            // Call
            var gridBoundary = new GridBoundary(grid);

            // Assert
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.West, expectedY);
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.North, expectedX);
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.East, expectedY);
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.South, expectedX);
        }

        private static void AssertGridBoundarySideHasExpectedValues(GridBoundary gridBoundary,
                                                                    GridSide side,
                                                                    int expectedNumberOfValues)
        {
            List<GridBoundaryCoordinate> gridBoundaryValues = gridBoundary[side].ToList();
            Assert.That(gridBoundaryValues, Has.Count.EqualTo(expectedNumberOfValues),
                        $"Expected the {side} boundary to have {expectedNumberOfValues} values");

            for (var i = 0; i < expectedNumberOfValues; i++)
            {
                Assert.That(gridBoundaryValues[i].GridSide, Is.EqualTo(side));
                Assert.That(gridBoundaryValues[i].Index, Is.EqualTo(i));
            }
        }

        [Test]
        public void Constructor_GridNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new GridBoundary(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception,
                        Has.Property("ParamName").EqualTo("grid"));

        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(2, 1)]
        [TestCase(0, 2)]
        [TestCase(2, 0)]
        [TestCase(-1, 2)]
        [TestCase(2, -1)]
        public void Constructor_GridWithInvalidDimensions_ThrowsArgumentException(int xVal, int yVal)
        {
            // Setup
            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(xVal);
            grid.Size2.Returns(yVal);

            // Call
            void Call() => new GridBoundary(grid);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception,
                        Has.Message.EqualTo("grid should contain at least 2 points in each dimension."));
        }

        [Test]
        public void GivenAGrid_WhenGetGridEnvelopeIsCalled_ThenTheCorrectEnvelopeIsReturned()
        {
            // Setup
            const int expectedNumberOfElements = 12;

            const int expectedX = 3;
            const int expectedY = 3;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(expectedX);
            grid.Size2.Returns(expectedY);

            var gridBoundary = new GridBoundary(grid);

            GridSide[] expectedSides =
            {
                GridSide.West,
                GridSide.North,
                GridSide.East,
                GridSide.South,
            };

            // Call
            IEnumerable<GridBoundaryCoordinate> envelope = gridBoundary.GetGridEnvelope();

            // Assert
            Assert.That(envelope, Is.Not.Null, "Expected the grid envelope to be not null.");
            List<GridBoundaryCoordinate> envelopeList = envelope.ToList();

            Assert.That(envelopeList, Has.Count.EqualTo(expectedNumberOfElements),
                        $"Expected the envelope to consist of {expectedNumberOfElements} elements");

            for (var i = 0; i < expectedNumberOfElements; i++)
            {
                GridBoundaryCoordinate gridBoundaryCoordinate = envelopeList[i];

                Assert.That(gridBoundaryCoordinate.GridSide, Is.EqualTo(expectedSides[i / 3]));
                Assert.That(gridBoundaryCoordinate.Index, Is.EqualTo(i % 3));
            }
        }

        [Test]
        public void GetWorldCoordinateFromBoundaryCoordinate_BoundaryCoordinateNull_ThrowsArgumentNullException()
        {
            // Setup
            const int expectedX = 5;
            const int expectedY = 6;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(expectedX);
            grid.Size2.Returns(expectedY);

            var gridBoundary = new GridBoundary(grid);

            // Call
            void Call() => gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(null);
            
            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception,
                        Has.Property("ParamName").EqualTo("boundaryCoordinate"));
        }

        [Test]
        public void GetWorldCoordinateFromBoundaryCoordinate_BoundaryCoordinateValid_ReturnsCorrespondingWorldCoordinate()
        {
            // Setup
            var gridCoordinate = new GridCoordinate(5, 4);
            var expectedXWorldCoordinate = random.NextDouble();
            var expectedYWorldCoordinate = random.NextDouble();

            const int expectedX = 6;
            const int expectedY = 5;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(expectedX);
            grid.Size2.Returns(expectedY);

            grid.X.Values[gridCoordinate.X, gridCoordinate.Y].Returns(expectedXWorldCoordinate);
            grid.Y.Values[gridCoordinate.X, gridCoordinate.Y].Returns(expectedYWorldCoordinate);

            var gridBoundary = new GridBoundary(grid);

            var gridBoundaryCoordinate = new GridBoundaryCoordinate(GridSide.North, 5);

            // Call
            Coordinate result = gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinate);


            // Assert
            Assert.That(result, Is.Not.Null, "Expected the result not to be null.");
            Assert.That(result.X, Is.EqualTo(expectedXWorldCoordinate), "Expected a different Coordinate.X value:");
            Assert.That(result.Y, Is.EqualTo(expectedYWorldCoordinate), "Expected a different Coordinate.Y value:");
        }
    }
}