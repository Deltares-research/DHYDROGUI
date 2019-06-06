using DelftTools.TestUtils;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.NGHS.TestUtils
{
    public static class LogTestHelper
    {
        /// <summary>
        /// Gets the log messages that are rendered during an action
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetRenderedMessages(Action action)
        {
            var memoryAppender = new MemoryAppender();
            BasicConfigurator.Configure(memoryAppender);
            LogHelper.SetLoggingLevel(Level.All);
            action();
            List<string> list = memoryAppender.GetEvents().Select(le => le.RenderedMessage).ToList();
            memoryAppender.Close();
            LogHelper.ResetLogging();
            return list;
        }
    }
}
