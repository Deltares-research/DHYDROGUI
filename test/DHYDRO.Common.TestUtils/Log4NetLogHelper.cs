using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Repository.Hierarchy;

namespace DHYDRO.Common.TestUtils
{
    /// <summary>
    /// Provides log4net helper methods.
    /// </summary>
    internal static class Log4NetLogHelper
    {
        /// <summary>
        /// Executes the provided action and captures the logging events generated during its execution.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="level">The logging level to filter events. If not provided, all events are captured.</param>
        /// <returns>An enumerable collection of <see cref="LoggingEvent"/> instances representing the captured logging events.</returns>
        /// <remarks>
        /// This method captures logging events using the current logging configuration.
        /// It's expected that a logging configuration is already set up before calling this method,
        /// and this method does not modify the existing logging configuration.
        /// </remarks>
        public static IEnumerable<LoggingEvent> GetLoggingEvents(Action action, Level level = null)
        {
            MemoryAppender tempAppender = CreateMemoryAppenderWithLevelFilter(level ?? Level.All);
            AddAppenderToRepositories(tempAppender);

            try
            {
                action();
                return tempAppender.GetEvents();
            }
            finally
            {
                tempAppender.Close();
                RemoveAppenderFromRepositories(tempAppender);
            }
        }

        private static MemoryAppender CreateMemoryAppenderWithLevelFilter(Level level)
        {
            var memoryAppender = new MemoryAppender();

            if (level != Level.All)
            {
                var levelFilter = new LevelMatchFilter { LevelToMatch = level };
                var denyAllFilter = new DenyAllFilter();

                memoryAppender.AddFilter(levelFilter);
                memoryAppender.AddFilter(denyAllFilter);
            }

            return memoryAppender;
        }

        private static void AddAppenderToRepositories(IAppender appender)
        {
            foreach (Hierarchy repository in LogManager.GetAllRepositories().OfType<Hierarchy>())
            {
                repository.Root.AddAppender(appender);
            }
        }

        private static void RemoveAppenderFromRepositories(IAppender appender)
        {
            foreach (Hierarchy repository in LogManager.GetAllRepositories().OfType<Hierarchy>())
            {
                repository.Root.RemoveAppender(appender);
            }
        }
    }
}