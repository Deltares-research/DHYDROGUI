using System.Diagnostics;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    public static class WaterQualityUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterQualityUtils));

        /// <summary>
        /// Starts an executable
        /// </summary>
        /// <param name="exePath">The path to the executable</param>
        /// <param name="parameters">The run parameters</param>
        /// <param name="workDirectory">The working directory while executing</param>
        /// <param name="useTimeOut">Whether to use an execution time out or not</param>
        /// <param name="timeOut">The time out in milliseconds (when <paramref name="useTimeOut"/> is true)</param>
        /// <param name="dataReceivedEventHandler">EventHandler for output that is written to the console (default null)</param>
        /// <returns>Whether the execution ended successfully or not</returns>
        public static bool RunProcess(string exePath, string parameters, string workDirectory, bool useTimeOut = true, int timeOut = 3000, DataReceivedEventHandler dataReceivedEventHandler = null)
        {
            Log.InfoFormat("Starting process: '{0} {1}' from working directory '{2}'.", exePath, parameters, workDirectory);
            
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

            var useOutputMonitoring = dataReceivedEventHandler != null;
            if (useOutputMonitoring)
            {
                waqModelProcess.StartInfo.RedirectStandardOutput = true;
                waqModelProcess.OutputDataReceived += dataReceivedEventHandler;
            }

            var currentDir = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(workDirectory);

            try
            {
                waqModelProcess.Start();

                if (useOutputMonitoring)
                {
                    waqModelProcess.BeginOutputReadLine();
                }

                if (useTimeOut)
                {
                    if (!waqModelProcess.WaitForExit(timeOut))
                    {
                        waqModelProcess.Kill();
                        Log.Error("Model process hung.");

                        return false;
                    }
                }
                else
                {
                    waqModelProcess.WaitForExit();
                }

                if (useOutputMonitoring)
                {
                    waqModelProcess.OutputDataReceived -= dataReceivedEventHandler;
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDir);
            }

            return true;
        }

        /// <summary>
        /// Writes a resource if it not exists yet
        /// </summary>
        /// <param name="path">The path to write the resource to</param>
        /// <param name="resource">The resource to write to the file system</param>
        public static void WriteFileIfNotExists(string path, byte[] resource)
        {
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, resource);
            }
        }

        /// <summary>
        /// Trims all white space characters from the front and back of <paramref name="toBeTrimmed"/>
        /// </summary>
        /// <param name="toBeTrimmed">The string to be trimmed</param>
        /// <returns>String where all white space characters from the front and back are removed</returns>
        // Note: In the future this method might be extended with logic that replaces white spaces by underscores
        public static string TrimString(string toBeTrimmed)
        {
            if (toBeTrimmed == null) return null;
            return toBeTrimmed.Trim();
        }
    }
}