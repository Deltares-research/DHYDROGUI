using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;

namespace DeltaShell.NGHS.TestUtils
{
    public class LogAppenderEntriesTester : IDisposable
    {
        private readonly MemoryAppender appender;
        private readonly Logger logger;

        public LogAppenderEntriesTester(Type classType)
        {
            logger = (Logger) LogManager.GetLogger(classType).Logger;
            appender = new MemoryAppender();
            BasicConfigurator.Configure(appender);
        }

        public LogAppenderEntriesTester()
        {
            appender = new MemoryAppender();
            BasicConfigurator.Configure(appender);
        }

        public IEnumerable<string> Messages
        {
            get
            {
                return appender.GetEvents().Select(loggingEvent => loggingEvent.RenderedMessage).ToList();
            }
        }

        public void Dispose()
        {
            logger?.RemoveAppender(appender);
        }
    }
}