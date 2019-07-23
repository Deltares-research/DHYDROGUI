using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Handlers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Handlers
{
    [TestFixture]
    public class LogMessagesListTest
    {
        private LogMessagesList logMessagesList;
        private const string LogMessage = "some_log_message";
        private const string InfoLogMessage = "info_log_message";
        private const string WarningLogMessage = "warning_log_message";
        private const string ErrorLogMessage = "error_log_message";

        [SetUp]
        public void SetUp()
        {
            logMessagesList = new LogMessagesList();
        }

        [TearDown]
        public void TearDown()
        {
            logMessagesList = null;
        }

        [TestCase(LogSeverity.Info)]
        [TestCase(LogSeverity.Warning)]
        [TestCase(LogSeverity.Error)]
        public void GivenALogMessageAndASeverity_WhenAddIsCalled_ThenANewTupleIsAddedToTheList(LogSeverity logSeverity)
        {
            logMessagesList.Add(LogMessage, logSeverity);

            Assert.AreEqual(1, logMessagesList.Count, "Exactly one tuple should have been added to the LogMessageList");
            Assert.AreEqual(LogMessage, logMessagesList.Single().Item1, "Log message was different than expected.");
            Assert.AreEqual(logSeverity, logMessagesList.Single().Item2, "Log severity was different than expected.");
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenInfoMessagesCalled_ThenCorrectMessageIsReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            var infoMessages = logMessagesList.InfoMessages.ToList();

            // Then
            AssertCorrectMessageRetrieved(infoMessages, InfoLogMessage);
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenWarningMessagesCalled_ThenCorrectMessageIsReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            var warningMessages = logMessagesList.WarningMessages.ToList();

            // Then
            AssertCorrectMessageRetrieved(warningMessages, WarningLogMessage);
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenErrorMessagesCalled_ThenCorrectMessageIsReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            var errorMessages = logMessagesList.ErrorMessages.ToList();

            // Then
            AssertCorrectMessageRetrieved(errorMessages, ErrorLogMessage);
        }

        [Test]
        public void GivenMessagesWithAllSeveritiesInList_WhenAllMessagesCalled_ThenAllMessagesAreReturned()
        {
            // Given
            AddMessagesWithAllSeveritiesToList();

            // When
            var allMessages = logMessagesList.AllMessages.ToList();

            // Then
            Assert.AreEqual(3, allMessages.Count);
            AssertMessagesContain(allMessages, InfoLogMessage);
            AssertMessagesContain(allMessages, WarningLogMessage);
            AssertMessagesContain(allMessages, ErrorLogMessage);
        }

        private static void AssertMessagesContain(List<string> allMessages, string expectedMessage)
        {
            Assert.IsTrue(allMessages.Contains(expectedMessage),
                $"Log message '{expectedMessage}' was expected to be in the list.");
        }

        private static void AssertCorrectMessageRetrieved(IList<string> logMessages, string expectedLogMessage)
        {
            Assert.AreEqual(1, logMessages.Count, "Exactly one message should have been retrieved.");
            var logMessage = logMessages.Single();
            Assert.AreEqual(expectedLogMessage, logMessage, "Log message was different than expected.");
        }

        private void AddMessagesWithAllSeveritiesToList()
        {
            logMessagesList.Add(new Tuple<string, LogSeverity>(InfoLogMessage, LogSeverity.Info));
            logMessagesList.Add(new Tuple<string, LogSeverity>(WarningLogMessage, LogSeverity.Warning));
            logMessagesList.Add(new Tuple<string, LogSeverity>(ErrorLogMessage, LogSeverity.Error));
        }
    }
}
