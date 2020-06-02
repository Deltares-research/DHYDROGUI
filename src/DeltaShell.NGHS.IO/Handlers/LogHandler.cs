using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace DeltaShell.NGHS.IO.Handlers
{
    /// <inheritdoc/>
    /// <seealso cref="T:DeltaShell.NGHS.IO.Handlers.ILogHandler"/>
    public class LogHandler : ILogHandler
    {
        private const char BulletPointCharacter = '-';
        private readonly ILog log;

        private readonly string activityName;

        private readonly string joinSeparator = Environment.NewLine + BulletPointCharacter + " ";

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandler"/> class with a default logger.
        /// </summary>
        /// <param name="activityName">Name of the activity for which log messages will be generated.</param>
        public LogHandler(string activityName) : this(activityName, typeof(LogHandler)) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandler"/> class.
        /// </summary>
        /// <param name="activityName">Name of the activity for which log messages will be generated.</param>
        /// <param name="type">The type that will be used to create the logger.</param>
        public LogHandler(string activityName, Type type) : this(activityName, LogManager.GetLogger(type)) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandler"/> class.
        /// </summary>
        /// <param name="activityName">Name of the activity for which log messages will be generated.</param>
        /// <param name="log">The logger that will be used to log the messages.</param>
        public LogHandler(string activityName, ILog log)
        {
            this.activityName = activityName;
            LogMessagesTable = new LogMessagesList();
            this.log = log;
        }

        public LogMessagesList LogMessagesTable { get; }

        public void ReportInfo(string logMessage)
        {
            LogMessagesTable.Add(logMessage, LogSeverity.Info);
        }

        public void ReportInfoFormat(string logMessage, params object[] args)
        {
            ReportInfo(string.Format(logMessage, args));
        }

        public void ReportWarning(string logMessage)
        {
            LogMessagesTable.Add(logMessage, LogSeverity.Warning);
        }

        public void ReportWarningFormat(string logMessage, params object[] args)
        {
            ReportWarning(string.Format(logMessage, args));
        }

        public void ReportError(string logMessage)
        {
            LogMessagesTable.Add(logMessage, LogSeverity.Error);
        }

        public void ReportErrorFormat(string logMessage, params object[] args)
        {
            ReportError(string.Format(logMessage, args));
        }

        public void LogReport()
        {
            if (!LogMessagesTable.Any())
            {
                return;
            }

            List<string> errorMessages = LogMessagesTable.ErrorMessages.ToList();
            if (errorMessages.Any())
            {
                log.Error(CreateReport(errorMessages, "errors"));
            }

            List<string> warningMessages = LogMessagesTable.WarningMessages.ToList();
            if (warningMessages.Any())
            {
                log.Warn(CreateReport(warningMessages, "warnings"));
            }

            List<string> infoMessages = LogMessagesTable.InfoMessages.ToList();
            if (infoMessages.Any())
            {
                log.Info(CreateReport(infoMessages, "infos"));
            }
        }

        private string CreateReport(IEnumerable<string> messages, string logSeverity)
        {
            string formattedMessages = GetFormattedMessages(messages);
            return GetReportHeader(logSeverity) + formattedMessages;
        }

        private string GetFormattedMessages(IEnumerable<string> logMessages)
        {
            return joinSeparator + string.Join(joinSeparator, logMessages);
        }

        private string GetReportHeader(string logSeverity)
        {
            return $"During {activityName} the following {logSeverity} were reported:";
        }
    }

    /// <summary>
    /// Severities of log messages.
    /// </summary>
    public enum LogSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <inheritdoc/>
    /// <summary>
    /// Represent a list of <see cref="T:System.String"/> and <see cref="T:DeltaShell.NGHS.IO.Handlers.LogSeverity"/> pairs.
    /// </summary>
    /// <seealso cref="T:System.Collections.Generic.List`1"/>
    public class LogMessagesList : List<Tuple<string, LogSeverity>>
    {
        /// <summary>
        /// Gets all the log messages with severity <see cref="LogSeverity.Info"/>.
        /// </summary>
        /// <value>
        /// The info messages.
        /// </value>
        public IEnumerable<string> InfoMessages => GetLogMessagesWithSeverity(LogSeverity.Info);

        /// <summary>
        /// Gets all the log messages with severity <see cref="LogSeverity.Warning"/>.
        /// </summary>
        /// <value>
        /// The info messages.
        /// </value>
        public IEnumerable<string> WarningMessages => GetLogMessagesWithSeverity(LogSeverity.Warning);

        /// <summary>
        /// Gets all the log messages with severity <see cref="LogSeverity.Error"/>.
        /// </summary>
        /// <value>
        /// The info messages.
        /// </value>
        public IEnumerable<string> ErrorMessages => GetLogMessagesWithSeverity(LogSeverity.Error);

        /// <summary>
        /// Gets all the messages the <see cref="LogMessagesList"/> contains.
        /// </summary>
        /// <value>
        /// The log messages.
        /// </value>
        public IEnumerable<string> AllMessages => this.Select(m => m.Item1);

        /// <summary>
        /// Adds a new pair of <see cref="System.String"/> and <see cref="LogSeverity"/> to the end of the
        /// <see cref="LogMessagesList"/>.
        /// </summary>
        /// <param name="logMessage">The log message</param>
        /// <param name="logSeverity">The severity of the log message</param>
        public void Add(string logMessage, LogSeverity logSeverity)
        {
            Add(new Tuple<string, LogSeverity>(logMessage, logSeverity));
        }

        private IEnumerable<string> GetLogMessagesWithSeverity(LogSeverity logSeverity)
        {
            return this.Where(p => p.Item2.Equals(logSeverity)).Select(m => m.Item1);
        }
    }
}