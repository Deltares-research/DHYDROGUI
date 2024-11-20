using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Calculators
{
    [TestFixture]
    public class CartesianDistanceCalculatorTest
    {
        private static IEnumerable<TestCaseData> SquaredTestData => new[]
        {
            new TestCaseData(new Coordinate(0.0, 0.0), new Coordinate(0.0, 0.0), 0.0),
            new TestCaseData(new Coordinate(5.0, 0.0), new Coordinate(0.0, 0.0), 25.0),
            new TestCaseData(new Coordinate(0.0, 4.0), new Coordinate(0.0, 0.0), 16.0),
            new TestCaseData(new Coordinate(0.0, 0.0), new Coordinate(3.0, 0.0), 9.0),
            new TestCaseData(new Coordinate(0.0, 0.0), new Coordinate(0.0, 2.0), 4.0),
            new TestCaseData(new Coordinate(-5.0, 0.0), new Coordinate(0.0, 0.0), 25.0),
            new TestCaseData(new Coordinate(0.0, -4.0), new Coordinate(0.0, 0.0), 16.0),
            new TestCaseData(new Coordinate(0.0, 0.0), new Coordinate(-3.0, 0.0), 9.0),
            new TestCaseData(new Coordinate(0.0, 0.0), new Coordinate(0.0, -2.0), 4.0),
            new TestCaseData(new Coordinate(5.0, 4.0), new Coordinate(0.0, 0.0), 41.0),
            new TestCaseData(new Coordinate(0.0, 0.0), new Coordinate(10.0, 10.0), 200.0),
            new TestCaseData(new Coordinate(3.0, 2.0), new Coordinate(5.0, 5.0), 13.0),
            new TestCaseData(new Coordinate(-3.0, -2.0), new Coordinate(5.0, 5.0), 113.0),
            new TestCaseData(new Coordinate(3.0, 2.0), new Coordinate(-5.0, -5.0), 113.0)
        };

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var distanceCalculator = new CartesianDistanceCalculator();

            // Assert
            Assert.That(distanceCalculator, Is.InstanceOf<IDistanceCalculator>());
        }

        [Test]
        [TestCaseSource(nameof(SquaredTestData))]
        public void CalculateDistanceSquared_ValidCoordinates_ReturnsExpectedResults(Coordinate coordinateA,
                                                                                     Coordinate coordinateB,
                                                                                     double expectedValue)
        {
            // Setup
            var distanceCalculator = new CartesianDistanceCalculator();

            // Call
            double result = distanceCalculator.CalculateDistanceSquared(coordinateA,
                                                                        coordinateB);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CalculateDistanceSquared_CoordinateANull_ThrowsArgumentNullException()
        {
            // Setup
            var distanceCalculator = new CartesianDistanceCalculator();

            // Call
            void Call() => distanceCalculator.CalculateDistanceSquared(null, new Coordinate(1.0, 1.0));

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("coordinateA"));
        }

        [Test]
        public void CalculateDistanceSquared_CoordinateBNull_ThrowsArgumentNullException()
        {
            // Setup
            var distanceCalculator = new CartesianDistanceCalculator();

            // Call
            void Call() => distanceCalculator.CalculateDistanceSquared(new Coordinate(1.0, 1.0), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("coordinateB"));
        }

        [Test]
        [TestCaseSource(nameof(SquaredTestData))]
        public void CalculateDistance_ValidCoordinates_ReturnsExpectedResults(Coordinate coordinateA,
                                                                              Coordinate coordinateB,
                                                                              double expectedSquaredValue)
        {
            // Setup
            var distanceCalculator = new CartesianDistanceCalculator();
            double expectedValue = Math.Sqrt(expectedSquaredValue);

            // Call
            double result = distanceCalculator.CalculateDistance(coordinateA,
                                                                 coordinateB);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void CalculateDistance_CoordinateANull_ThrowsArgumentNullException()
        {
            // Setup
            var distanceCalculator = new CartesianDistanceCalculator();

            // Call
            void Call() => distanceCalculator.CalculateDistance(null, new Coordinate(1.0, 1.0));

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("coordinateA"));
        }

        [Test]
        public void CalculateDistance_CoordinateBNull_ThrowsArgumentNullException()
        {
            // Setup
            var distanceCalculator = new CartesianDistanceCalculator();

            // Call
            void Call() => distanceCalculator.CalculateDistance(new Coordinate(1.0, 1.0), null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception, Has.Property("ParamName").EqualTo("coordinateB"));
        }
    }
}