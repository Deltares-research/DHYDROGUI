using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridBoundaryTest
    {
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
            Assert.That(gridBoundary[GridSide.West], Has.Count.EqualTo(expectedY), 
                        $"Expected the west boundary to have {expectedY} values");
            for (var i = 0; i < gridBoundary[GridSide.West].Count; i++)
            {
                AssertExpectedCoordinate(gridBoundary[GridSide.West][i], 0, i);
            }

            Assert.That(gridBoundary[GridSide.North], Has.Count.EqualTo(expectedX), 
                        $"Expected the west boundary to have {expectedX} values");
            for (var i = 0; i < gridBoundary[GridSide.North].Count; i++)
            {
                AssertExpectedCoordinate(gridBoundary[GridSide.North][i], i, expectedY - 1);
            }

            Assert.That(gridBoundary[GridSide.East], Has.Count.EqualTo(expectedY), 
                        $"Expected the west boundary to have {expectedY} values");
            for (var i = 0; i < gridBoundary[GridSide.East].Count; i++)
            {
                AssertExpectedCoordinate(gridBoundary[GridSide.East][i], expectedX - 1, expectedY - 1 - i);
            }

            Assert.That(gridBoundary[GridSide.South], Has.Count.EqualTo(expectedX), 
                        $"Expected the west boundary to have {expectedX} values");
            for (var i = 0; i < gridBoundary[GridSide.South].Count; i++)
            {
                AssertExpectedCoordinate(gridBoundary[GridSide.South][i], expectedX - 1 - i, 0);
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
        public void GivenAGridWithOverlappingBoundaries_WhenGetGridEnvelopeIsCalled_ThenTheCorrectEnvelopeIsReturned()
        {
            // Setup
            const int expectedX = 3;
            const int expectedY = 3;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(expectedX);
            grid.Size2.Returns(expectedY);

            var gridBoundary = new GridBoundary(grid);

            // Call
            IEnumerable<GridCoordinate> envelope = gridBoundary.GetGridEnvelope();

            // Assert
            Assert.That(envelope, Is.Not.Null, "Expected the grid envelope to be not null.");
            List<GridCoordinate> envelopeList = envelope.ToList();

            Assert.That(envelopeList, Has.Count.EqualTo(8), 
                        "Expected the envelope to consist of 8 elements");

            AssertExpectedCoordinate(envelopeList[0], 0, 0);
            AssertExpectedCoordinate(envelopeList[1], 0, 1);
            AssertExpectedCoordinate(envelopeList[2], 0, 2);
            AssertExpectedCoordinate(envelopeList[3], 1, 2);
            AssertExpectedCoordinate(envelopeList[4], 2, 2);
            AssertExpectedCoordinate(envelopeList[5], 2, 1);
            AssertExpectedCoordinate(envelopeList[6], 2, 0);
            AssertExpectedCoordinate(envelopeList[7], 1, 0);
        }

        [Test]
        public void GivenAGridWithoutOverlappingBoundaries_WhenGetGridEnvelopeIsCalled_ThenTheCorrectEnvelopeIsReturned()
        {
            // Setup
            const int expectedX = 4;
            const int expectedY = 4;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(expectedX);
            grid.Size2.Returns(expectedY);

            // Make the corners dry points.
            grid.X.Values[0, 0] = double.NaN;
            grid.X.Values[0, 3] = double.NaN;
            grid.X.Values[3, 3] = double.NaN;
            grid.X.Values[3, 0] = double.NaN;

            grid.Y.Values[0, 0] = double.NaN;
            grid.Y.Values[0, 3] = double.NaN;
            grid.Y.Values[3, 3] = double.NaN;
            grid.Y.Values[3, 0] = double.NaN;

            var gridBoundary = new GridBoundary(grid);

            // Call
            IEnumerable<GridCoordinate> envelope = gridBoundary.GetGridEnvelope();

            // Assert
            Assert.That(envelope, Is.Not.Null, "Expected the grid envelope to be not null.");
            List<GridCoordinate> envelopeList = envelope.ToList();

            Assert.That(envelopeList, Has.Count.EqualTo(8), 
                        "Expected the envelope to consist of 8 elements");

            AssertExpectedCoordinate(envelopeList[0], 0, 1);
            AssertExpectedCoordinate(envelopeList[1], 0, 2);
            AssertExpectedCoordinate(envelopeList[2], 1, 3);
            AssertExpectedCoordinate(envelopeList[3], 2, 3);
            AssertExpectedCoordinate(envelopeList[4], 3, 2);
            AssertExpectedCoordinate(envelopeList[5], 3, 1);
            AssertExpectedCoordinate(envelopeList[6], 2, 0);
            AssertExpectedCoordinate(envelopeList[7], 1, 0);
        }

        private static void AssertExpectedCoordinate(GridCoordinate coordinate, 
                                                     int expectedX, 
                                                     int expectedY)
        {
            Assert.That(coordinate.X, Is.EqualTo(expectedX),
                        "Expected X value of the coordinate to be different:");
            Assert.That(coordinate.Y, Is.EqualTo(expectedY), 
                        "Expected Y value of the coordinate to be different:");
        }
    }
}