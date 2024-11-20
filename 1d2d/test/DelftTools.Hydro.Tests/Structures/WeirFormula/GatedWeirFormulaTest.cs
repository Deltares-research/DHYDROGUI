using System;
using DelftTools.Hydro.Structures.WeirFormula;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures.WeirFormula
{
    [TestFixture]
    public class GatedWeirFormulaTest
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void IsUsingTimeSeriesForLowerEdgeLevel_CanBeTimeDependent_ReturnsExpected(bool useTimeSeries)
        {
            var formula = new GatedWeirFormula(true) { UseLowerEdgeLevelTimeSeries = useTimeSeries };

            Assert.That(formula.IsUsingTimeSeriesForLowerEdgeLevel(), Is.EqualTo(useTimeSeries));
        }

        [Test]
        public void IsUsingTimeSeriesForLowerEdgeLevel_CannotBeTimeDependentAndUseTimeSeries_ThrowsNotSupportedException()
        {
            var formula = new GatedWeirFormula(false);

            Assert.Throws<NotSupportedException>(() => formula.UseLowerEdgeLevelTimeSeries = true);
        }
    }
}