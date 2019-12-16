using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.FeatureProviders.Boundaries.Helpers
{
    [TestFixture]
    public class WaveBoundaryFactoryHelperTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var factoryHelper = new WaveBoundaryFactoryHelper();

            // Assert
            Assert.That(factoryHelper, Is.InstanceOf(typeof(IWaveBoundaryFactoryHelper)));
        }

        private static IEnumerable<TestCaseData> TestCaseDataSmallerThanTwoDistinctCoordinates
        {
            get
            {
                yield return new TestCaseData(Enumerable.Empty<Coordinate>());

                var coord = new Coordinate(0.0, 0.0);
                yield return new TestCaseData(new List<Coordinate> { coord });
                yield return new TestCaseData(new List<Coordinate> { coord, coord });
                yield return new TestCaseData(new List<Coordinate> { coord, coord, coord });
                yield return new TestCaseData(
                    new List<Coordinate>
                    {
                        new Coordinate(5.0, 5.0),
                        new Coordinate(5.0, 5.0),
                    });
            }
        }

        [Test]
        [TestCaseSource(nameof(TestCaseDataSmallerThanTwoDistinctCoordinates))]
        public void GivenABoundarySnappingCalculatorAndASetContainingLessThanTwoDistinctCoordinates_WhenGetSnappedEndPointsIsCalled_ThenAnArgumentExceptionIsThrown(IEnumerable<Coordinate> coordinates)
        {
            // Given
            var factoryHelper = new WaveBoundaryFactoryHelper();
            var calculator = Substitute.For<IBoundarySnappingCalculator>();

            // When
            void Call() => factoryHelper.GetSnappedEndPoints(calculator, coordinates);

            // Then
            var exception = 
                Assert.Throws<ArgumentException>(Call, $"Expected {nameof(WaveBoundaryFactoryHelper.GetSnappedEndPoints)} to throw an {nameof(ArgumentException)}");
            Assert.That(exception.Message, Is.EqualTo("There should be two or more distinct coordinates in coordinates."), 
                        "Expected a different message:");
        }

        private static IEnumerable<TestCaseData> TestCaseDataGetSnappedEndpoints
        {
            get
            {
                var firstCoord = new Coordinate(0.0, 0.0);
                var lastCoord = new Coordinate(0.0, 5.0);

                var minimalSetCoordinates = new List<Coordinate>
                {
                    firstCoord,
                    lastCoord,
                };

                yield return new TestCaseData(minimalSetCoordinates);

                var extraSetCoordinates = new List<Coordinate>
                {
                    firstCoord,
                    new Coordinate(1.0, 0.0),
                    new Coordinate(2.0, 0.0),
                    new Coordinate(3.0, 0.0),
                    new Coordinate(4.0, 0.0),
                    lastCoord,
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
                    new Coordinate(0.0, 5.0),
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
                    lastCoord,
                };

                yield return new TestCaseData(extraSetCoordinatesNonDistinct);
            }
        }

        [Test]
        [TestCaseSource(nameof(TestCaseDataGetSnappedEndpoints))]
        public void GivenABoundarySnappingCalculatorAndASetOfCoordinates_WhenGetSnappedEndPointsIsCalled_ThenTheFirstAndLastEndPointAreSnappedAndTheConcatenatedResultIsReturned(IEnumerable<Coordinate> coordinates)
        {
            // Given
            var coordinateComparer = new Coordinate2DEqualityComparer();

            Coordinate[] coordinatesArray = coordinates.ToArray();

            var factoryHelper = new WaveBoundaryFactoryHelper();
            var calculator = Substitute.For<IBoundarySnappingCalculator>();

            var firstGridCoordinates = new[]
            {
                new GridBoundaryCoordinate(GridSide.East, 0),
                new GridBoundaryCoordinate(GridSide.North, 10)
            };

            var lastGridCoordinates = new[]
            {
                new GridBoundaryCoordinate(GridSide.East, 10),
                new GridBoundaryCoordinate(GridSide.South, 0),
            };

            calculator.SnapCoordinateToGridBoundaryCoordinate(coordinatesArray.First())
                      .Returns(firstGridCoordinates);
            calculator.SnapCoordinateToGridBoundaryCoordinate(coordinatesArray.Last())
                      .Returns(lastGridCoordinates);

            // When
            IEnumerable<GridBoundaryCoordinate> result = factoryHelper.GetSnappedEndPoints(calculator, coordinatesArray);

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

        private static IEnumerable<TestCaseData> TestCaseDataGetGeometricDefinitionNotNull
        {
            get
            {
                const int indexFirst = 0;
                const int indexLast = 10;
                var coordinateFirst = new GridBoundaryCoordinate(GridSide.East, indexFirst);
                var coordinateLast  = new GridBoundaryCoordinate(GridSide.East, indexLast);

                GridBoundaryCoordinate[] coordinatesValid = new[]
                {
                    coordinateFirst,
                    coordinateLast
                };

                yield return new TestCaseData(coordinatesValid, 
                                              new WaveBoundaryGeometricDefinition(indexFirst, 
                                                                                  indexLast, 
                                                                                  GridSide.East));

                GridBoundaryCoordinate[] coordinatesValidExtra = new[]
                {
                    coordinateFirst,
                    new GridBoundaryCoordinate(GridSide.East, 5), 
                    coordinateLast,
                    new GridBoundaryCoordinate(GridSide.East, 7), 
                    new GridBoundaryCoordinate(GridSide.East, 0), 
                };

                yield return new TestCaseData(coordinatesValidExtra, 
                                              new WaveBoundaryGeometricDefinition(indexFirst, 
                                                                                  indexLast, 
                                                                                  GridSide.East));

                var coordinateFirstSmall = new GridBoundaryCoordinate(GridSide.West, 0);
                var coordinateLastSmall  = new GridBoundaryCoordinate(GridSide.West, 3);

                GridBoundaryCoordinate[] coordinatesValidExtraDifferentSide = new[]
                {
                    coordinateFirstSmall,
                    coordinateFirst,
                    coordinateLast,
                    coordinateLastSmall,
                };

                yield return new TestCaseData(coordinatesValidExtraDifferentSide, 
                                              new WaveBoundaryGeometricDefinition(indexFirst, 
                                                                                  indexLast, 
                                                                                  GridSide.East));
            }
        }

        [Test]
        [TestCaseSource(nameof(TestCaseDataGetGeometricDefinitionNotNull))]
        public void GivenAValidEnumerableOfGridBoundaryCoordinates_WhenGetGeometricDefinitionIsCalled_ThenTheExpectedCandidateIsReturned(IEnumerable<GridBoundaryCoordinate> coordinates, 
                                                                                                                                     IWaveBoundaryGeometricDefinition expectedDefinition)
        {
            // Given
            var factoryHelper = new WaveBoundaryFactoryHelper();

            // When
            IWaveBoundaryGeometricDefinition result = 
                factoryHelper.GetGeometricDefinition(coordinates);

            // Then
            Assert.That(result.StartingIndex, Is.EqualTo(expectedDefinition.StartingIndex),
                        "Expected a different starting index:");
            Assert.That(result.EndingIndex, Is.EqualTo(expectedDefinition.EndingIndex),
                        "Expected a different ending index:");
            Assert.That(result.GridSide, Is.EqualTo(expectedDefinition.GridSide),
                        "Expected a different grid side:");
        }

        private static IEnumerable<TestCaseData> TestCaseDataGetGeometricDefinitionNull
        {
            get
            {

                yield return new TestCaseData(Enumerable.Empty<GridBoundaryCoordinate>());

                yield return new TestCaseData(new List<GridBoundaryCoordinate>
                    {
                        new GridBoundaryCoordinate(GridSide.East, 0)
                    });

                const int indexFirst = 0;
                var coordinate = new GridBoundaryCoordinate(GridSide.East, indexFirst);

                var coordinatesSame = new List<GridBoundaryCoordinate>
                {
                    coordinate,
                    coordinate
                };

                yield return new TestCaseData(coordinatesSame);

                var coordinatesEqual = new List<GridBoundaryCoordinate>
                {
                    new GridBoundaryCoordinate(GridSide.East, 5),
                    new GridBoundaryCoordinate(GridSide.East, 5),
                };

                yield return new TestCaseData(coordinatesEqual);
            }
        }


        [Test]
        [TestCaseSource(nameof(TestCaseDataGetGeometricDefinitionNull))]
        public void GivenAnInvalidEnumerableOfGridBoundaryCoordinates_WhenGetGeometricDefinitionIsCalled_ThenNullIsReturned(IEnumerable<GridBoundaryCoordinate> coordinates)
        {
            // Given
            var factoryHelper = new WaveBoundaryFactoryHelper();

            // When
            IWaveBoundaryGeometricDefinition result =
                factoryHelper.GetGeometricDefinition(coordinates);

            // Then
            Assert.That(result, Is.Null);
        }
    }
}