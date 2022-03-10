using DelftTools.Hydro.Structures.LeveeBreachFormula;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures.LeveeBreachFormula
{
    [TestFixture]
    public class UserDefinedBreachConversionHelper
    {
        [Test]
        public void UserDefinedBreachConversionHelper_GetFormattedTimeSeriesShouldBe()
        {
            var timeSeries = Hydro.Structures.LeveeBreachFormula.UserDefinedBreachConversionHelper.GetFormattedTimeSeries();

            Assert.AreEqual(1, timeSeries.Arguments.Count);
            Assert.AreEqual(2, timeSeries.Components.Count);
        }
    }
}