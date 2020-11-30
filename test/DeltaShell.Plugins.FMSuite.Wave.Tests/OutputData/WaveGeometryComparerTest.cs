using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WaveGeometryComparerTest
    {
        [TestCaseSource(nameof(EqualsCases))]
        public void Equals_ReturnsCorrectResult(IGeometry x, IGeometry y, bool expResult)
        {
            // Setup
            var comparer = new WaveGeometryComparer();

            // Call
            bool result = comparer.Equals(x, y);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        [TestCaseSource(nameof(GetHashCodeCases))]
        public void GetHashCode_ReturnsCorrectResult(IGeometry obj, int expResult)
        {
            // Setup
            var comparer = new WaveGeometryComparer();

            // Call
            int result = comparer.GetHashCode(obj);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> GetHashCodeCases()
        {
            yield return new TestCaseData(new Point(0, 1), 0);
            yield return new TestCaseData(new Point(2, 4), 8);
            yield return new TestCaseData(new Point(2.1, 4.1), 8);
            yield return new TestCaseData(new Point(5, 15), 75);
            yield return new TestCaseData(new Point(5.9, 15.9), 75);
            yield return new TestCaseData(new Point(16, 64), 1024);
        }

        private static IEnumerable<TestCaseData> EqualsCases()
        {
            for (var i = 1e-15; i < 1e+9; i *= 10)
            {
                yield return new TestCaseData(new Point(i, i),
                                              new Point(i - 5e-8, i + 5e-8),
                                              true);
                yield return new TestCaseData(new Point(i, i),
                                              new Point(i - 5e-7, i + 5e-7),
                                              false);
            }

            for (var i = 1e+10; i < 1e+20; i *= 10)
            {
                yield return new TestCaseData(new Point(i, i),
                                              new Point(i - 5e-7, i + 5e-7),
                                              true);
            }
        }
    }
}