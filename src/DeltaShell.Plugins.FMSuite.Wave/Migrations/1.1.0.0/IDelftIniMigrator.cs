using System.IO;
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
        /// Migrates the delft ini file contained in <paramref name="sourceFileStream"/> to the
        /// specified <paramref name="targetFilePath"/>.
        /// </summary>
        /// <param name="sourceFileStream">The source file.</param>
        /// <param name="sourceFilePath">The source file path to write to.</param>
        /// <param name="targetFilePath">The target file path to write to.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="sourceFileStream"/> or <paramref name="targetFilePath"/>
        /// are <c>null</c>.
        /// </exception>
        // TODO: add exceptions from writing here
        void MigrateFile(Stream sourceFileStream, 
                         string sourceFilePath,
                         string targetFilePath, 
                         ILogHandler logHandler);
    }
}