using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;

namespace DHYDRO.Common.Tests
{
    internal static class Log4NetTestHelper
    {
        public static void AssertLogMessagesCount(Action action, int count)
        {
            IEnumerable<string> renderedMessages = GetAllRenderedMessages(action);

            Assert.AreEqual(count, renderedMessages.Count());
        }

        public static IEnumerable<string> GetAllRenderedMessages(Action action)
        {
            return GetAllRenderedMessages(action, Level.All);
        }

        public static void AssertAtLeastOneLogMessagesContains(Action action, string expectedPartOfMessage)
        {
            IEnumerable<string> renderedMessages = GetAllRenderedMessages(action);
            if (!renderedMessages.Any(m => m.Contains(expectedPartOfMessage)))
            {
                Assert.Fail("Message part '{0}' was not found in the log4net messages.", expectedPartOfMessage);
            }
        }

        public static IEnumerable<string> GetAllRenderedMessages(Action action, Level logLevel)
        {
            var memoryAppender = new MemoryAppender();
            BasicConfigurator.Configure(memoryAppender);
            Log4NetLogHelper.SetLoggingLevel(logLevel);

            action();

            List<string> renderedMessages = memoryAppender.GetEvents().Select(le => le.RenderedMessage).ToList();

            memoryAppender.Close();
            Log4NetLogHelper.ResetLogging();

            return renderedMessages;
        }
    }
}