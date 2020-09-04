using DeltaShell.NGHS.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0
{
    /// <summary>
    /// The <see cref="IDelftIniMigrator"/> is responsible for migrating a delft ini
    /// file from one directory to another.
    /// </summary>
    public interface IDelftIniMigrator
    {
        /// <summary>
        /// Migrates the delft ini file at <paramref name="srcFilePath"/> to the
        /// specified <paramref name="targetDirectory"/>.
        /// </summary>
        /// <param name="srcFilePath">The source file.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if any parameter is <c>null</c>.
        /// </exception>
        // TODO: add exceptions from writing here
        void MigrateFile(string srcFilePath, string targetDirectory, ILogHandler logHandler);
    }
}