using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;
using NetTopologySuite.Utilities;
using NSubstitute;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Calculators
{
    [TestFixture]
    public class CartesianOrientationCalculatorHelperTest
    {
        private static readonly Random random = new Random();

        [Test]
        public void GetCoordinateAt_WithIndices_GridNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call()
            {
                CartesianOrientationCalculatorHelper.GetCoordinateAt(null, 0, 0);
            }

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("grid"));
        }

        [Test]
        public void GetCoordinateAt_WithGridCoordinate_GridNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call()
            {
                CartesianOrientationCalculatorHelper.GetCoordinateAt(null, new GridCoordinate(0, 0));
            }

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("grid"));
        }

        [Test]
        public void GetCoordinateAt_CoordinateNull_ThrowsArgumentNullException()
        {
            // Setup
            var grid = Substitute.For<IDiscreteGridPointCoverage>();

            // Call | Assert
            void Call()
            {
                grid.GetCoordinateAt(null);
            }

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("coordinate"));
        }

        [Test]
        public void GetCoordinate_WithIndices_ReturnsExpectedResult()
        {
            // Setup
            int x = random.Next();
            int y = random.Next();

            double expectedWorldX = random.NextDouble() * 1000;
            double expectedWorldY = random.NextDouble() * 1000;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.X.Values[x, y] = expectedWorldX;
            grid.Y.Values[x, y] = expectedWorldY;

            // Call
            Coordinate result = grid.GetCoordinateAt(x, y);

            // Assert
            Assert.That(result.X, Is.EqualTo(expectedWorldX));
            Assert.That(result.Y, Is.EqualTo(expectedWorldY));
        }

        [Test]
        public void GetCoordinate_WithCoordinates_ReturnsExpectedResult()
        {
            // Setup
            var coord = new GridCoordinate(random.Next(),
                                           random.Next());

            double expectedWorldX = random.NextDouble() * 1000;
            double expectedWorldY = random.NextDouble() * 1000;

            var grid = Substitute.For<IDiscreteGridPointCoverage>();
            grid.X.Values[coord.X, coord.Y] = expectedWorldX;
            grid.Y.Values[coord.X, coord.Y] = expectedWorldY;

            // Call
            Coordinate result = grid.GetCoordinateAt(coord);

            // Assert
            Assert.That(result.X, Is.EqualTo(expectedWorldX));
            Assert.That(result.Y, Is.EqualTo(expectedWorldY));
        }

        private static Tuple<double, double> GetDoubleValues()
        {
            double lowerX0 = (random.NextDouble() * 2000) - 2000;
            double lowerX1 = (random.NextDouble() * 2000) + 2000;

            return lowerX0 < lowerX1 ? new Tuple<double, double>(lowerX0, lowerX1) : new Tuple<double, double>(lowerX1, lowerX0);
        }

        private static IEnumerable<TestCaseData> GetIsCounterClockWiseData()
        {
            Tuple<double, double> lowerX = GetDoubleValues();
            Tuple<double, double> upperX = GetDoubleValues();
            Tuple<double, double> leftY = GetDoubleValues();
            Tuple<double, double> rightY = GetDoubleValues();

            var pointLowerLeft = new Coordinate(lowerX.Item1, leftY.Item1);
            var pointLowerRight = new Coordinate(lowerX.Item2, rightY.Item1);
            var pointUpperLeft = new Coordinate(upperX.Item1, leftY.Item2);
            var pointUpperRight = new Coordinate(upperX.Item2, rightY.Item2);

            // Counter-Clockwise
            yield return new TestCaseData(new[]
            {
                pointLowerLeft,
                pointLowerRight,
                pointUpperRight,
                pointUpperLeft
            }, true);
            yield return new TestCaseData(new[]
            {
                pointUpperLeft,
                pointLowerLeft,
                pointLowerRight,
                pointUpperRight
            }, true);
            yield return new TestCaseData(new[]
            {
                pointUpperRight,
                pointUpperLeft,
                pointLowerLeft,
                pointLowerRight
            }, true);
            yield return new TestCaseData(new[]
            {
                pointLowerRight,
                pointUpperRight,
                pointUpperLeft,
                pointLowerLeft
            }, true);
            // Clockwise
            yield return new TestCaseData(new[]
            {
                pointLowerLeft,
                pointUpperLeft,
                pointUpperRight,
                pointLowerRight
            }, false);
            yield return new TestCaseData(new[]
            {
                pointLowerRight,
                pointLowerLeft,
                pointUpperLeft,
                pointUpperRight
            }, false);
            yield return new TestCaseData(new[]
            {
                pointUpperRight,
                pointLowerRight,
                pointLowerLeft,
                pointUpperLeft
            }, false);
            yield return new TestCaseData(new[]
            {
                pointUpperLeft,
                pointUpperRight,
                pointLowerRight,
                pointLowerLeft
            }, false);
        }

        [Test]
        [TestCaseSource(nameof(GetIsCounterClockWiseData))]
        public void IsCounterClockwisePolygon_ExpectedResults(Coordinate[] vertices, bool expectedResult)
        {
            // Call
            bool result = CartesianOrientationCalculatorHelper.IsCounterClockwisePolygon(vertices);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void IsCounterClockWisePolygon_PolygonVerticesNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call()
            {
                CartesianOrientationCalculatorHelper.IsCounterClockwisePolygon(null);
            }

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("polygonVertices"));
        }

        private static IEnumerable<TestCaseData> GetIsCounterClockWisePolygonInvalidOperationData()
        {
            var coordinate0 = new Coordinate(0.0, 0.0);
            var coordinate1 = new Coordinate(0.0, 1.0);

            IEnumerable<Coordinate> singleCoordinate = new[]
            {
                coordinate0
            };
            IEnumerable<Coordinate> twoCoordinates = new[]
            {
                coordinate0,
                coordinate1
            };

            yield return new TestCaseData(singleCoordinate);
            yield return new TestCaseData(twoCoordinates);
        }

        [Test]
        [TestCaseSource(nameof(GetIsCounterClockWisePolygonInvalidOperationData))]
        public void IsCounterClockWisePolygon_LessThanThreeCoordinates_ThrowsInvalidOperationException(Coordinate[] coordinates)
        {
            // Call | Assert
            void Call()
            {
                CartesianOrientationCalculatorHelper.IsCounterClockwisePolygon(coordinates);
            }

            Assert.Throws<InvalidOperationException>(Call);
        }

        private static IEnumerable<TestCaseData> GetGetNormalData()
        {
            var yCoord0 = new Coordinate(0.0, 0.0);
            var yCoord1 = new Coordinate(0.0, 1.0);

            yield return new TestCaseData(yCoord0, yCoord1, Vector2D.Create(1.0, 0.0));
            yield return new TestCaseData(yCoord1, yCoord0, Vector2D.Create(-1.0, 0.0));

            var xCoord0 = new Coordinate(0.0, 0.0);
            var xCoord1 = new Coordinate(1.0, 0.0);

            yield return new TestCaseData(xCoord0, xCoord1, Vector2D.Create(0.0, -1.0));
            yield return new TestCaseData(xCoord1, xCoord0, Vector2D.Create(0.0, 1.0));

            var yCoord2 = new Coordinate(0.0, -1.0);

            yield return new TestCaseData(yCoord2, yCoord1, Vector2D.Create(1.0, 0.0));
            yield return new TestCaseData(yCoord1, yCoord2, Vector2D.Create(-1.0, 0.0));

            var xCoord2 = new Coordinate(-1.0, 0.0);

            yield return new TestCaseData(xCoord2, xCoord1, Vector2D.Create(0.0, -1.0));
            yield return new TestCaseData(xCoord1, xCoord2, Vector2D.Create(0.0, 1.0));

            yield return new TestCaseData(xCoord1, yCoord1, Vector2D.Create(1.0, 0.0).Rotate(Degrees.ToRadians(45.0)));
            yield return new TestCaseData(yCoord1, xCoord1, Vector2D.Create(-1.0, 0.0).Rotate(Degrees.ToRadians(45.0)));
        }

        [Test]
        [TestCaseSource(nameof(GetGetNormalData))]
        public void GetNormal_ExpectedResults(Coordinate firstCoordinate,
                                              Coordinate lastCoordinate,
                                              Vector2D expectedNormal)
        {
            // Call
            Vector2D result = CartesianOrientationCalculatorHelper.GetNormal(firstCoordinate, lastCoordinate);

            // Assert
            Assert.That(result.X, Is.EqualTo(expectedNormal.X).Within(1E-10));
            Assert.That(result.Y, Is.EqualTo(expectedNormal.Y).Within(1E-10));
        }

        private static IEnumerable<TestCaseData> GetClosestAlignedValueParameterNullData()
        {
            IEnumerable<Tuple<int, Vector2D>> pairs = Enumerable.Empty<Tuple<int, Vector2D>>();
            var referenceNormal = Vector2D.Create(1.0, 0.0);

            yield return new TestCaseData(null, referenceNormal, "valueVectorPairs");
            yield return new TestCaseData(pairs, null, "referenceVector");
        }

        [Test]
        [TestCaseSource(nameof(GetClosestAlignedValueParameterNullData))]
        public void GetValueClosestAlignedWithVector_ParameterNull_ThrowsArgumentNullException(IEnumerable<Tuple<int, Vector2D>> valueNormalPairs,
                                                                                               Vector2D referenceNormal,
                                                                                               string expectedParamName)
        {
            // Call | Assert
            void Call()
            {
                CartesianOrientationCalculatorHelper.GetValueClosestAlignedWithVector(valueNormalPairs, referenceNormal, 0);
            }

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        private double GetRandomRotation()
        {
            return ((random.NextDouble() * 2) - 1) * Math.PI;
        }

        private Tuple<double, Vector2D> GetValueNormalPair(Vector2D referenceNormal)
        {
            double rotation = GetRandomRotation();
            double scalar = random.NextDouble() + 0.5;
            Vector2D normal = referenceNormal.Multiply(scalar)
                                             .Rotate(rotation);

            return new Tuple<double, Vector2D>(rotation, normal);
        }

        [Test]
        public void GetValueClosestAlignedWithVector_ExpectedResults()
        {
            var referenceNormal = Vector2D.Create((random.NextDouble() + 0.1) * 100,
                                                  (random.NextDouble() + 0.1) * 100);

            Tuple<double, Vector2D>[] valueNormalPairs =
                Enumerable.Range(0, 5).Select(_ => GetValueNormalPair(referenceNormal)).ToArray();

            double expectedValue = valueNormalPairs.OrderBy(x => Math.Abs(x.Item1))
                                                   .First().Item1;

            // Call
            double result = CartesianOrientationCalculatorHelper.GetValueClosestAlignedWithVector(valueNormalPairs,
                                                                                                  referenceNormal,
                                                                                                  0.0);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }
    }
}