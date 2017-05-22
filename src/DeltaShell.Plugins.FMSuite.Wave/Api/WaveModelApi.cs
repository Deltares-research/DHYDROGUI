using System;
using System.IO;
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
        /// <param name="mdwFilePath">the full filepath of the mdw file</param>
        public void Initialize(string mdwFilePath)
        {
            workingDirectory = Path.GetDirectoryName(mdwFilePath);
            var mdwFileName = Path.GetFileName(mdwFilePath);
            using (new WaveDllHelper(workingDirectory))
            {
                WaveModelDll.initialize(mdwFileName);
            }
        }

        public void Update(double timestep)
        {
            using (new WaveDllHelper(workingDirectory))
            {
                WaveModelDll.update(timestep);
            }
        }

        public void Finish()
        {
            WaveModelDll.finalize();
        }

        public void SetVar(string variable, string value)
        {
            WaveModelDll.set_var(variable, value);
        }
        
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

        public void Dispose()
        {
        }

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
                var arch = WaveModelDll.Arch;
                
                var d3DhomeDir = DimrApiDataSet.DllDirectory;

                waveExeDir = Path.Combine(d3DhomeDir, arch, "wave", "bin");
                swanExeDir = Path.Combine(d3DhomeDir, arch, "swan","bin");
                swanScriptDir = Path.Combine(d3DhomeDir, arch, "swan", "scripts");

                esmfPath = Path.Combine(d3DhomeDir, arch, "esmf", "bin");
                esmfScriptPath = Path.Combine(d3DhomeDir, arch, "esmf", "scripts");

                oldPath = Environment.GetEnvironmentVariable("PATH");
                oldDelft3DDirectory = Environment.GetEnvironmentVariable("D3D_HOME");
                oldArch = Environment.GetEnvironmentVariable("ARCH");
                originalWorkingDirectory = Directory.GetCurrentDirectory();

                DimrRun = DimrRun && DimrRun;

                Environment.SetEnvironmentVariable("D3D_HOME", d3DhomeDir);
                Environment.SetEnvironmentVariable("PATH", waveExeDir + ";" + swanExeDir + ";" + swanScriptDir + ";" + esmfPath + ";" + esmfScriptPath + ";" + oldPath);
                Environment.SetEnvironmentVariable("ARCH", arch, EnvironmentVariableTarget.Process);
                if (workDir != string.Empty)
                    Directory.SetCurrentDirectory(workDir);
            }
            
            public string WaveExeDir
            {
                get { return waveExeDir; }
            }

            public string SwanExeDir
            {
                get { return swanExeDir; }
            }

            public string SwanScriptDir
            {
                get { return swanScriptDir; }
            }

            public string EsmfPath
            {
                get { return esmfPath; }
            }

            public string EsmfScriptPath
            {
                get { return esmfScriptPath; }
            }
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
                    Directory.SetCurrentDirectory(originalWorkingDirectory);
            }
        }
    }
}