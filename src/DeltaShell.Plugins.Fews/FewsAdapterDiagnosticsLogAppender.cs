using System;
using System.Collections.Generic;
using java.io;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using nl.wldelft.fews.pi;

namespace DeltaShell.Plugins.Fews
{
    public class FewsAdapterDiagnosticsAppender : IAppender
    {
        private static PiDiagnosticsWriter piDiagnosticsWriter;
        private static FewsAdapterDiagnosticsAppender instance;
        private static string outputDiagnosticsFile = "FEWS-DIAG-FILE-NOT-SET.xml";
        private static readonly string fewsNamespace = typeof(FewsAdapter).Namespace;
        private static bool doLogNonFewsDebugMessages;

        public static void SetOutputDiagnosticsFile(string diagnosticsFile)
        {
            outputDiagnosticsFile = diagnosticsFile;
        }

        public static void SetLoggingNonFewsDebugMessages(bool doLoggingOfNonFewsDebugMessages)
        {
            doLogNonFewsDebugMessages = doLoggingOfNonFewsDebugMessages;
        }

        private FewsAdapterDiagnosticsAppender(string diagFilePath)
        {
            piDiagnosticsWriter = new PiDiagnosticsWriter(new File(diagFilePath));
        }

        public void Close()
        {
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            if ( (loggingEvent.Level == Level.Info || loggingEvent.Level == Level.Debug) &&  
                !loggingEvent.LocationInformation.ClassName.StartsWith(fewsNamespace) &&
                !doLogNonFewsDebugMessages)
                return;

            // write into diagnostics ...
            try
            {
                string format = "ddd dd-MM-yyyy HH:mm:ss";
                IList<string> errorMessages = new List<string>();
                if (loggingEvent.ExceptionObject != null)
                {
                    string[] exceptionMessages = loggingEvent.ExceptionObject.Message.Split('\n');
                    foreach (string message in exceptionMessages)
                    {
                        errorMessages.Add(message.Trim());
                    }
                }
                else
                {
                    errorMessages.Add(loggingEvent.RenderedMessage);
                }
                foreach (string errorMessage in errorMessages)
                {
                    string message = DateTime.Now.ToString(format) + " " + errorMessage;
                    if (loggingEvent.Level == Level.Debug)
                    {
                        AddDebug(message);
                    }
                    if (loggingEvent.Level == Level.Info)
                    {
                        AddInfo(message);
                    }
                    if (loggingEvent.Level == Level.Warn)
                    {
                        AddWarning(message);
                    }
                    if (loggingEvent.Level == Level.Error)
                    {
                        AddError(message);
                    }
                    if (loggingEvent.Level == Level.Fatal)
                    {
                        AddFatal(message);
                    }
                }
            }
            catch (Exception e)
            {
                // swallow all exceptions
            }
        }

        public string Name { get; set; }

        public static void Initialize()
        {
            if (instance == null)
            {
                instance = new FewsAdapterDiagnosticsAppender(outputDiagnosticsFile);
            }
            else
            {
                return;
            }

            var log = LogManager.GetLogger(typeof (FewsAdapterDiagnosticsAppender));
            var logger = (Logger) log.Logger;
            var rootLogger = logger.Hierarchy.Root;

            rootLogger.AddAppender(instance);
        }

        public static void Flush()
        {
            // TODO: check action
        }

        public static void FlushAndFinish()
        {
            if (instance == null)
            {
                return;
            }
            // finish diagnostics
            if (piDiagnosticsWriter != null)
            {
                piDiagnosticsWriter.close();
                piDiagnosticsWriter = null;
            }
            instance = null;
        }

        public void AddWarning(string message)
        {
            piDiagnosticsWriter.writeLogEvent(PiLogLevel.WARN, message);
        }

        public void AddError(string message)
        {
            piDiagnosticsWriter.writeLogEvent(PiLogLevel.ERROR, message);
        }

        public void AddInfo(string message)
        {
            piDiagnosticsWriter.writeLogEvent(PiLogLevel.INFO, message);
        }

        public void AddDebug(string message)
        {
            piDiagnosticsWriter.writeLogEvent(PiLogLevel.DEBUG, message);
        }

        public void AddTrace(string message)
        {
            piDiagnosticsWriter.writeLogEvent(PiLogLevel.TRACE, message);
        }

        public void AddFatal(string message)
        {
            piDiagnosticsWriter.writeLogEvent(PiLogLevel.FATAL, message);
        }
    }
}