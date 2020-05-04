using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Shared;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.Shared
{
    [TestFixture]
    public class SpatialDoubleTest
    {
        private readonly Random random = new Random();

        [TestCaseSource(nameof(GetRoundTestCases))]
        public void Round_ReturnsCorrectResult(double value, double expected)
        {
            // Call
            double result = SpatialDouble.Round(value);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        private IEnumerable<TestCaseData> GetRoundTestCases()
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

        private IEnumerable<TestCaseData> GetAreEqualTestCases()
        {
            const double maxDif = 1E-7;
            double valueA = 0;
            double valueB = maxDif;
            yield return new TestCaseData(valueA, valueB, false);

            valueA = random.NextDouble();
            valueB = valueA + maxDif + 1E-10;
            yield return new TestCaseData(valueA, valueB, false);

            valueA = random.NextDouble();
            valueB = valueA + (maxDif - 1E-10);
            yield return new TestCaseData(valueA, valueB, true);
        }
    }
}