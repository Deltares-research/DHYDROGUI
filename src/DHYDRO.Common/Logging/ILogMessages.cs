using System.Collections.Generic;

namespace DHYDRO.Common.Logging
{
    /// <summary>
    /// Represents a container for log messages that can be added and retrieved.
    /// </summary>
    public interface ILogMessages
    {
        /// <summary>
        /// Gets all the log messages with severity <see cref="LogSeverity.Info"/>.
        /// </summary>
        IEnumerable<string> InfoMessages { get; }

        /// <summary>
        /// Gets all the log messages with severity <see cref="LogSeverity.Warning"/>.
        /// </summary>
        IEnumerable<string> WarningMessages { get; }

        /// <summary>
        /// Gets all the log messages with severity <see cref="LogSeverity.Error"/>.
        /// </summary>
        IEnumerable<string> ErrorMessages { get; }

        /// <summary>
        /// Gets all the log messages.
        /// </summary>
        IEnumerable<string> AllMessages { get; }

        /// <summary>
        /// Adds a new log message with the provided severity.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="logSeverity">The severity of the log message.</param>
        void Add(string logMessage, LogSeverity logSeverity);

        /// <summary>
        /// Clears all the log messages.
        /// </summary>
        void Clear();
    }
}