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
        /// Thrown if <paramref name="sourceFileStream"/>,
        /// <paramref name="sourceFilePath"/>, or
        /// <paramref name="targetFilePath"/> are <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="sourceFileStream"/> does not support
        /// reading;
        /// Thrown when <paramref name="targetFilePath"/> contains invalid
        /// character, is empty or contains only white spaces.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        /// Thrown when access to <paramref name="targetFilePath"/> is denied.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// Thrown when <paramref name="targetFilePath"/> is invalid (for
        /// example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Thrown when <paramref name="targetFilePath"/> exceeds the
        /// system-defined maximum length. For example, on Windows-based
        /// platforms, paths must not exceed 248 characters, and file names
        /// must not exceed 260 characters.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when <paramref name="targetFilePath"/> includes an incorrect
        /// or invalid syntax for file name, directory name, or volume label
        /// syntax.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// Thrown when the caller does not have the required permission.
        /// </exception>
        void MigrateFile(Stream sourceFileStream, 
                         string sourceFilePath,
                         string targetFilePath, 
                         ILogHandler logHandler);
    }
}