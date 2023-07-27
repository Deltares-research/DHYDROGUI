using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.Logging;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.Logging
{
    [TestFixture]
    public class LogMessagesTest
    {
        private const string LogMessage = "some_log_message";
        private const string InfoLogMessage = "info_log_message";
        private const string WarningLogMessage = "warning_log_message";
        private const string ErrorLogMessage = "error_log_message";
        private LogMessages logMessages;

        [SetUp]
        public void SetUp()
        {
            logMessages = new LogMessages();
        }

        [TearDown]
        public void TearDown()
        {
            logMessages = null;
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenInfoMessagesCalled_ThenCorrectMessageIsReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            List<string> infoMessages = logMessages.InfoMessages.ToList();

            // Then
            AssertCorrectMessageRetrieved(infoMessages, InfoLogMessage);
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenWarningMessagesCalled_ThenCorrectMessageIsReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            List<string> warningMessages = logMessages.WarningMessages.ToList();

            // Then
            AssertCorrectMessageRetrieved(warningMessages, WarningLogMessage);
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenErrorMessagesCalled_ThenCorrectMessageIsReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            List<string> errorMessages = logMessages.ErrorMessages.ToList();

            // Then
            AssertCorrectMessageRetrieved(errorMessages, ErrorLogMessage);
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenAllMessagesCalled_ThenAllMessagesAreReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            List<string> allMessages = logMessages.AllMessages.ToList();

            // Then
            Assert.AreEqual(3, allMessages.Count);
            AssertMessagesContain(allMessages, InfoLogMessage);
            AssertMessagesContain(allMessages, WarningLogMessage);
            AssertMessagesContain(allMessages, ErrorLogMessage);
        }

        [Test]
        public void GivenALogMessageWithInfoSeverity_WhenAddIsCalled_ThenANewTupleIsAddedToTheList()
        {
            logMessages.Add(LogMessage, LogSeverity.Info);

            Assert.AreEqual(1, logMessages.InfoMessages.Count(), "Exactly one info message should have been added to the LogMessages");
            Assert.AreEqual(LogMessage, logMessages.InfoMessages.Single(), "Log message was different than expected.");
        }

        [Test]
        public void GivenALogMessageWithWarningSeverity_WhenAddIsCalled_ThenANewTupleIsAddedToTheList()
        {
            logMessages.Add(LogMessage, LogSeverity.Warning);

            Assert.AreEqual(1, logMessages.WarningMessages.Count(), "Exactly one warning message should have been added to the LogMessages");
            Assert.AreEqual(LogMessage, logMessages.WarningMessages.Single(), "Log message was different than expected.");
        }

        [Test]
        public void GivenALogMessageWithErrorSeverity_WhenAddIsCalled_ThenANewTupleIsAddedToTheList()
        {
            logMessages.Add(LogMessage, LogSeverity.Error);

            Assert.AreEqual(1, logMessages.ErrorMessages.Count(), "Exactly one error message should have been added to the LogMessages");
            Assert.AreEqual(LogMessage, logMessages.ErrorMessages.Single(), "Log message was different than expected.");
        }

        private static void AssertMessagesContain(List<string> allMessages, string expectedMessage)
        {
            Assert.IsTrue(allMessages.Contains(expectedMessage),
                          $"Log message '{expectedMessage}' was expected to be in the list.");
        }

        private static void AssertCorrectMessageRetrieved(IList<string> logMessages, string expectedLogMessage)
        {
            Assert.AreEqual(1, logMessages.Count, "Exactly one message should have been retrieved.");
            string logMessage = logMessages.Single();
            Assert.AreEqual(expectedLogMessage, logMessage, "Log message was different than expected.");
        }

        private void AddMessagesWithAllSeveritiesToList()
        {
            logMessages.Add(InfoLogMessage, LogSeverity.Info);
            logMessages.Add(WarningLogMessage, LogSeverity.Warning);
            logMessages.Add(ErrorLogMessage, LogSeverity.Error);
        }
    }
}