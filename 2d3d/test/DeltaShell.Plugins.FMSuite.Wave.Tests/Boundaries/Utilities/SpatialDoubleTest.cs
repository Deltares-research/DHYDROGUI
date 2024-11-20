using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Utilities
{
    [TestFixture]
    public class SpatialDoubleTest
    {
        private static readonly Random random = new Random();

        [TestCaseSource(nameof(GetRoundTestCases))]
        public void Round_ReturnsCorrectResult(double value, double expected)
        {
            // Call
            double result = SpatialDouble.Round(value);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> GetRoundTestCases()
        {
            double value = random.NextDouble();
            double expected = Math.Round(value, 7, MidpointRounding.AwayFromZero);
            yield return new TestCaseData(value, expected);

            value = 0.00000005;
            expected = 0.0000001;
            yield return new TestCaseData(value, expected);
        }

        [TestCaseSource(nameof(GetAreEqualTestCases))]
        public void AreEqual_ReturnsCorrectResult(double valueA, double valueB, bool expectedResult)
        {
            // Call
            bool result = SpatialDouble.AreEqual(valueA, valueB);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> GetAreEqualTestCases()
        {
            const double value1 = 7E-7;
            const double value2 = 8E-7;

            foreach (double a in GetDoublesRoundingTo(value1))
            {
                foreach (double b in GetDoublesRoundingTo(value1))
                {
                    yield return new TestCaseData(a, b, true);
                }

                foreach (double b in GetDoublesRoundingTo(value2))
                {
                    yield return new TestCaseData(a, b, false);
                }
            }
        }

        private static IEnumerable<double> GetDoublesRoundingTo(double value)
        {
            yield return Math.Round(value - 5E-8, 8);
            yield return value - 4E-8;
            yield return value - 3E-8;
            yield return value - 2E-8;
            yield return value - 1E-8;
            yield return value;
            yield return value + 1E-8;
            yield return value + 2E-8;
            yield return value + 3E-8;
            yield return value + 4E-8;
        }
    }
}