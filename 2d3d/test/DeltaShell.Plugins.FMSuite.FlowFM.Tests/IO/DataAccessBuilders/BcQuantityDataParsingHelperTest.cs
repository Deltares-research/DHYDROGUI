using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.DataAccessBuilders
{
    public class BcQuantityDataParsingHelperTest
    {
        [Test]
        public void GivenIncorrectDateTimeFormat_WhenParseTimeZone_ThenThrowFormatException()
        {
            //Arrange
            const string datetimeFormat = "seconds since incorrect-date incorrect-time";
            const string incorrectDatetimeFormat = "incorrect-date incorrect-time";
            const string locationName = "locationName";
            const string expectedMessage = "Time format '" + incorrectDatetimeFormat + "' in support point '" + locationName + "' is not supported by bc file parser";

            //Act
            void Call() => BcQuantityDataParsingHelper.ParseTimeZone(datetimeFormat, locationName);

            //Assert
            var exception = Assert.Throws<FormatException>(Call);
            Assert.That(exception.Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(DateTimesTimeZonesAndExpectedUnits))]
        public void GivenDateTime_WhenParseTimeZone_ThenReturnTimeZone(DateTime _, TimeSpan expectedTimeZone, string givenDateTime)
        {
            Assert.That(BcQuantityDataParsingHelper.ParseTimeZone(givenDateTime,"name"), Is.EqualTo(expectedTimeZone));
        }

        [Test]
        [TestCase("")]
        [TestCase("-")]
        [TestCase("seconds from 2023-08-01 14:05:05")]
        public void GivenSpecificFormats_WhenParseTimeZone_ThenReturnTimeZoneOfZero(string givenFormat)
        {
            Assert.That(BcQuantityDataParsingHelper.ParseTimeZone(givenFormat, "name"), Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        [TestCaseSource(nameof(DateTimesTimeZonesAndExpectedUnits))]
        public void GivenDateTimeAndTimeSpan_WhenGetDateTimeUnit_ThenReturnExpectedUnit(DateTime referenceTime, TimeSpan timeZone, string expectedUnit)
        {
            Assert.That(BcQuantityDataParsingHelper.GetDateTimeUnit(referenceTime, timeZone), Is.EqualTo(expectedUnit));
        }

        [Test]
        [TestCaseSource(nameof(GivenTimeAndUnitAndExpectedDateTime))]
        public void GivenTimeAndUnitAsBcQuantityData_WhenParseDateTimes_ThenReturnExpectedDateTimes(List<string> values, string unit, List<DateTime> expectedDateTimes)
        {
            var locationName = "Boundary1_0001";
            var quantityData = new BcQuantityData()
            {
                Values = values,
                Unit = unit
            };

            List<DateTime> dateTimes = BcQuantityDataParsingHelper.ParseDateTimes(locationName, quantityData).ToList();
            Assert.That(dateTimes[0], Is.EqualTo(expectedDateTimes[0]));
            Assert.That(dateTimes[1], Is.EqualTo(expectedDateTimes[1]));
        }

        private static IEnumerable<TestCaseData> DateTimesTimeZonesAndExpectedUnits()
        {
            yield return new TestCaseData(new DateTime(2023, 8, 1, 14, 5, 5), TimeSpan.Zero, "seconds since 2023-08-01 14:05:05");
            yield return new TestCaseData(new DateTime(2023, 8, 1, 14, 5, 5), new TimeSpan(4, 15, 00), "seconds since 2023-08-01 14:05:05 +04:15");
            yield return new TestCaseData(new DateTime(2023, 8, 1, 14, 5, 5), new TimeSpan(-10, -30, 00), "seconds since 2023-08-01 14:05:05 -10:30");
        }

        private static IEnumerable<TestCaseData> GivenTimeAndUnitAndExpectedDateTime()
        {
            var values = new List<string>()
            {
                "0",
                "86400"
            };
            var unit = "seconds since 2001-01-01 00:00:00";

            var dateTimes = new List<DateTime>()
            {
                new DateTime(2001, 1, 1, 0, 0, 0),
                new DateTime(2001, 1, 2, 0, 0, 0)
            };

            yield return new TestCaseData(values, unit, dateTimes);
            
            unit = "seconds since 2001-01-01 00:00:00 +04:15";
            yield return new TestCaseData(values, unit, dateTimes);
            
            unit = "seconds since 2001-01-01 00:00:00 -10:30";
            yield return new TestCaseData(values, unit, dateTimes);

            values = new List<string>()
            {
                "0",
                "1440"
            };
            unit = "minutes since 2001-01-01 00:00:00";

            yield return new TestCaseData(values, unit, dateTimes);

            values = new List<string>()
            {
                "0",
                "24"
            };
            unit = "hours since 2001-01-01 00:00:00";

            yield return new TestCaseData(values, unit, dateTimes);

            values = new List<string>()
            {
                "0",
                "1"
            };
            unit = "days since 2001-01-01 00:00:00";

            yield return new TestCaseData(values, unit, dateTimes);
        }
    }
}