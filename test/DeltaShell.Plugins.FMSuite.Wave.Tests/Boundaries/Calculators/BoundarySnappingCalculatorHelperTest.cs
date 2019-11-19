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
        private readonly Random random = new Random();

        [Test]
        [TestCaseSource(nameof(CoordinateTestData))]
        public void IsDefined_ValidCoordinate_ReturnsExpectedResult(Coordinate coordinate, bool expectedResult)
        {
            // Call
            var result = coordinate.IsDefined();

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private IEnumerable<TestCaseData> CoordinateTestData
        {
            get
            {
                yield return new TestCaseData(new Coordinate(random.NextDouble(), random.NextDouble()), true);

                var invalidValues = new[]
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

        [Test]
        public void GivenAnEmptySetOfCoordinates_WhenFindClosestIndicesIsCalled_ThenAnEmptySetAndPosInfIsReturned()
        {
            // Setup
            var distanceCalculator = Substitute.For<IDistanceCalculator>();
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
            var distanceCalculator = Substitute.For<IDistanceCalculator>();
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

            SetUpDistances(distanceCalculator, coordinateRef, coordinates, expectedDistances);

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
            var distanceCalculator = Substitute.For<IDistanceCalculator>();
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

            SetUpDistances(distanceCalculator, coordinateRef, coordinates, expectedDistances);

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


        private IList<Coordinate> GetRandomCoordinates(int numberOfCoordinates)
        {
            var result = new List<Coordinate>();

            for (var i = 0; i < numberOfCoordinates; i++)
            {
                result.Add(new Coordinate(random.NextDouble(), random.NextDouble()));
            }

            return result;
        }

        private void SetUpDistances(IDistanceCalculator distanceCalculator,
                                    Coordinate coordinateRef,
                                    IList<Coordinate> coordinates,
                                    IList<double> distances)
        {
            for (int i = 0; i < coordinates.Count; i++)
            {
                distanceCalculator.CalculateDistance(coordinateRef, coordinates[i]).Returns(distances[i]);
                distanceCalculator.CalculateDistance(coordinates[i], coordinateRef).Returns(distances[i]);
            }
        }
    }
}