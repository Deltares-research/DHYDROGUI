using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;
using NetTopologySuite.Utilities;
using NSubstitute;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridBoundaryTest
    {
        private const int gridSizeX = 10;
        private const int gridSizeY = 12;

        [Test]
        [TestCaseSource(nameof(GetConstructorTestCaseData))]
        public void Constructor_ExpectedValues(IDiscreteGridPointCoverage grid)
        {
            // Call
            const int expectedX = gridSizeX;
            const int expectedY = gridSizeY;

            var gridBoundary = new GridBoundary(grid);

            // Assert
            Coordinate[] eastCoordinates = Enumerable.Range(0, expectedY).Select(j => new Coordinate(expectedX - 1, j)).ToArray();
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.East, expectedY, eastCoordinates);

            Coordinate[] northCoordinates = Enumerable.Range(0, expectedX).Select(i => new Coordinate(expectedX - 1 - i, expectedY - 1)).ToArray();
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.North, expectedX, northCoordinates);

            Coordinate[] westCoordinates = Enumerable.Range(0, expectedY).Select(j => new Coordinate(0, expectedY - 1 - j)).ToArray();
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.West, expectedY, westCoordinates);

            Coordinate[] southCoordinates = Enumerable.Range(0, expectedX).Select(i => new Coordinate(i, 0)).ToArray();
            AssertGridBoundarySideHasExpectedValues(gridBoundary, GridSide.South, expectedX, southCoordinates);
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

            GridBoundary gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(expectedX, expectedY);

            GridSide[] expectedSides =
            {
                GridSide.East,
                GridSide.North,
                GridSide.West,
                GridSide.South
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

            GridBoundary gridBoundary = GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(expectedX, expectedY);

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
            const int expectedX = 6;
            const int expectedY = 5;

            GridBoundary gridBoundary =
                GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(expectedX,
                                                                     expectedY,
                                                                     out IDiscreteGridPointCoverage grid);
            var gridBoundaryCoordinate = new GridBoundaryCoordinate(GridSide.North, 0);

            // Call
            Coordinate result = gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinate);

            // Assert
            Assert.That(result, Is.Not.Null, "Expected the result not to be null.");
            Assert.That(result.X, Is.EqualTo(grid.X.Values[5, 4]), "Expected a different Coordinate.X value:");
            Assert.That(result.Y, Is.EqualTo(grid.Y.Values[5, 4]), "Expected a different Coordinate.Y value:");
        }

        [Test]
        public void GetSideAlignedWithNormal_ReferenceNormalNull_ThrowsArgumentNullException()
        {
            // Setup
            GridBoundary gridBoundary =
                GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(3,
                                                                     3,
                                                                     out IDiscreteGridPointCoverage _);

            // Call | Assert
            void Call() => gridBoundary.GetSideAlignedWithNormal(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("referenceNormal"));
        }

        [Test]
        [TestCaseSource(nameof(GetSideAlignedWithNormalData))]
        public void GetSideAlignedWithNormal_ExpectedResults(Vector2D referenceNormal, GridSide expectedResult)
        {
            // Setup
            const int expectedX = 6;
            const int expectedY = 5;

            GridBoundary gridBoundary =
                GridBoundaryTestHelper.GetGridBoundaryWithMockedGrid(expectedX,
                                                                     expectedY,
                                                                     out IDiscreteGridPointCoverage _);

            // Call
            GridSide result = gridBoundary.GetSideAlignedWithNormal(referenceNormal);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IDiscreteGridPointCoverage GetRegularGrid()
        {
            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(gridSizeX);
            grid.Size2.Returns(gridSizeY);

            for (var j = 0; j < gridSizeY; j++)
            {
                for (var i = 0; i < gridSizeX; i++)
                {
                    grid.X.Values[i, j] = i;
                    grid.Y.Values[i, j] = j;
                }
            }

            return grid;
        }

        private static IDiscreteGridPointCoverage GetRegularGridInvertedX()
        {
            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(gridSizeX);
            grid.Size2.Returns(gridSizeY);

            for (var j = 0; j < gridSizeY; j++)
            {
                for (var i = 0; i < gridSizeX; i++)
                {
                    grid.X.Values[i, j] = gridSizeX - 1 - i;
                    grid.Y.Values[i, j] = j;
                }
            }

            return grid;
        }

        private static IDiscreteGridPointCoverage GetRegularGridInvertedY()
        {
            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(gridSizeX);
            grid.Size2.Returns(gridSizeY);

            for (var j = 0; j < gridSizeY; j++)
            {
                for (var i = 0; i < gridSizeX; i++)
                {
                    grid.X.Values[i, j] = i;
                    grid.Y.Values[i, j] = gridSizeY - 1 - j;
                }
            }

            return grid;
        }

        private static IDiscreteGridPointCoverage GetRegularGridInvertedXY()
        {
            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.Size1.Returns(gridSizeX);
            grid.Size2.Returns(gridSizeY);

            for (var j = 0; j < gridSizeY; j++)
            {
                for (var i = 0; i < gridSizeX; i++)
                {
                    grid.X.Values[i, j] = gridSizeX - 1 - i;
                    grid.Y.Values[i, j] = gridSizeY - 1 - j;
                }
            }

            return grid;
        }

        private static IEnumerable<TestCaseData> GetConstructorTestCaseData()
        {
            yield return new TestCaseData(GetRegularGrid());
            yield return new TestCaseData(GetRegularGridInvertedX());
            yield return new TestCaseData(GetRegularGridInvertedY());
            yield return new TestCaseData(GetRegularGridInvertedXY());
        }

        private static void AssertGridBoundarySideHasExpectedValues(GridBoundary gridBoundary,
                                                                    GridSide side,
                                                                    int expectedNumberOfValues,
                                                                    Coordinate[] expectedWorldCoordinates)
        {
            List<GridBoundaryCoordinate> gridBoundaryValues = gridBoundary[side].ToList();
            Assert.That(gridBoundaryValues, Has.Count.EqualTo(expectedNumberOfValues),
                        $"Expected the {side} boundary to have {expectedNumberOfValues} values");

            for (var i = 0; i < expectedNumberOfValues; i++)
            {
                Assert.That(gridBoundaryValues[i].GridSide, Is.EqualTo(side));
                Assert.That(gridBoundaryValues[i].Index, Is.EqualTo(i));
                Assert.That(gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryValues[i]),
                            Is.EqualTo(expectedWorldCoordinates[i]));
            }
        }

        private static IEnumerable<TestCaseData> GetSideAlignedWithNormalData()
        {
            var referenceNormal = Vector2D.Create(1.0, 0.0);

            var sideConversion = new[]
            {
                GridSide.East,
                GridSide.North,
                GridSide.West,
                GridSide.South
            };

            Vector2D GetNormal(int rot) => referenceNormal.Rotate(Degrees.ToRadians(rot));

            // Create vectors starting at -40 until 310 degrees.
            return Enumerable.Range(0, 36).Select(i => new TestCaseData(GetNormal((i * 10) - 40),
                                                                        sideConversion[i / 9]));
        }
    }
}