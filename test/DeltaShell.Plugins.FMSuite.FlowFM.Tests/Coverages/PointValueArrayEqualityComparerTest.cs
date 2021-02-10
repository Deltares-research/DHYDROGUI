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
            var comparer = new PointValueArrayEqualityComparer();

            // Assert
            Assert.That(comparer, Is.InstanceOf<IEqualityComparer<IPointValue[]>>());
        }

        [Test]
        [TestCaseSource(nameof(HashCodeCases))]
        public void GetHashCode_ReturnsCorrectResult(IPointValue[] x, IPointValue[] y, bool expResult)
        {
            // Setup
            var comparer = new PointValueArrayEqualityComparer();

            // Call
            int hashCodeX = comparer.GetHashCode(x);
            int hashCodeY = comparer.GetHashCode(y);

            // Assert
            Assert.That(hashCodeX == hashCodeY, Is.EqualTo(expResult));
        }

        [TestCaseSource(nameof(EqualsCases))]
        public void Equals_ReturnsCorrectResult(IPointValue[] x, IPointValue[] y, bool expResult)
        {
            // Setup
            var comparer = new PointValueArrayEqualityComparer();

            // Call
            bool result = comparer.Equals(x, y);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> HashCodeCases()
        {
            IPointValue[] array1 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array2 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
                GetPointValue(9, 10, 11),
            };

            IPointValue[] array3 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 13),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array4 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(13, 4, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array5 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 13, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array6 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array7 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
            };

            yield return new TestCaseData(array1, array1, true);
            yield return new TestCaseData(array1, array2, false);
            yield return new TestCaseData(array1, array3, false);
            yield return new TestCaseData(array1, array4, false);
            yield return new TestCaseData(array1, array5, false);
            yield return new TestCaseData(array1, array6, true);
            yield return new TestCaseData(array6, array1, true);
            yield return new TestCaseData(array1, array7, true);
            yield return new TestCaseData(array7, array1, true);
            yield return new TestCaseData(array7, array6, true);
            yield return new TestCaseData(array6, array7, true);
        }

        private static IEnumerable<TestCaseData> EqualsCases()
        {
            IPointValue[] array1 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array2 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
                GetPointValue(9, 10, 11),
            };

            IPointValue[] array3 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 13),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array4 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(13, 4, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array5 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 13, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array6 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
            };

            IPointValue[] array7 =
            {
                GetPointValue(0, 1, 2),
                GetPointValue(3, 4, 5),
                GetPointValue(6, 7, 8),
            };

            yield return new TestCaseData(array1, array1, true);
            yield return new TestCaseData(array1, null, false);
            yield return new TestCaseData(null, array1, false);
            yield return new TestCaseData(array1, array2, false);
            yield return new TestCaseData(array1, array3, false);
            yield return new TestCaseData(array1, array4, false);
            yield return new TestCaseData(array1, array5, false);
            yield return new TestCaseData(array1, array6, true);
            yield return new TestCaseData(array6, array1, true);
            yield return new TestCaseData(array1, array7, true);
            yield return new TestCaseData(array7, array1, true);
            yield return new TestCaseData(array7, array6, true);
            yield return new TestCaseData(array6, array7, true);
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