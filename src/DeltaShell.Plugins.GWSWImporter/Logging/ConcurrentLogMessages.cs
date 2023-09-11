using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW.Logging
{
    /// <inheritdoc/>
    public class ConcurrentLogMessages : ILogMessages
    {
        private readonly ConcurrentStack<LogMessage> logMessages = new ConcurrentStack<LogMessage>();

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
            logMessages.Push(new LogMessage(logMessage, logSeverity));
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