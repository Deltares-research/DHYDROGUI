using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    public static class WaterQualityUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterQualityUtils));

        /// <summary>
        /// Starts an executable
        /// </summary>
        /// <param name="exePath"> The path to the executable </param>
        /// <param name="parameters"> The run parameters </param>
        /// <param name="workDirectory"> The working directory while executing </param>
        /// <param name="useTimeOut"> Whether to use an execution time out or not </param>
        /// <param name="timeOut"> The time out in milliseconds (when <paramref name="useTimeOut"/> is true) </param>
        /// <param name="dataReceivedEventHandler"> EventHandler for output that is written to the console (default null) </param>
        /// <returns> Whether the execution ended successfully or not </returns>
        public static bool RunProcess(string exePath, string parameters, string workDirectory, Func<bool> isCanceled,
                                      bool useTimeOut = true, int timeOut = 3000,
                                      DataReceivedEventHandler dataReceivedEventHandler = null)
        {
            if (isCanceled == null)
            {
                isCanceled = () => false;
            }

            Log.InfoFormat("Starting process: '{0} {1}' from working directory '{2}'.", exePath, parameters,
                           workDirectory);

            var waqModelProcess = new Process
            {
                StartInfo =
                {
                    FileName = exePath,
                    Arguments = parameters,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    WorkingDirectory = workDirectory
                }
            };

            bool useOutputMonitoring = dataReceivedEventHandler != null;
            if (useOutputMonitoring)
            {
                waqModelProcess.StartInfo.RedirectStandardOutput = true;
                waqModelProcess.OutputDataReceived += dataReceivedEventHandler;
            }

            string currentDir = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(workDirectory);

            try
            {
                waqModelProcess.Start();

                if (useOutputMonitoring)
                {
                    waqModelProcess.BeginOutputReadLine();
                }

                var timePasted = 0;
                while (!waqModelProcess.HasExited)
                {
                    if (isCanceled() || useTimeOut && timePasted > timeOut)
                    {
                        break;
                    }

                    Thread.Sleep(500);
                    timePasted += 500;
                }

                if (useOutputMonitoring)
                {
                    waqModelProcess.OutputDataReceived -= dataReceivedEventHandler;
                }

                if (waqModelProcess.HasExited && waqModelProcess.ExitCode != 0)
                {
                    Log.ErrorFormat(
                        $"The process ({Path.GetFileNameWithoutExtension(exePath)}) has exited with error code {waqModelProcess.ExitCode}");
                    return false;
                }

                if (!isCanceled() && (!useTimeOut || timePasted <= timeOut))
                {
                    return true;
                }

                var closeMainWindow = false;
                try
                {
                    closeMainWindow = waqModelProcess.CloseMainWindow();
                }
                catch (Exception e) when (e is PlatformNotSupportedException || e is InvalidOperationException)
                {
                    Log.WarnFormat(e.Message);
                }
                finally
                {
                    if (!closeMainWindow && !waqModelProcess.HasExited)
                    {
                        Log.Warn(
                            $"Could not close process ({Path.GetFileNameWithoutExtension(exePath)}) normally, trying to kill the process. This might affect the model output.");
                        waqModelProcess.Kill();
                        Thread.Sleep(100); // wait for process to end
                    }
                }

                return false;
            }
            finally
            {
                waqModelProcess.Close();
                Directory.SetCurrentDirectory(currentDir);
            }
        }

        /// <summary>
        /// Trims all white space characters from the front and back of <paramref name="toBeTrimmed"/>
        /// </summary>
        /// <param name="toBeTrimmed"> The string to be trimmed </param>
        /// <returns> String where all white space characters from the front and back are removed </returns>
        // Note: In the future this method might be extended with logic that replaces white spaces by underscores
        public static string TrimString(string toBeTrimmed)
        {
            if (toBeTrimmed == null)
            {
                return null;
            }

            return toBeTrimmed.Trim();
        }
    }
}