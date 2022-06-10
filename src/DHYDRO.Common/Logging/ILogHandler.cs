namespace DHYDRO.Common.Logging
{
    /// <summary>
    /// Represents a handler for log messages and is responsible for keeping track of the log messages and their severity
    /// and logging them as one report.
    /// </summary>
    public interface ILogHandler
    {
        /// <summary>
        /// Get the log messages.
        /// </summary>
        ILogMessages LogMessages { get; }

        /// <summary>
        /// Adds a log message to the collection of log messages with severity <see cref="LogSeverity.Info"/>.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        void ReportInfo(string logMessage);

        /// <summary>
        /// Adds a formatted log message to the collection of log messages with severity <see cref="LogSeverity.Info"/>.
        /// </summary>
        /// <param name="logMessage">The log message containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        void ReportInfoFormat(string logMessage, params object[] args);

        /// <summary>
        /// Adds a log message to the collection of log messages with severity <see cref="LogSeverity.Warning"/>.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        void ReportWarning(string logMessage);

        /// <summary>
        /// Adds a formatted log message to the collection of log messages with severity <see cref="LogSeverity.Warning"/>.
        /// </summary>
        /// <param name="logMessage">The log message containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        void ReportWarningFormat(string logMessage, params object[] args);

        /// <summary>
        /// Adds a log message to the collection of log messages with severity <see cref="LogSeverity.Error"/>.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        void ReportError(string logMessage);

        /// <summary>
        /// Adds a formatted log message to the collection of log messages with severity <see cref="LogSeverity.Error"/>.
        /// </summary>
        /// <param name="logMessage">The log message containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        void ReportErrorFormat(string logMessage, params object[] args);

        /// <summary>
        /// Logs all the log messages as one report.
        /// </summary>
        void LogReport();
    }
}