using System;
using System.IO;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    /// <summary>
    /// <see cref="WaveEnvironmentHelper"/> provides a way of setting the
    /// environment to suit running wave models. Upon creation, the environment
    /// and working directory are modified. Upon disposing, the environment and
    /// working directory are restored to their original state.
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public sealed class WaveEnvironmentHelper : IDisposable
    {
        private readonly string previousPath;
        private readonly string previousArch;
        private readonly string previousWorkingDirectory;

        private readonly IEnvironment environment;

        /// <summary>
        /// Creates a new <see cref="WaveEnvironmentHelper"/>.
        /// </summary>
        /// <param name="workDir">The working directory to switch to.</param>
        public WaveEnvironmentHelper(string workDir) : this(workDir, new SystemEnvironment()) {}

        /// <summary>
        /// Creates a new <see cref="WaveEnvironmentHelper"/>.
        /// </summary>
        /// <param name="workDir">The working directory to switch to.</param>
        /// <param name="environment">The environment to interact with.</param>
        internal WaveEnvironmentHelper(string workDir, IEnvironment environment)
        {
            this.environment = environment;

            previousPath = environment.GetVariable(EnvironmentConstants.PathKey);
            previousArch = environment.GetVariable(WaveEnvironmentConstants.ArchKey);
            previousWorkingDirectory = Directory.GetCurrentDirectory();

            UpdateEnvironment();
            UpdateWorkingDirectory(workDir);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dimr run].
        /// </summary>
        public static bool DimrRun { get; set; } = false;

        private void UpdateEnvironment()
        {
            string modifiedPath = string.Join(";",
                                              DimrApiDataSet.WaveExeDirectory,
                                              DimrApiDataSet.SwanExeDirectory,
                                              DimrApiDataSet.EsmfExeDirectory,
                                              previousPath);

            environment.SetVariable(EnvironmentConstants.PathKey, modifiedPath);
            environment.SetVariable(WaveEnvironmentConstants.ArchKey,
                                    WaveEnvironmentConstants.ArchValue);
        }

        private void UpdateWorkingDirectory(string newWorkingDirectory)
        {
            if (!string.IsNullOrEmpty(newWorkingDirectory))
            {
                Directory.SetCurrentDirectory(newWorkingDirectory);
            }
        }

        private void RestoreEnvironment()
        {
            string archVariableKey = DimrRun
                                         ? WaveEnvironmentConstants.OldArchKey
                                         : WaveEnvironmentConstants.ArchKey;
            environment.SetVariable(archVariableKey, previousArch);
            environment.SetVariable(EnvironmentConstants.PathKey, previousPath);
        }

        private void RestoreWorkingDirectory()
        {
            if (!string.IsNullOrEmpty(previousWorkingDirectory))
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