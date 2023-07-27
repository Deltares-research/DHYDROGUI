using System.Collections.Generic;
using System.Linq;

namespace DHYDRO.Common.Logging
{
    /// <inheritdoc/>
    public class LogMessages : ILogMessages
    {
        private readonly IList<LogMessage> logMessages = new List<LogMessage>();

        /// <inheritdoc/>
        public IEnumerable<string> InfoMessages => GetLogMessagesWithSeverity(LogSeverity.Info);

        /// <inheritdoc/>
        public IEnumerable<string> WarningMessages => GetLogMessagesWithSeverity(LogSeverity.Warning);

        /// <inheritdoc/>
        public IEnumerable<string> ErrorMessages => GetLogMessagesWithSeverity(LogSeverity.Error);

        /// <inheritdoc/>
        public IEnumerable<string> AllMessages => logMessages.Select(m => m.Message);

        /// <inheritdoc/>
        public void Add(string logMessage, LogSeverity logSeverity)
        {
            logMessages.Add(new LogMessage(logMessage, logSeverity));
        }

        /// <inheritdoc/>
        public void Clear() => logMessages.Clear();

        private IEnumerable<string> GetLogMessagesWithSeverity(LogSeverity logSeverity)
        {
            return logMessages.Where(p => p.Severity.Equals(logSeverity)).Select(m => m.Message);
        }

        private sealed class LogMessage
        {
            public LogMessage(string message, LogSeverity severity)
            {
                Message = message;
                Severity = severity;
            }

            public string Message { get; }
            public LogSeverity Severity { get; }
        }
    }
}