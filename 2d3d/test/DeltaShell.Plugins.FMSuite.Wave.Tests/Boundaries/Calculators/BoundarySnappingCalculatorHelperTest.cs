using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Calculators
{
    [TestFixture]
    public class BoundarySnappingCalculatorHelperTest
    {
        private const double tolerance = 1E-10;
        private static readonly Random random = new Random();
        private IDistanceCalculator distanceCalculator;

        private static IEnumerable<TestCaseData> CoordinateTestData
        {
            get
            {
                yield return new TestCaseData(new Coordinate(random.NextDouble(), random.NextDouble()), true);

                double[] invalidValues = new[]
                {
                    double.NaN,
                    double.PositiveInfinity,
                    double.NegativeInfinity
                };

                foreach (double val in invalidValues)
                {
                    yield return new TestCaseData(new Coordinate(random.NextDouble(), val), false);
                    yield return new TestCaseData(new Coordinate(val, random.NextDouble()), false);
                }
            }
        }

        [SetUp]
        public void Setup()
        {
            distanceCalculator = Substitute.For<IDistanceCalculator>();
        }

        [Test]
        [TestCaseSource(nameof(CoordinateTestData))]
        public void IsDefined_ValidCoordinate_ReturnsExpectedResult(Coordinate coordinate, bool expectedResult)
        {
            // Call
            bool result = coordinate.IsDefined();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GivenAnEmptySetOfCoordinates_WhenFindClosestIndicesIsCalled_ThenAnEmptySetAndPosInfIsReturned()
        {
            // Setup
            var coordinates = new List<Coordinate>();
            var coordinateRef = new Coordinate(0.0, 0.0);

            // Call
            Tuple<IEnumerable<int>, double> result =
                BoundarySnappingCalculatorHelper.FindClosestIndices(distanceCalculator,
                                                                    coordinateRef,
                                                                    coordinates);

            // Assert
            Assert.That(result.Item2, Is.EqualTo(double.PositiveInfinity),
                        "Expected the distance result to be different:");
            Assert.That(result.Item1, Is.Not.Null,
                        "Expected the returned items not to be null.");
            List<int> returnedItems = result.Item1.ToList();
            Assert.That(returnedItems, Is.Empty,
                        "Expected no values to be returned.");
        }

        [Test]
        public void GivenASetOfCoordinatesAndASourceCoordinate_WhenFindClosestIndicesIsCalled_ThenTheClosestIndexIsReturned()
        {
            // Setup
            var coordinateRef = new Coordinate(0.0, 0.0);

            IList<Coordinate> coordinates = GetRandomCoordinates(5);

            const double smallestValue = 0.5;
            var expectedDistances = new List<double>()
            {
                1.0,
                2.0,
                3.0,
                smallestValue,
                1.0
            };

            SetUpDistances(coordinateRef, coordinates, expectedDistances);

            // Call
            Tuple<IEnumerable<int>, double> result =
                BoundarySnappingCalculatorHelper.FindClosestIndices(distanceCalculator,
                                                                    coordinateRef,
                                                                    coordinates);

            // Assert
            Assert.That(result.Item2, Is.EqualTo(smallestValue),
                        "Expected the distance result to be different:");
            Assert.That(result.Item1, Is.Not.Null,
                        "Expected the returned items not to be null.");
            List<int> returnedItems = result.Item1.ToList();
            Assert.That(returnedItems, Has.Count.EqualTo(1),
                        "Expected a single value to be returned.");
            Assert.That(returnedItems.First(), Is.EqualTo(3),
                        "Expected the returned index to be different:");
        }

        [Test]
        public void GivenASetOfCoordinatesContainingEqualDistancesToASourceCoordinate_WhenFindClosestIndicesIsCalled_ThenTheClosestSetOfCoordinatesIsReturned()
        {
            // Setup
            var coordinateRef = new Coordinate(0.0, 0.0);

            IList<Coordinate> coordinates = GetRandomCoordinates(6);

            const double smallestValue = 0.5;
            var expectedDistances = new List<double>()
            {
                1.0,
                smallestValue,
                2.0,
                3.0,
                smallestValue,
                1.0
            };

            SetUpDistances(coordinateRef, coordinates, expectedDistances);

            // Call
            Tuple<IEnumerable<int>, double> result =
                BoundarySnappingCalculatorHelper.FindClosestIndices(distanceCalculator,
                                                                    coordinateRef,
                                                                    coordinates);

            // Assert
            Assert.That(result.Item2, Is.EqualTo(smallestValue),
                        "Expected the distance result to be different:");
            Assert.That(result.Item1, Is.Not.Null,
                        "Expected the returned items not to be null.");
            List<int> returnedItems = result.Item1.ToList();
            Assert.That(returnedItems, Has.Count.EqualTo(2),
                        "Expected a single value to be returned.");
            Assert.That(returnedItems[0], Is.EqualTo(1),
                        "Expected the first returned index to be different:");
            Assert.That(returnedItems[1], Is.EqualTo(4),
                        "Expected the second returned index to be different:");
        }

        [Test]
        public void GivenASetOfCoordinatesAndADistanceEqualToTheDistancesBetweenTheCoordinates_WhenCalculateCoordinateFromDistanceIsCalled_ThenCorrectResultIsReturned()
        {
            // Setup
            int index = random.Next(9);
            double distanceBetweenCoordinates = random.NextDouble();
            double distance = distanceBetweenCoordinates * index;

            Coordinate[] coordinates = GetRandomCoordinates(10).ToArray();
            SetUpDistancesBetweenCoordinates(coordinates, distanceBetweenCoordinates);

            // Call
            Coordinate result = BoundarySnappingCalculatorHelper.CalculateCoordinateFromDistance(distance,
                                                                                                 coordinates,
                                                                                                 distanceCalculator);

            // Assert
            Assert.That(result.Equals2D(coordinates[index], tolerance), $"Expected: {coordinates[index]} \n" +
                                                                        $"But was:  {result}.");
        }

        [Test]
        public void GivenASetOfCoordinatesAndAValidDistance_WhenCalculateCoordinateFromDistanceIsCalled_ThenCorrectResultIsReturned()
        {
            // Setup
            int index = random.Next(9);
            double distanceBetweenCoordinates = random.NextDouble();
            double distanceFromLastCoordinate = distanceBetweenCoordinates * random.NextDouble();
            double distance = (distanceBetweenCoordinates * index) + distanceFromLastCoordinate;

            Coordinate[] coordinates = GetRandomCoordinates(10).ToArray();
            SetUpDistancesBetweenCoordinates(coordinates, distanceBetweenCoordinates);

            // Call
            Coordinate result = BoundarySnappingCalculatorHelper.CalculateCoordinateFromDistance(distance,
                                                                                                 coordinates,
                                                                                                 distanceCalculator);

            // Assert
            Coordinate expectedCoordinate = GetExpectedCoordinate(distanceBetweenCoordinates,
                                                                  distanceFromLastCoordinate,
                                                                  coordinates,
                                                                  index);

            Assert.That(result.Equals2D(expectedCoordinate, tolerance), $"Expected: {expectedCoordinate} \n" +
                                                                        $"But was:  {result}.");
        }

        [Test]
        public void GivenASetOfCoordinatesAndADistanceThatExceedsTheTotalDistance_WhenCalculateCoordinateFromDistanceIsCalled_ThenInvalidOperationExceptionIsThrown()
        {
            // Setup
            double distanceBetweenCoordinates = random.NextDouble();
            double distance = distanceBetweenCoordinates * 10;

            Coordinate[] coordinates = GetRandomCoordinates(10).ToArray();
            SetUpDistancesBetweenCoordinates(coordinates, distanceBetweenCoordinates);

            // Call
            void Call() => BoundarySnappingCalculatorHelper.CalculateCoordinateFromDistance(distance,
                                                                                            coordinates,
                                                                                            distanceCalculator);

            // Assert
            Assert.That(Call, Throws.TypeOf<InvalidOperationException>()
                                    .With.Message.EqualTo("Distance exceeds total distance between coordinates."));
        }

        private IList<Coordinate> GetRandomCoordinates(int numberOfCoordinates)
        {
            var result = new List<Coordinate>();

            for (var i = 0; i < numberOfCoordinates; i++)
            {
                result.Add(new Coordinate(random.NextDouble(), random.NextDouble()));
            }

            return result;
        }

        private void SetUpDistances(Coordinate coordinateRef,
                                    IList<Coordinate> coordinates,
                                    IList<double> distances)
        {
            for (var i = 0; i < coordinates.Count; i++)
            {
                distanceCalculator.CalculateDistance(coordinateRef, coordinates[i]).Returns(distances[i]);
                distanceCalculator.CalculateDistance(coordinates[i], coordinateRef).Returns(distances[i]);
            }
        }

        private void SetUpDistancesBetweenCoordinates(Coordinate[] coordinates,
                                                      double distance)
        {
            for (var i = 0; i < coordinates.Length - 1; i++)
            {
                distanceCalculator.CalculateDistance(coordinates[i], coordinates[i + 1]).Returns(distance);
            }
        }

        private static Coordinate GetExpectedCoordinate(double distanceBetweenCoordinates,
                                                        double distanceFromLastCoordinate,
                                                        Coordinate[] coordinates,
                                                        int index)
        {
            double GetExpectedValue(double start, double end)
            {
                double difference = end - start;
                double normalized = difference / distanceBetweenCoordinates;
                return start + (normalized * distanceFromLastCoordinate);
            }

            double expectedX = GetExpectedValue(coordinates[index].X, coordinates[index + 1].X);
            double expectedY = GetExpectedValue(coordinates[index].Y, coordinates[index + 1].Y);

            return new Coordinate(expectedX, expectedY);
        }
    }
}