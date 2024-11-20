using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class WaveBoundaryGeometricDefinitionFactoryHelperTest
    {
        private static IEnumerable<TestCaseData> TestCaseDataGetSnappedEndpoints
        {
            get
            {
                var firstCoord = new Coordinate(0.0, 0.0);
                var lastCoord = new Coordinate(0.0, 5.0);

                var minimalSetCoordinates = new List<Coordinate>
                {
                    firstCoord,
                    lastCoord
                };

                yield return new TestCaseData(minimalSetCoordinates);

                var extraSetCoordinates = new List<Coordinate>
                {
                    firstCoord,
                    new Coordinate(1.0, 0.0),
                    new Coordinate(2.0, 0.0),
                    new Coordinate(3.0, 0.0),
                    new Coordinate(4.0, 0.0),
                    lastCoord
                };

                yield return new TestCaseData(extraSetCoordinates);

                var nonDistinctSetCoordinates = new List<Coordinate>
                {
                    firstCoord,
                    firstCoord,
                    lastCoord
                };

                yield return new TestCaseData(nonDistinctSetCoordinates);

                var nonDistinctSetCoordinates2 = new List<Coordinate>
                {
                    firstCoord,
                    firstCoord,
                    lastCoord,
                    lastCoord,
                    lastCoord
                };

                yield return new TestCaseData(nonDistinctSetCoordinates2);

                var nonDistinctSetCoordinates3 = new List<Coordinate>
                {
                    firstCoord,
                    new Coordinate(0.0, 0.0),
                    new Coordinate(0.0, 0.0),
                    new Coordinate(0.0, 5.0),
                    lastCoord,
                    new Coordinate(0.0, 5.0)
                };

                yield return new TestCaseData(nonDistinctSetCoordinates3);

                var extraSetCoordinatesNonDistinct = new List<Coordinate>
                {
                    firstCoord,
                    firstCoord,
                    new Coordinate(1.0, 0.0),
                    new Coordinate(1.0, 0.0),
                    new Coordinate(2.0, 0.0),
                    new Coordinate(2.0, 0.0),
                    new Coordinate(3.0, 0.0),
                    new Coordinate(3.0, 0.0),
                    new Coordinate(4.0, 0.0),
                    new Coordinate(4.0, 0.0),
                    lastCoord,
                    lastCoord
                };

                yield return new TestCaseData(extraSetCoordinatesNonDistinct);
            }
        }

        private static IEnumerable<TestCaseData> TestCaseDataSmallerThanTwoDistinctCoordinates
        {
            get
            {
                yield return new TestCaseData(Enumerable.Empty<Coordinate>());

                var coord = new Coordinate(0.0, 0.0);
                yield return new TestCaseData(new List<Coordinate> {coord});
                yield return new TestCaseData(new List<Coordinate>
                {
                    coord,
                    coord
                });
                yield return new TestCaseData(new List<Coordinate>
                {
                    coord,
                    coord,
                    coord
                });
                yield return new TestCaseData(
                    new List<Coordinate>
                    {
                        new Coordinate(5.0, 5.0),
                        new Coordinate(5.0, 5.0)
                    });
            }
        }

        [Test]
        [TestCaseSource(nameof(TestCaseDataSmallerThanTwoDistinctCoordinates))]
        public void GetSnappedEndPoints_CoordinatesContainsLessThanTwoDistinctCoordinates_ThrowsArgumentException(
            IEnumerable<Coordinate> coordinates)
        {
            // Given
            var calculator = Substitute.For<IBoundarySnappingCalculator>();

            // When
            void Call() => WaveBoundaryGeometricDefinitionFactoryHelper.GetSnappedEndPoints(calculator, coordinates);

            // Then
            var exception = Assert.Throws<ArgumentException>(
                Call, $"Expected {nameof(WaveBoundaryFactoryHelper.GetSnappedEndPoints)} to throw an {nameof(ArgumentException)}");
            Assert.That(exception.Message, Is.EqualTo("There should be two or more distinct coordinates in coordinates."),
                        "Expected a different message:");
        }

        [Test]
        [TestCaseSource(nameof(TestCaseDataGetSnappedEndpoints))]
        public void GetSnappedEndPoints_ReturnsCorrectResult(IEnumerable<Coordinate> coordinates)
        {
            // Given
            var coordinateComparer = new Coordinate2DEqualityComparer();

            Coordinate[] coordinatesArray = coordinates.ToArray();

            var calculator = Substitute.For<IBoundarySnappingCalculator>();

            var firstGridCoordinates = new[]
            {
                new GridBoundaryCoordinate(GridSide.East, 0),
                new GridBoundaryCoordinate(GridSide.North, 10)
            };

            var lastGridCoordinates = new[]
            {
                new GridBoundaryCoordinate(GridSide.East, 10),
                new GridBoundaryCoordinate(GridSide.South, 0)
            };

            calculator.SnapCoordinateToGridBoundaryCoordinate(coordinatesArray.First())
                      .Returns(firstGridCoordinates);
            calculator.SnapCoordinateToGridBoundaryCoordinate(coordinatesArray.Last())
                      .Returns(lastGridCoordinates);

            // When
            IEnumerable<GridBoundaryCoordinate>
                result = WaveBoundaryGeometricDefinitionFactoryHelper.GetSnappedEndPoints(calculator, coordinatesArray);

            // Then
            calculator.Received(1).SnapCoordinateToGridBoundaryCoordinate(coordinatesArray.First());
            calculator.Received(1).SnapCoordinateToGridBoundaryCoordinate(coordinatesArray.Last());
            calculator.DidNotReceive().SnapCoordinateToGridBoundaryCoordinate(
                Arg.Is<Coordinate>(x => !coordinateComparer.Equals(x, coordinatesArray.First()) &&
                                        !coordinateComparer.Equals(x, coordinatesArray.Last())));

            IEnumerable<GridBoundaryCoordinate> expectedResult = lastGridCoordinates.Concat(firstGridCoordinates);
            Assert.That(result, Is.EquivalentTo(expectedResult),
                        $"Expected the result of {nameof(IWaveBoundaryFactoryHelper.GetSnappedEndPoints)} to be different:");
        }

        [Test]
        [TestCaseSource(nameof(GetGeometricDefinitionFromOrientationData))]
        public void GetGeometricDefinition_Orientation(BoundaryOrientationType orientation,
                                                       Vector2D normal)
        {
            // Setup
            const int startIndex = 0;
            const int endIndex = 10;
            const GridSide expectedSide = GridSide.South;
            const double expectedLength = 3.0;

            var startCoord = new GridBoundaryCoordinate(expectedSide, startIndex);
            var endCoord = new GridBoundaryCoordinate(expectedSide, endIndex);

            var calculator = Substitute.For<IBoundarySnappingCalculator>();
            calculator.GridBoundary[expectedSide].Returns(new[]
            {
                startCoord,
                endCoord
            });

            calculator.GridBoundary.GetSideAlignedWithNormal(Arg.Is<Vector2D>(n => Math.Abs(n.X - normal.X) < 0.0001 &&
                                                                                   Math.Abs(n.Y - normal.Y) < 0.0001))
                      .Returns(expectedSide);

            calculator.CalculateDistanceBetweenBoundaryIndices(startIndex, endIndex, expectedSide)
                      .Returns(expectedLength);

            // Call
            IWaveBoundaryGeometricDefinition geometricDefinition =
                WaveBoundaryGeometricDefinitionFactoryHelper.GetGeometricDefinition(orientation,
                                                                                    calculator);

            // Assert
            Assert.That(geometricDefinition.StartingIndex, Is.EqualTo(startIndex));
            Assert.That(geometricDefinition.EndingIndex, Is.EqualTo(endIndex));
            Assert.That(geometricDefinition.GridSide, Is.EqualTo(expectedSide));
            Assert.That(geometricDefinition.Length, Is.EqualTo(expectedLength));
        }

        private static IEnumerable<TestCaseData> GetGeometricDefinitionFromOrientationData()
        {
            yield return new TestCaseData(BoundaryOrientationType.East, Vector2D.Create(1.0, 0.0));
            yield return new TestCaseData(BoundaryOrientationType.NorthEast, Vector2D.Create(1.0, 1.0));
            yield return new TestCaseData(BoundaryOrientationType.North, Vector2D.Create(0.0, 1.0));
            yield return new TestCaseData(BoundaryOrientationType.NorthWest, Vector2D.Create(-1.0, 1.0));
            yield return new TestCaseData(BoundaryOrientationType.West, Vector2D.Create(-1.0, 0.0));
            yield return new TestCaseData(BoundaryOrientationType.SouthWest, Vector2D.Create(-1.0, -1.0));
            yield return new TestCaseData(BoundaryOrientationType.South, Vector2D.Create(0.0, -1.0));
            yield return new TestCaseData(BoundaryOrientationType.SouthEast, Vector2D.Create(1.0, -1.0));
        }
    }
}