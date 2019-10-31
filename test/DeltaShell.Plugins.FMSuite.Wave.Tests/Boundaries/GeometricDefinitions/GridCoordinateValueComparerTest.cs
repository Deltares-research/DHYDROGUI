using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.GeometricDefinitions
{
    [TestFixture]
    public class GridCoordinateValueComparerTest
    {
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
            };

        [Test]
        public void GetHashCode_ReturnsCorrectValue()
        {
            // Setup
            var equalityComparer = new GridCoordinateValueComparer();
            var coordinate = new GridCoordinate(5, 10);
            const int expectedValue = 5 ^ 10;

            // Call
            int hashValue = equalityComparer.GetHashCode(coordinate);

            // Assert
            Assert.That(hashValue, Is.EqualTo(expectedValue));
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