using System;
using System.IO;
using DeltaShell.NGHS.Utils.Extensions;

namespace DeltaShell.NGHS.Common.IO.RestartFiles
{
    /// <summary>
    /// Represents a restart file.
    /// </summary>
    public sealed class RestartFile
    {
        // used to retrieve the original value that Path was set with.
        private string path;
        private FileInfo pathInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartFile"/> class.
        /// </summary>
        public RestartFile() : this(null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartFile"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> contains invalid characters such as ", &, >, or |, or if <paramref name="path"/> is empty.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified <paramref name="path"/>, file name, or both exceed the system-defined maximum length.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// Thrown when the caller does not have the required permission.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the access to the <paramref name="path"/> is denied.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when <paramref name="path"/> contains a colon (:) in the middle of the string.
        /// </exception>
        public RestartFile(string path) => Path = path;

        /// <summary>
        /// Gets the path.
        /// </summary>
        public string Path
        {
            get => path;
            private set
            {
                FileInfo newPathInfo = value != null
                                           ? new FileInfo(value)
                                           : null;

                pathInfo = newPathInfo;
                path = value;
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public string Name => pathInfo?.Name ?? string.Empty;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        public bool IsEmpty => Path == null;

        /// <summary>
        /// Gets a value indicating whether the restart file exists.
        /// </summary>
        public bool Exists
        {
            get
            {
                if (pathInfo == null)
                {
                    return false;
                }

                pathInfo.Refresh();
                return pathInfo.Exists;
            }
        }

        /// <summary>
        /// Copies the file in to the specified <paramref name="directoryPath"/>.
        /// </summary>
        /// <param name="directoryPath"> The destination directory. </param>
        /// <param name="switchTo">Whether this instance should be switched to the new path./></param>
        /// <exception cref="ArgumentException">
        /// Throws when <paramref name="directoryPath"/> contains invalid characters such as ", &, or |.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Throws when the specified <paramref name="directoryPath"/> exceeds the system-defined maximum length.
        /// </exception>
        /// <remarks>
        /// If <paramref name="directoryPath"/> is <c>null</c> or empty, the destination file path equals the current file path
        /// or this <see cref="RestartFile"/> does not exist, the method returns.
        /// </remarks>
        /// <remarks>The <paramref name="directoryPath"/> will be created without overwriting the existing one.</remarks>
        public void CopyToDirectory(string directoryPath, bool switchTo)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Exists)
            {
                return;
            }

            var dirInfo = new DirectoryInfo(directoryPath);
            FileInfo destinationFileInfo = pathInfo.CopyToDirectory(dirInfo, true);

            if (switchTo)
            {
                Path = destinationFileInfo.FullName;
            }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>
        /// A new copied instance of this instance.
        /// </returns>
        public RestartFile Clone() => new RestartFile(Path);

        public override string ToString() => Name;
    }
}