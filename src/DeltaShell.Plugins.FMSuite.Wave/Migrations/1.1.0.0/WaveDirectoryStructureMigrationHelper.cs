using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// <see cref="WaveDirectoryStructureMigrationHelper"/> acts as a facade
    /// to the Directory Structure migration associated with file format version
    /// 1.2.0.0.
    /// </summary>
    public static class WaveDirectoryStructureMigrationHelper
    {
        /// <summary>
        /// Migrates the specified wave model to the directory structure
        /// associated with file format version 1.2.0.0.
        /// </summary>
        /// <param name="waveModel">The wave model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveModel"/> is <c>null</c>.
        /// </exception>
        public static void Migrate(IWaveModel waveModel)
        {
            Ensure.NotNull(waveModel, nameof(waveModel));
        }
    }
}