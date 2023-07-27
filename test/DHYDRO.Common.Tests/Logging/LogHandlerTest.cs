using System;
using System.Linq;
using DHYDRO.Common.Logging;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.Logging
{
    [TestFixture]
    public class LogHandlerTest
    {
        private const string ActivityName = "some_activity";
        private const string LogMessage = "some_log_message";
        private const string Format = "{0}_{1}_{2}";

        private readonly object[] formatArgs =
        {
            "some",
            "log",
            "message"
        };

        [Test]
        public void GivenALogMessage_WhenReportInfoIsCalled_MessageIsAddedToLogMessagesTableWithInfoSeverity()
        {
            // When
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportInfo(LogMessage);

            // Then
            Assert.AreEqual(1, logHandler.LogMessages.AllMessages.Count(),
                            "Exactly 1 log message was expected to be collected by the log handler.");
            string logMessage = logHandler.LogMessages.InfoMessages.FirstOrDefault(m => m.Equals(LogMessage));
            Assert.NotNull(logMessage,
                           $"Expected log message '{LogMessage}' with expected severity '{LogSeverity.Info}' was not added to the list.");
        }

        [Test]
        public void GivenALogMessage_WhenReportWarningIsCalled_MessageIsAddedToLogMessagesTableWithWarningSeverity()
        {
            // When
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportWarning(LogMessage);

            // Then
            Assert.AreEqual(1, logHandler.LogMessages.AllMessages.Count(),
                            "Exactly 1 log message was expected to be collected by the log handler.");
            string logMessage = logHandler.LogMessages.WarningMessages.FirstOrDefault(m => m.Equals(LogMessage));
            Assert.NotNull(logMessage,
                           $"Expected log message '{LogMessage}' with expected severity '{LogSeverity.Warning}' was not added to the list.");
        }

        [Test]
        public void GivenALogMessage_WhenReportErrorIsCalled_MessageIsAddedToLogMessagesTableWithErrorSeverity()
        {
            // When
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportError(LogMessage);

            // Then
            Assert.AreEqual(1, logHandler.LogMessages.AllMessages.Count(),
                            "Exactly 1 log message was expected to be collected by the log handler.");
            string logMessage = logHandler.LogMessages.ErrorMessages.FirstOrDefault(m => m.Equals(LogMessage));
            Assert.NotNull(logMessage,
                           $"Expected log message '{LogMessage}' with expected severity '{LogSeverity.Error}' was not added to the list.");
        }

        [Test]
        public void GivenALogMessage_WhenReportInfoFormatIsCalled_MessageIsAddedToLogMessagesTableWithInfoSeverity()
        {
            // When
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportInfoFormat(Format, formatArgs);

            // Then
            Assert.AreEqual(1, logHandler.LogMessages.AllMessages.Count(),
                            "Exactly 1 log message was expected to be collected by the log handler.");
            string logMessage = logHandler.LogMessages.InfoMessages.FirstOrDefault(m => m.Equals(LogMessage));
            Assert.NotNull(logMessage,
                           $"Expected log message '{LogMessage}' with expected severity '{LogSeverity.Info}' was not added to the list.");
        }

        [Test]
        public void GivenALogMessage_WhenReportWarningFormatIsCalled_MessageIsAddedToLogMessagesTableWithWarningSeverity()
        {
            // When
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportWarningFormat(Format, formatArgs);

            // Then
            Assert.AreEqual(1, logHandler.LogMessages.AllMessages.Count(),
                            "Exactly 1 log message was expected to be collected by the log handler.");
            string logMessage = logHandler.LogMessages.WarningMessages.FirstOrDefault(m => m.Equals(LogMessage));
            Assert.NotNull(logMessage,
                           $"Expected log message '{LogMessage}' with expected severity '{LogSeverity.Warning}' was not added to the list.");
        }

        [Test]
        public void GivenALogMessage_WhenReportErrorFormatIsCalled_MessageIsAddedToLogMessagesTableWithErrorSeverity()
        {
            // When
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportErrorFormat(Format, formatArgs);

            // Then
            Assert.AreEqual(1, logHandler.LogMessages.AllMessages.Count(),
                            "Exactly 1 log message was expected to be collected by the log handler.");
            string logMessage = logHandler.LogMessages.ErrorMessages.FirstOrDefault(m => m.Equals(LogMessage));
            Assert.NotNull(logMessage,
                           $"Expected log message '{LogMessage}' with expected severity '{LogSeverity.Error}' was not added to the list.");
        }

        [Test]
        public void GivenALogHandlerWithoutLogMessages_WhenLogReportIsCalled_ThenNoReportIsLogged()
        {
            var logHandler = new LogHandler(ActivityName);
            Log4NetTestHelper.AssertLogMessagesCount(() => logHandler.LogReport(), 0);
        }

        [Test]
        public void GivenALogHandlerWithALogMessage_WhenLogReportIsCalled_ThenReportIsLoggedWithCorrectName()
        {
            // Given
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportError(LogMessage);

            // When, Then
            Log4NetTestHelper.AssertAtLeastOneLogMessagesContains(() => logHandler.LogReport(), ActivityName);
        }

        [Test]
        public void GivenALogHandlerWithALogMessage_WhenLogReportIsCalled_ThenReportIsLoggedWithCorrectMessage()
        {
            // Given
            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportError(LogMessage);

            // When, Then
            Log4NetTestHelper.AssertAtLeastOneLogMessagesContains(() => logHandler.LogReport(), LogMessage);
        }

        [Test]
        public void GivenALogHandlerWithLogMessagesOfAllSeverities_WhenLogReportIsCalled_ThenAlwaysOneReportForEachSeverityIsCreated()
        {
            // Given
            const string infoMessage1 = "info_message1";
            const string infoMessage2 = "info_message2";
            const string warningMessage1 = "warning_message1";
            const string warningMessage2 = "warning_message2";
            const string errorMessage1 = "error_message1";
            const string errorMessage2 = "error_message2";

            var logHandler = new LogHandler(ActivityName);
            logHandler.ReportInfo(infoMessage1);
            logHandler.ReportInfo(infoMessage2);
            logHandler.ReportWarning(warningMessage1);
            logHandler.ReportWarning(warningMessage2);
            logHandler.ReportError(errorMessage1);
            logHandler.ReportError(errorMessage2);

            Assert.AreEqual(6, logHandler.LogMessages.AllMessages.Count(),
                            "Exactly 6 log messages were expected to be collected by the log handler.");

            // When
            void LogReportAction() => logHandler.LogReport();

            // Then
            string[] messages = Log4NetTestHelper.GetAllRenderedMessages(LogReportAction).ToArray();
            Assert.That(messages[0], Is.EqualTo(CreateExpectedLogMessage("During some_activity the following errors were reported:", errorMessage1, errorMessage2)));
            Assert.That(messages[1], Is.EqualTo(CreateExpectedLogMessage("During some_activity the following warnings were reported:", warningMessage1, warningMessage2)));
            Assert.That(messages[2], Is.EqualTo(CreateExpectedLogMessage("During some_activity the following infos were reported:", infoMessage1, infoMessage2)));
            Assert.That(logHandler.LogMessages.AllMessages, Is.Empty);
        }

        [Test]
        public void LogReport_WithMaxMessages_CreatesCorrectReport()
        {
            // Setup
            var logHandler = new LogHandler(ActivityName, 2);
            logHandler.ReportWarning(LogMessage);
            logHandler.ReportWarning(LogMessage);
            logHandler.ReportWarning(LogMessage);
            logHandler.ReportWarning(LogMessage);

            // Call
            void Call() => logHandler.LogReport();

            // Assert
            string warning = Log4NetTestHelper.GetAllRenderedMessages(Call).Single();
            string expWarning = CreateExpectedLogMessage("During some_activity the following warnings were reported:",
                                                         LogMessage,
                                                         LogMessage,
                                                         "2 more warnings were not shown...");
            Assert.That(warning, Is.EqualTo(expWarning));
        }

        [Test]
        public void Constructor_NegativeMaxMessages_ThrowsArgumentOutOfRangeException()
        {
            // Call
            void Call() => new LogHandler(ActivityName, -1);

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("maxMessages"));
        }

        private static string CreateExpectedLogMessage(string header, string message1, string message2, string notShownMessage = null)
        {
            return header +
                   $"{Environment.NewLine}- {message1}" +
                   $"{Environment.NewLine}- {message2}" +
                   (notShownMessage == null ? string.Empty : Environment.NewLine + notShownMessage);
        }
    }
}