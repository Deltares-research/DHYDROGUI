using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace DHYDRO.Common.Tests
{
    public static class Log4NetLogHelper
    {
        /// <summary>
        /// Sets logging level for all current loggers to the level provided in arguments.
        /// Note: use it only when you need more control on logging, e.g. in unit tests. Otherwise use configuration files.
        /// </summary>
        /// <param name="level"></param>
        public static void SetLoggingLevel(Level level)
        {
            ILoggerRepository[] repositories = LogManager.GetAllRepositories();

            //Configure all loggers to be at the debug level.
            foreach (ILoggerRepository repository in repositories)
            {
                repository.Threshold = repository.LevelMap[level.ToString()];
                var hierarchy = (Hierarchy)repository;
                ILogger[] loggers = hierarchy.GetCurrentLoggers();
                foreach (ILogger logger in loggers)
                {
                    ((Logger)logger).Level = hierarchy.LevelMap[level.ToString()];
                }
            }

            //Configure the root logger.
            var h = (Hierarchy)LogManager.GetRepository();
            Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level.ToString()];
        }

        /// <summary>
        /// Resets logging configuration, no log messages are sent after that.
        /// </summary>
        public static void ResetLogging()
        {
            LogManager.ResetConfiguration();
            SetLoggingLevel(Level.Error);
        }
    }
}