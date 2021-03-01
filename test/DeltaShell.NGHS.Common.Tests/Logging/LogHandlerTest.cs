using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Logging;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Logging
{
    [TestFixture]
    public class LogHandlerTest
    {
        private const string ActivityName = "some_activity";
        private const string LogMessage = "some_log_message";
        private const string Format = "{0}_{1}_{2}";
        private LogHandler logHandler;

        private readonly object[] formatArgs =
        {
            "some",
            "log",
            "message"
        };

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler(ActivityName);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
        }

        [Test]
        public void GivenALogMessage_WhenReportInfoIsCalled_MessageIsAddedToLogMessagesTableWithInfoSeverity()
        {
            // When
            logHandler.ReportInfo(LogMessage);

            // Then
            AssertMessageWithCorrectSeverity(LogMessage, LogSeverity.Info);
        }

        [Test]
        public void GivenALogMessage_WhenReportWarningIsCalled_MessageIsAddedToLogMessagesTableWithWarningSeverity()
        {
            // When
            logHandler.ReportWarning(LogMessage);

            // Then
            AssertMessageWithCorrectSeverity(LogMessage, LogSeverity.Warning);
        }

        [Test]
        public void GivenALogMessage_WhenReportErrorIsCalled_MessageIsAddedToLogMessagesTableWithErrorSeverity()
        {
            // When
            logHandler.ReportError(LogMessage);

            // Then
            AssertMessageWithCorrectSeverity(LogMessage, LogSeverity.Error);
        }

        [Test]
        public void GivenALogMessage_WhenReportInfoFormatIsCalled_MessageIsAddedToLogMessagesTableWithInfoSeverity()
        {
            // When
            logHandler.ReportInfoFormat(Format, formatArgs);

            // Then
            AssertMessageWithCorrectSeverity(LogMessage, LogSeverity.Info);
        }

        [Test]
        public void GivenALogMessage_WhenReportWarningFormatIsCalled_MessageIsAddedToLogMessagesTableWithWarningSeverity()
        {
            // When
            logHandler.ReportWarningFormat(Format, formatArgs);

            // Then
            AssertMessageWithCorrectSeverity(LogMessage, LogSeverity.Warning);
        }

        [Test]
        public void GivenALogMessage_WhenReportErrorFormatIsCalled_MessageIsAddedToLogMessagesTableWithErrorSeverity()
        {
            // When
            logHandler.ReportErrorFormat(Format, formatArgs);

            // Then
            AssertMessageWithCorrectSeverity(LogMessage, LogSeverity.Error);
        }

        [Test]
        public void GivenALogHandlerWithoutLogMessages_WhenLogReportIsCalled_ThenNoReportIsLogged()
        {
            TestHelper.AssertLogMessagesCount(() => logHandler.LogReport(), 0);
        }

        [Test]
        public void GivenALogHandlerWithALogMessage_WhenLogReportIsCalled_ThenReportIsLoggedWithCorrectName()
        {
            // Given
            logHandler.ReportError(LogMessage);

            // When, Then
            TestHelper.AssertAtLeastOneLogMessagesContains(() => logHandler.LogReport(), ActivityName);
        }

        [Test]
        public void GivenALogHandlerWithALogMessage_WhenLogReportIsCalled_ThenReportIsLoggedWithCorrectMessage()
        {
            // Given
            logHandler.ReportError(LogMessage);

            // When, Then
            TestHelper.AssertAtLeastOneLogMessagesContains(() => logHandler.LogReport(), LogMessage);
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

            logHandler.ReportInfo(infoMessage1);
            logHandler.ReportInfo(infoMessage2);
            logHandler.ReportWarning(warningMessage1);
            logHandler.ReportWarning(warningMessage2);
            logHandler.ReportError(errorMessage1);
            logHandler.ReportError(errorMessage2);

            Assert.AreEqual(6, logHandler.LogMessagesTable.Count,
                            "Exactly 6 log messages were expected to be collected by the log handler.");

            // When
            void LogReportAction() => logHandler.LogReport();

            // Then
            string[] messages = TestHelper.GetAllRenderedMessages(LogReportAction).ToArray();
            Assert.That(messages[0], Is.EqualTo(CreateExpectedLogMessage("During some_activity the following errors were reported:", errorMessage1, errorMessage2)));
            Assert.That(messages[1], Is.EqualTo(CreateExpectedLogMessage("During some_activity the following warnings were reported:", warningMessage1, warningMessage2)));
            Assert.That(messages[2], Is.EqualTo(CreateExpectedLogMessage("During some_activity the following infos were reported:", infoMessage1, infoMessage2)));
            Assert.That(logHandler.LogMessagesTable, Is.Empty);
        }

        public void AssertMessageWithCorrectSeverity(string message, LogSeverity logSeverity)
        {
            Assert.AreEqual(1, logHandler.LogMessagesTable.Count,
                            "Exactly 1 log message was expected to be collected by the log handler.");

            Tuple<string, LogSeverity> messageSeverityPair = logHandler.LogMessagesTable
                                                                       .FirstOrDefault(t => t.Item1.Equals(message) && t.Item2.Equals(logSeverity));

            Assert.NotNull(messageSeverityPair,
                           $"Expected log message '{message}' with expected severity '{logSeverity.ToString()}' was not added to the list.");
        }

        private static string CreateExpectedLogMessage(string header, string message1, string message2)
        {
            return header +
                   $"{Environment.NewLine}- {message1}" +
                   $"{Environment.NewLine}- {message2}";
        }
    }
}