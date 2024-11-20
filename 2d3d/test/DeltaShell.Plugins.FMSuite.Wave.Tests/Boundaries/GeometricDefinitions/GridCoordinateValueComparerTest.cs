using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridCoordinateValueComparerTest
    {
        private static IEnumerable<TestCaseData> EqualsValues =>
            new[]
            {
                new TestCaseData(null, null, true),
                new TestCaseData(new GridCoordinate(0, 0), null, false),
                new TestCaseData(null, new GridCoordinate(0, 0), false),
                new TestCaseData(new GridCoordinate(2, 2), new GridCoordinate(2, 2), true),
                new TestCaseData(new GridCoordinate(1, 2), new GridCoordinate(2, 2), false),
                new TestCaseData(new GridCoordinate(2, 1), new GridCoordinate(2, 2), false),
                new TestCaseData(new GridCoordinate(2, 2), new GridCoordinate(1, 2), false),
                new TestCaseData(new GridCoordinate(2, 2), new GridCoordinate(2, 1), false),
                new TestCaseData(new GridCoordinate(1, 1), new GridCoordinate(2, 2), false),
                new TestCaseData(new GridCoordinate(1, 2), new GridCoordinate(2, 1), false),
                new TestCaseData(new GridCoordinate(1, 1), new GridCoordinate(2, 1), false),
                new TestCaseData(new GridCoordinate(1, 1), new GridCoordinate(1, 2), false),
                new TestCaseData(new GridCoordinate(int.MaxValue, 0), new GridCoordinate(int.MaxValue, 0), true),
                new TestCaseData(new GridCoordinate(int.MaxValue, int.MaxValue), new GridCoordinate(int.MaxValue, int.MaxValue), true),
                new TestCaseData(new GridCoordinate(0, int.MaxValue), new GridCoordinate(0, int.MaxValue), true),
                new TestCaseData(new GridCoordinate(int.MaxValue, int.MaxValue), new GridCoordinate(int.MaxValue, int.MaxValue), true),
                new TestCaseData(new GridCoordinate(int.MaxValue, int.MaxValue), new GridCoordinate(2, 2), false),
                new TestCaseData(new GridCoordinate(int.MaxValue, int.MaxValue), new GridCoordinate(1, 2), false)
            };

        [Test]
        [TestCaseSource(nameof(EqualsValues))]
        public void Equals_ReturnsCorrectValue(GridCoordinate coordinate1, GridCoordinate coordinate2, bool expectedValue)
        {
            // Setup
            var equalityComparer = new GridCoordinateValueComparer();

            // Call
            bool result = equalityComparer.Equals(coordinate1,
                                                  coordinate2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue),
                        $"Expected the comparison of {coordinate1} and {coordinate2} to be different:");
        }

        [Test]
        public void GetHashCode_ReturnsCorrectValue()
        {
            // Setup
            var equalityComparer = new GridCoordinateValueComparer();
            var coordinate = new GridCoordinate(5, 10);
            const int expectedValue = 31;

            // Call
            int hashValue = equalityComparer.GetHashCode(coordinate);

            // Assert
            Assert.That(hashValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCaseSource(nameof(EqualsValues))]
        public void GetHashCode_MatchesEquals(GridCoordinate coordinate1, GridCoordinate coordinate2, bool expectedResult)
        {
            // Setup
            var equalityComparer = new GridCoordinateValueComparer();

            // Call
            int hashValue1 = equalityComparer.GetHashCode(coordinate1);
            int hashValue2 = equalityComparer.GetHashCode(coordinate2);

            // Assert
            Assert.That(hashValue1 == hashValue2, Is.EqualTo(expectedResult),
                        $"Expected hashValue1, {hashValue1} == hashValue2, {hashValue2} to be {expectedResult}.");
        }

        [Test]
        public void GetHashCode_NullValue_ReturnsZero()
        {
            // Setup
            var equalityComparer = new GridCoordinateValueComparer();

            // Call
            int hashValue = equalityComparer.GetHashCode(null);

            // Assert
            Assert.That(hashValue, Is.EqualTo(0));
        }
    }
}