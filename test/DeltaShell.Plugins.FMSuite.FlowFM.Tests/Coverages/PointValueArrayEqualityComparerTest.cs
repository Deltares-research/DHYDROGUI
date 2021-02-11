using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Coverages
{
    [TestFixture]
    public class PointValueArrayEqualityComparerTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var comparer = new PointValueEqualityComparer();

            // Assert
            Assert.That(comparer, Is.InstanceOf<IEqualityComparer<IPointValue>>());
        }

        [Test]
        [TestCaseSource(nameof(HashCodeCases))]
        public void GetHashCode_ReturnsCorrectResult(IPointValue x, IPointValue y, bool expResult)
        {
            // Setup
            var comparer = new PointValueEqualityComparer();

            // Call
            int hashCodeX = comparer.GetHashCode(x);
            int hashCodeY = comparer.GetHashCode(y);

            // Assert
            Assert.That(hashCodeX == hashCodeY, Is.EqualTo(expResult));
        }

        [TestCaseSource(nameof(EqualsCases))]
        public void Equals_ReturnsCorrectResult(IPointValue x, IPointValue y, bool expResult)
        {
            // Setup
            var comparer = new PointValueEqualityComparer();

            // Call
            bool result = comparer.Equals(x, y);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> HashCodeCases()
        {
            IPointValue pointValue0 = GetPointValue(0, 1, 2);
            IPointValue pointValue1 = GetPointValue(0, 1, 13);
            IPointValue pointValue2 = GetPointValue(0, 13, 2);
            IPointValue pointValue3 = GetPointValue(13, 1, 2);
            IPointValue pointValue4 = GetPointValue(0, 1, 2);
            IPointValue pointValue5 = GetPointValue(0, 1, 2);

            yield return new TestCaseData(pointValue0, pointValue0, true);
            yield return new TestCaseData(pointValue0, pointValue1, false);
            yield return new TestCaseData(pointValue0, pointValue2, false);
            yield return new TestCaseData(pointValue0, pointValue3, false);
            yield return new TestCaseData(pointValue0, pointValue4, true);
            yield return new TestCaseData(pointValue4, pointValue0, true);
            yield return new TestCaseData(pointValue0, pointValue5, true);
            yield return new TestCaseData(pointValue5, pointValue0, true);
            yield return new TestCaseData(pointValue5, pointValue4, true);
            yield return new TestCaseData(pointValue4, pointValue5, true);
        }

        private static IEnumerable<TestCaseData> EqualsCases()
        {
            IPointValue pointValue0 = GetPointValue(0, 1, 2);
            IPointValue pointValue1 = GetPointValue(0, 1, 13);
            IPointValue pointValue2 = GetPointValue(0, 13, 2);
            IPointValue pointValue3 = GetPointValue(13, 1, 2);
            IPointValue pointValue4 = GetPointValue(0, 1, 2);
            IPointValue pointValue5 = GetPointValue(0, 1, 2);

            yield return new TestCaseData(pointValue0, pointValue0, true);
            yield return new TestCaseData(pointValue0, null, false);
            yield return new TestCaseData(null, pointValue0, false);
            yield return new TestCaseData(pointValue0, pointValue1, false);
            yield return new TestCaseData(pointValue0, pointValue2, false);
            yield return new TestCaseData(pointValue0, pointValue3, false);
            yield return new TestCaseData(pointValue0, pointValue4, true);
            yield return new TestCaseData(pointValue4, pointValue0, true);
            yield return new TestCaseData(pointValue0, pointValue5, true);
            yield return new TestCaseData(pointValue5, pointValue0, true);
            yield return new TestCaseData(pointValue5, pointValue4, true);
            yield return new TestCaseData(pointValue4, pointValue5, true);
        }

        private static IPointValue GetPointValue(double x, double y, double value)
        {
            var pointValue = Substitute.For<IPointValue>();
            pointValue.X = x;
            pointValue.Y = y;
            pointValue.Value = value;

            return pointValue;
        }
    }
}