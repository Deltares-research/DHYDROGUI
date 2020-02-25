using System;
using System.IO;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    /// <summary>
    /// <see cref="WaveEnvironmentHelper"/> provides a way of setting the
    /// environment to suit running wave models. Upon creation, the environment
    /// and working directory are modified. Upon disposing, the environment and
    /// working directory are restored to their original state.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public sealed class WaveEnvironmentHelper : IDisposable
    {
        private readonly string previousPath;
        private readonly string previousArch;
        private readonly string previousWorkingDirectory;

        /// <summary>
        /// Creates a new <see cref="WaveEnvironmentHelper"/>.
        /// </summary>
        /// <param name="workDir">The working directory to switch to.</param>
        public WaveEnvironmentHelper(string workDir)
        {
            previousPath = Environment.GetEnvironmentVariable("PATH");
            previousArch = Environment.GetEnvironmentVariable("ARCH");
            previousWorkingDirectory = Directory.GetCurrentDirectory();

            UpdateEnvironment();
            UpdateWorkingDirectory(workDir);
        }

        private void UpdateEnvironment()
        {
            string modifiedPath = string.Join(";", 
                                              DimrApiDataSet.WaveExePath, 
                                              DimrApiDataSet.SwanExePath,
                                              DimrApiDataSet.SwanScriptPath, 
                                              DimrApiDataSet.EsmfExePath,
                                              DimrApiDataSet.EsmfScriptPath, 
                                              previousPath);

            Environment.SetEnvironmentVariable("PATH", modifiedPath);
            Environment.SetEnvironmentVariable("ARCH", "x64", EnvironmentVariableTarget.Process);
        }

        private void UpdateWorkingDirectory(string newWorkingDirectory)
        {
            if (!string.IsNullOrEmpty(newWorkingDirectory))
            {
                Directory.SetCurrentDirectory(newWorkingDirectory);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dimr run].
        /// </summary>
        public static bool DimrRun { get; set; } = false;

        private void RestoreEnvironment()
        {
            string archVariableKey = DimrRun ? "OLD_ARCH" : "ARCH";
            Environment.SetEnvironmentVariable(archVariableKey, previousArch, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", previousPath);
        }

        private void RestoreWorkingDirectory()
        {
            if (string.IsNullOrEmpty(previousWorkingDirectory))
            {
                Directory.SetCurrentDirectory(previousWorkingDirectory);
            }
        }

        #region IDisposable
        ~WaveEnvironmentHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                RestoreEnvironment();
                RestoreWorkingDirectory();
            }

            isDisposed = true;
        }

        private bool isDisposed;
        #endregion
    }
}