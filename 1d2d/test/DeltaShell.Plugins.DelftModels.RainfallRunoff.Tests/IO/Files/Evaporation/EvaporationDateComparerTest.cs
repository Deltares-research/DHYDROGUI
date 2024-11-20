using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Files.Evaporation
{
    public class EvaporationDateComparerTest
    {
        [Test]
        [TestCaseSource(nameof(CompareToCases))]
        public void CompareTo_ReturnsTheCorrectResult(EvaporationDate date, EvaporationDate other, int expResult)
        {
            //Arrange
            EvaporationDateComparer dateComparer = new EvaporationDateComparer();
            
            // Call
            int result = dateComparer.Compare(date, other);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }
        
        private static IEnumerable<TestCaseData> CompareToCases()
        {
            var date = new EvaporationDate(2022, 8, 24);

            var otherYearGreater = new EvaporationDate(date.Year + 1, date.Month, date.Day);
            var otherMonthGreater = new EvaporationDate(date.Year, date.Month + 1, date.Day);
            var otherDayGreater = new EvaporationDate(date.Year, date.Month, date.Day + 1);
            var otherSame = new EvaporationDate(date.Year, date.Month, date.Day);
            var otherYearSmaller = new EvaporationDate(date.Year - 1, date.Month, date.Day);
            var otherMonthSmaller = new EvaporationDate(date.Year, date.Month - 1, date.Day);
            var otherDaySmaller = new EvaporationDate(date.Year, date.Month, date.Day - 1);

            yield return new TestCaseData(date, otherYearGreater, -1);
            yield return new TestCaseData(date, otherMonthGreater, -1);
            yield return new TestCaseData(date, otherDayGreater, -1);
            yield return new TestCaseData(date, otherSame, 0);
            yield return new TestCaseData(date, otherYearSmaller, 1);
            yield return new TestCaseData(date, otherMonthSmaller, 1);
            yield return new TestCaseData(date, otherDaySmaller, 1);
        }
    }
}