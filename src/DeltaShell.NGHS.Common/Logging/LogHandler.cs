using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DHYDRO.Common.Logging;
using log4net;

namespace DeltaShell.NGHS.Common.Logging
{
    /// <inheritdoc/>
    /// <seealso cref="T:DeltaShell.NGHS.Common.Logging.ILogHandler"/>
    public class LogHandler : ILogHandler
    {
        private const char bulletPointCharacter = '-';
        private readonly ILog log;
        private readonly string activityName;
        private readonly string joinSeparator = Environment.NewLine + bulletPointCharacter + " ";
        private readonly int maxMessages;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandler"/> class with a default logger.
        /// </summary>
        /// <param name="activityName">Name of the activity for which log messages will be generated.</param>
        /// <param name="maxMessages"> Optional; the maximum number of messages that should be logged in the report.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxMessages"/> is a negative integer.
        /// </exception>
        public LogHandler(string activityName, int maxMessages = int.MaxValue) : this(activityName, typeof(LogHandler), maxMessages) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandler"/> class.
        /// </summary>
        /// <param name="activityName">Name of the activity for which log messages will be generated.</param>
        /// <param name="type">The type that will be used to create the logger.</param>
        /// <param name="maxMessages"> Optional; the maximum number of messages that should be logged in the report.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxMessages"/> is a negative integer.
        /// </exception>
        public LogHandler(string activityName, Type type, int maxMessages = int.MaxValue) : this(activityName, LogManager.GetLogger(type), maxMessages) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandler"/> class.
        /// </summary>
        /// <param name="activityName">Name of the activity for which log messages will be generated.</param>
        /// <param name="log">The logger that will be used to log the messages.</param>
        /// <param name="maxMessages"> Optional; the maximum number of messages that should be logged in the report.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxMessages"/> is a negative integer.
        /// </exception>
        public LogHandler(string activityName, ILog log, int maxMessages = int.MaxValue)
        {
            Ensure.NotNegative(maxMessages, nameof(maxMessages));

            this.activityName = activityName;
            this.maxMessages = maxMessages;
            LogMessages = new LogMessages();
            this.log = log;
        }

        /// <inheritdoc/>
        public ILogMessages LogMessages { get; }

        /// <inheritdoc/>
        public void ReportInfo(string logMessage)
        {
            LogMessages.Add(logMessage, LogSeverity.Info);
        }

        /// <inheritdoc/>
        public void ReportInfoFormat(string logMessage, params object[] args)
        {
            ReportInfo(string.Format(logMessage, args));
        }

        /// <inheritdoc/>
        public void ReportWarning(string logMessage)
        {
            LogMessages.Add(logMessage, LogSeverity.Warning);
        }

        /// <inheritdoc/>
        public void ReportWarningFormat(string logMessage, params object[] args)
        {
            ReportWarning(string.Format(logMessage, args));
        }

        /// <inheritdoc/>
        public void ReportError(string logMessage)
        {
            LogMessages.Add(logMessage, LogSeverity.Error);
        }

        /// <inheritdoc/>
        public void ReportErrorFormat(string logMessage, params object[] args)
        {
            ReportError(string.Format(logMessage, args));
        }

        /// <inheritdoc/>
        public void LogReport()
        {
            if (!LogMessages.AllMessages.Any())
            {
                return;
            }

            string[] errorMessages = LogMessages.ErrorMessages.ToArray();
            if (errorMessages.Any())
            {
                log.Error(CreateReport(errorMessages, "errors"));
            }

            string[] warningMessages = LogMessages.WarningMessages.ToArray();
            if (warningMessages.Any())
            {
                log.Warn(CreateReport(warningMessages, "warnings"));
            }

            string[] infoMessages = LogMessages.InfoMessages.ToArray();
            if (infoMessages.Any())
            {
                log.Info(CreateReport(infoMessages, "infos"));
            }

            LogMessages.Clear();
        }

        private string CreateReport(IReadOnlyCollection<string> messages, string logSeverity)
        {
            string formattedMessages = GetFormattedMessages(messages.Take(maxMessages));
            string notShownMessage = GetNotShownMessage(messages.Count, logSeverity);
            return GetReportHeader(logSeverity) + formattedMessages + notShownMessage;
        }

        private string GetFormattedMessages(IEnumerable<string> logMessages)
        {
            return joinSeparator + string.Join(joinSeparator, logMessages);
        }

        private string GetNotShownMessage(int nMessages, string logSeverity)
        {
            int hiddenMessages = nMessages - maxMessages;
            return hiddenMessages > 0
                       ? $"{Environment.NewLine}{hiddenMessages} more {logSeverity} were not shown..."
                       : string.Empty;
        }

        private string GetReportHeader(string logSeverity)
        {
            return $"During {activityName} the following {logSeverity} were reported:";
        }
    }
}