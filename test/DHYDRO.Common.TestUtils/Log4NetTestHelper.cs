using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using NUnit.Framework;

namespace DHYDRO.Common.TestUtils
{
    /// <summary>
    /// Provides helper methods for testing log messages with log4net.
    /// </summary>
    /// <remarks>
    /// The assertions capture logging messages using the current logging configuration.
    /// It's expected that a logging configuration is already set up before calling the assertions,
    /// and the assertions do not modify the existing logging configuration.
    /// </remarks>
    public static class Log4NetTestHelper
    {
        /// <summary>
        /// Asserts that the number of log messages matches the expected count for all logging levels.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="count">The expected count of log messages.</param>
        public static void AssertLogMessagesCount(Action action, int count)
        {
            AssertLogMessagesCount(action, Level.All, count);
        }

        /// <summary>
        /// Asserts that the number of log messages matches the expected count for the specified logging level.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="level">The logging level at which messages should be captured.</param>
        /// <param name="count">The expected count of log messages.</param>
        public static void AssertLogMessagesCount(Action action, Level level, int count)
        {
            IEnumerable<string> renderedMessages = GetAllRenderedMessages(action, level);

            Assert.AreEqual(count, renderedMessages.Count());
        }

        /// <summary>
        /// Asserts that at least one log message contains the expected part of the message for all logging levels.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="expectedPartOfMessage">The expected part of the log message.</param>
        public static void AssertAtLeastOneLogMessagesContains(Action action, string expectedPartOfMessage)
        {
            AssertAtLeastOneLogMessagesContains(action, Level.All, expectedPartOfMessage);
        }

        /// <summary>
        /// Asserts that at least one log message contains the expected part of the message for the specified logging level.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="level">The logging level at which messages should be captured.</param>
        /// <param name="expectedPartOfMessage">The expected part of the log message.</param>
        public static void AssertAtLeastOneLogMessagesContains(Action action, Level level, string expectedPartOfMessage)
        {
            IEnumerable<string> renderedMessages = GetAllRenderedMessages(action, level);

            if (!renderedMessages.Any(m => m.Contains(expectedPartOfMessage)))
            {
                Assert.Fail("Message part '{0}' was not found in the log4net messages.", expectedPartOfMessage);
            }
        }

        /// <summary>
        /// Executes the provided action and captures all rendered log messages.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>An enumerable collection of rendered log messages.</returns>
        public static IEnumerable<string> GetAllRenderedMessages(Action action)
        {
            return GetAllRenderedMessages(action, Level.All);
        }

        /// <summary>
        /// Executes the provided action and captures all rendered log messages with the specified logging level.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="level">The logging level at which messages should be captured.</param>
        /// <returns>An enumerable collection of rendered log messages.</returns>
        public static IEnumerable<string> GetAllRenderedMessages(Action action, Level level)
        {
            return Log4NetLogHelper.GetLoggingEvents(action, level).Select(x => x.RenderedMessage);
        }
    }
}