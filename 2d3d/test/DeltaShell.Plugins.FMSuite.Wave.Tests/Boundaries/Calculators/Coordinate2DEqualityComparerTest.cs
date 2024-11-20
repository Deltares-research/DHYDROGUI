using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Calculators
{
    [TestFixture]
    public class Coordinate2DEqualityComparerTest
    {
        private static IEnumerable<TestCaseData> EqualsTestData
        {
            get
            {
                var coordinate = new Coordinate(0.0, 0.0);

                yield return new TestCaseData(null, null, true);
                yield return new TestCaseData(coordinate, null, false);
                yield return new TestCaseData(null, coordinate, false);
                yield return new TestCaseData(coordinate, coordinate, true);
                yield return new TestCaseData(coordinate, new Coordinate(1.0, 0.0), false);
                yield return new TestCaseData(new Coordinate(1.0, 0.0), new Coordinate(1.0, 0.0), true);
            }
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var comparer = new Coordinate2DEqualityComparer();

            // Assert
            Assert.That(comparer, Is.InstanceOf(typeof(IEqualityComparer<Coordinate>)));
        }

        [Test]
        [TestCaseSource(nameof(EqualsTestData))]
        public void Equals_ExpectedResult(Coordinate x, Coordinate y, bool expectedResult)
        {
            // Setup
            var comparer = new Coordinate2DEqualityComparer();

            // Call
            bool result = comparer.Equals(x, y);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetHashCode_ExpectedResult()
        {
            // Setup
            var coordinate = new Coordinate(5.1, 10.37);
            int expectedResult = (coordinate.X.GetHashCode() ^ coordinate.Y.GetHashCode()).GetHashCode();

            var comparer = new Coordinate2DEqualityComparer();

            // Call
            int hCode = comparer.GetHashCode(coordinate);

            // Assert
            Assert.That(hCode, Is.EqualTo(expectedResult));
        }
    }
}