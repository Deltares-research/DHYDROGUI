using System;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.NGHS.Common.IO
{
    /// <summary>
    /// A manager that keeps a collection of files and
    /// provides methods to perform actions with the files.
    /// </summary>
    public interface IFilesManager
    {
        /// <summary>
        /// Adds the specified file path to this <see cref="FilesManager"/>.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="switchToAction">The action with which to update the model file path.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// Thrown when the caller does not have the required permission.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> is empty, contains only white spaces, or contains invalid characters.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when access to <paramref name="filePath"/> is denied.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// Thrown when <paramref name="filePath"/> exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when <paramref name="filePath"/> contains a colon (:) in the middle of the string.
        /// </exception>
        void Add(string filePath, Action<string> switchToAction);

        /// <summary>
        /// Copies the files to the specified directory at the specified <paramref name="targetPath"/>.
        /// </summary>
        /// <param name="targetPath">The destination directory.</param>
        /// <param name="logHandler">The log handler.</param>
        /// <param name="switchTo">Whether or not the model should be updated with the new file paths.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="targetPath"/> or <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// Thrown when the caller does not have the required permission.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="targetPath"/> contains invalid characters such as ", &lt;, &gt;, or |.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        void CopyTo(string targetPath, ILogHandler logHandler, bool switchTo);
    }
}