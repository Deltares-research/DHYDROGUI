using System;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Import
{
    [TestFixture]
    public class DimrXmlTimeParserTest
    {
        [TestCase(0.5, 1.5, 2.5)]
        [TestCase(86400, 1200, 172800)]
        [TestCase(60, 1, 20)]
        public void TryParse_TimeStringValid_ReturnsTrueWithCorrectlyParsedTimers(double start, double step, double stop)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            DateTime referenceTime = DateTime.Today;
            var timeStr = $"{start}  {step}  {stop}";

            // Call
            bool result = DimrXmlTimeParser.TryParse(referenceTime, timeStr, logHandler, out ModelTimers timers);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(timers.StartTime, Is.EqualTo(referenceTime.AddSeconds(start)));
            Assert.That(timers.TimeStep, Is.EqualTo(TimeSpan.FromSeconds(step)));
            Assert.That(timers.StopTime, Is.EqualTo(referenceTime.AddSeconds(stop)));

            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void TryParse_TimeStringNullOrEmpty_ReturnsFalseAndLogsError(string timeStr)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            DateTime referenceTime = DateTime.Today;

            // Call
            bool result = DimrXmlTimeParser.TryParse(referenceTime, timeStr, logHandler, out ModelTimers timers);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(timers, Is.Null);

            Assert.That(logHandler.ReceivedCalls(), Has.Length.EqualTo(1));
            logHandler.Received(1).ReportError("The time element should not be empty.");
        }

        [TestCase("0 86400 1200 172800")]
        [TestCase("86400 172800")]
        public void TryParse_TimeStringInvalidNumberOfValues_ReturnsFalseAndLogsError(string timeStr)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            DateTime referenceTime = DateTime.Today;

            // Call
            bool result = DimrXmlTimeParser.TryParse(referenceTime, timeStr, logHandler, out ModelTimers timers);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(timers, Is.Null);

            Assert.That(logHandler.ReceivedCalls(), Has.Length.EqualTo(1));
            logHandler.Received(1).ReportError("The time element should contain three timers: the start time, the time step, and the stop time.");
        }

        [TestCase("one two 172800", "one", "two")]
        [TestCase("one 1200 three", "one", "three")]
        [TestCase("86400 two three", "two", "three")]
        public void TryParse_TimeStringInvalidValues_ReturnsFalseAndLogsError(string timeStr, string value1, string value2)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            DateTime referenceTime = DateTime.Today;

            // Call
            bool result = DimrXmlTimeParser.TryParse(referenceTime, timeStr, logHandler, out ModelTimers timers);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(timers, Is.Null);

            Assert.That(logHandler.ReceivedCalls(), Has.Length.EqualTo(2));
            logHandler.Received(1).ReportError($"'{value1}' is not a valid number of seconds.");
            logHandler.Received(1).ReportError($"'{value2}' is not a valid number of seconds.");
        }
    }
}