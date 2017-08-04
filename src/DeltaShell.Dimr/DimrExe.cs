using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BasicModelInterface;
using log4net;

namespace DeltaShell.Dimr
{
    [ExcludeFromCodeCoverage]
    public class DimrExe : IDimrApi
    {
        private readonly bool useMessagesBuffering;
        private static readonly ILog Log = LogManager.GetLogger(typeof(DimrExe));
        private string configFile;
        
        private int processId = -1;
        private readonly string[] messages;

        public DimrExe(bool useMessagesBuffering)
        {
            this.useMessagesBuffering = useMessagesBuffering;
            messages = new []{ string.Empty };
        }

        private void Run(string xmlFile)
        {
            var previousDir = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(xmlFile);
                Log.Info(string.Format("Running dimr in : {0}", Environment.CurrentDirectory));
                var dimrProcInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        ErrorDialog = false,
                        FileName = DimrApiDataSet.ExePath,
                        Arguments = Path.GetFileName(xmlFile)
                    };

                dimrProcInfo.EnvironmentVariables["PATH"] = KernelDirs + ";" +
                                                            Path.GetDirectoryName(DimrApiDataSet.ExePath) + ";" +
                                                            dimrProcInfo.EnvironmentVariables["PATH"];

                Log.Info(string.Format("Path used: {0}", dimrProcInfo.EnvironmentVariables["PATH"]));
                
                Log.Info(string.Format("Running dimr as : {0} {1}", dimrProcInfo.FileName, dimrProcInfo.Arguments));

                var infoMessages = new List<string>();
                var warnMessages = new List<string>();

                using (var process = new Process { StartInfo = dimrProcInfo })
                {
                    process.OutputDataReceived += (o, e) =>
                        {
                            if (e.Data == null) return;
                            if (useMessagesBuffering)
                            {
                                infoMessages.Add(e.Data);
                            }
                            else
                            {
                                Log.Info(e.Data);
                            }
                        };
                    process.ErrorDataReceived += (o, e) =>
                    {
                        if (e.Data == null) return;

                        var message = string.Format("Error occurred while running Dimr.exe: {0}", e.Data);
                        if (useMessagesBuffering)
                        {
                            warnMessages.Add(message);
                        }
                        else
                        {
                            Log.Warn(message);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    processId = process.Id;
                    
                    process.WaitForExit();
                    process.WaitForExit();

                    if (useMessagesBuffering)
                    {
                        infoMessages.ForEach(m => Log.Info(m));
                        warnMessages.ForEach(m => Log.Warn(m));
                    }

                    if (process.ExitCode > 0)
                    {
                        throw new Exception("dimr returned error code " + process.ExitCode);
                    }
                }
            }
            finally
            {
                processId = -1;
                Environment.CurrentDirectory = previousDir;
            }
        }

        public void Dispose()
        {
            //euhm..
            if (processId != -1)
            {
                Log.Info("Canceling dimr run");
                Process.GetProcessById(processId).Kill();
                processId = -1;
            }
        }

        public string KernelDirs { get; set; }
        public DateTime DimrRefDate { get; set; }

        public void set_feedback_logger()
        {
            
        }

        public int Initialize(string xmlFile)
        {
            configFile = xmlFile;
            if (!File.Exists(configFile))
            {
                //hmm call exporter or something...
            }
            StartTime = StopTime = CurrentTime = DimrRefDate;
            return 0;
        }

        public int Update(double step)
        {
            Run(configFile);
            
            return 0;
        }

        public int Finish()
        {
            // throw everything away? set output into projects? etc..
            return 0;
        }

        public int[] GetShape(string variable)
        {
            return new int[] {};
        }

        public Array GetValues(string variable)
        {
            return null;
        }

        public Array GetValues(string variable, int[] index)
        {
            return null;
        }

        public Array GetValues(string variable, int[] start, int[] count)
        {
            return null;
        }

        public void SetValues(string variable, Array values)
        {
        }

        public void SetValues(string variable, int[] start, int[] count, Array values)
        {
        }

        public void SetValues(string variable, int[] index, Array values)
        {
        }

        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }
        public DateTime CurrentTime { get; private set; }
        public TimeSpan TimeStep { get; private set; }
        public string[] VariableNames { get; private set; }
        public Logger Logger { get; set; }

        public string[] Messages
        {
            get { return messages; }
        }

        public void ProcessMessages()
        {
            //nope..
        }

        public void SetValuesDouble(string variable, double[] values)
        {
        }

        public void SetValuesInt(string variable, int[] values)
        {
        }

        public void SetLoggingLevel(string logType, Level level)
        {
        }
    }
}