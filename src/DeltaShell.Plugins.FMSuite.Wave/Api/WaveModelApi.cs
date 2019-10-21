using System;
using System.IO;
using BasicModelInterface;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    /// <summary>
    /// Warning: This API should currently be used through the RemoteWaveModelApi only.
    /// </summary>
    public class WaveModelApi : IWaveModelApi
    {
        private string workingDirectory;

        /// <summary>
        /// Initializes the wave model, mdw file directory will be used to switch to
        /// </summary>
        /// <param name="mdwFilePath"> the full filepath of the mdw file </param>
        public int Initialize(string mdwFilePath)
        {
            workingDirectory = Path.GetDirectoryName(mdwFilePath);
            string mdwFileName = Path.GetFileName(mdwFilePath);
            using (new WaveDllHelper(workingDirectory))
            {
                WaveModelDll.initialize(mdwFileName);
            }

            return 0;
        }

        public int Update(double timestep)
        {
            using (new WaveDllHelper(workingDirectory))
            {
                WaveModelDll.update(timestep);
            }

            return 0;
        }

        public int Finish()
        {
            WaveModelDll.finalize();
            return 0;
        }

        public int[] GetShape(string variable)
        {
            return new int[]
                {};
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
            foreach (object value in values)
            {
                WaveModelDll.set_var(variable, value.ToString());
            }
        }

        public void SetValues(string variable, int[] start, int[] count, Array values) {}

        public void SetValues(string variable, int[] index, Array values) {}

        public DateTime StartTime
        {
            get
            {
                var t = 0.0;
                WaveModelDll.get_start_time(ref t);
                return ReferenceDateTime.AddSeconds(t);
            }
        }

        public DateTime StopTime { get; set; }

        public DateTime ReferenceDateTime { get; set; }

        public DateTime CurrentTime
        {
            get
            {
                var t = 0.0;
                WaveModelDll.get_current_time(ref t);
                return ReferenceDateTime.AddSeconds(t);
            }
        }

        public TimeSpan TimeStep { get; set; }
        public string[] VariableNames { get; set; }
        public Logger Logger { get; set; }

        public void Dispose() {}

        public class WaveDllHelper : IDisposable
        {
            private readonly string oldPath;
            private readonly string oldDelft3DDirectory;
            private readonly string oldArch;
            private readonly string originalWorkingDirectory;

            private readonly string waveExeDir;
            private readonly string swanExeDir;
            private readonly string swanScriptDir;
            private readonly string esmfPath;
            private readonly string esmfScriptPath;

            public WaveDllHelper(string workDir)
            {
                string d3DhomeDir = DimrApiDataSet.KernelsDirectory;

                waveExeDir = DimrApiDataSet.WaveExePath;
                swanExeDir = DimrApiDataSet.SwanExePath;
                swanScriptDir = DimrApiDataSet.SwanScriptPath;

                esmfPath = DimrApiDataSet.EsmfExePath;
                esmfScriptPath = DimrApiDataSet.EsmfScriptPath;

                oldPath = Environment.GetEnvironmentVariable("PATH");
                oldDelft3DDirectory = Environment.GetEnvironmentVariable("D3D_HOME");
                oldArch = Environment.GetEnvironmentVariable("ARCH");
                originalWorkingDirectory = Directory.GetCurrentDirectory();

                Environment.SetEnvironmentVariable("D3D_HOME", d3DhomeDir);
                Environment.SetEnvironmentVariable(
                    "PATH",
                    waveExeDir + ";" + swanExeDir + ";" + swanScriptDir + ";" + esmfPath + ";" + esmfScriptPath + ";" +
                    oldPath);
                Environment.SetEnvironmentVariable("ARCH", "x64", EnvironmentVariableTarget.Process);
                if (workDir != string.Empty)
                {
                    Directory.SetCurrentDirectory(workDir);
                }
            }

            public string WaveExeDir => waveExeDir;

            public string SwanExeDir => swanExeDir;

            public string SwanScriptDir => swanScriptDir;

            public string EsmfPath => esmfPath;

            public string EsmfScriptPath => esmfScriptPath;
            public static bool DimrRun { get; set; }

            public void Dispose()
            {
                if (DimrRun)
                {
                    Environment.SetEnvironmentVariable("OLD_D3D_HOME", oldDelft3DDirectory);
                    Environment.SetEnvironmentVariable("OLD_ARCH", oldArch, EnvironmentVariableTarget.Process);
                }
                else
                {
                    Environment.SetEnvironmentVariable("D3D_HOME", oldDelft3DDirectory);
                    Environment.SetEnvironmentVariable("ARCH", oldArch, EnvironmentVariableTarget.Process);
                }

                Environment.SetEnvironmentVariable("PATH", oldPath);
                if (originalWorkingDirectory != string.Empty)
                {
                    Directory.SetCurrentDirectory(originalWorkingDirectory);
                }
            }
        }
    }
}