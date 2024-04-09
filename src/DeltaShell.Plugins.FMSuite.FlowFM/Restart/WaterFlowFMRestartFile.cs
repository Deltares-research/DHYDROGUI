using System;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Restart;
using DeltaShell.Plugins.FMSuite.Common.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Restart
{
    /// <summary>
    /// Represents a restart file.
    /// </summary>
    public sealed class WaterFlowFMRestartFile : IRestartFile
    {
        // used to retrieve the original value that Path was set with.
        private string path;
        private FileInfo pathInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterFlowFMRestartFile"/> class.
        /// </summary>
        public WaterFlowFMRestartFile()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WaterFlowFMRestartFile"/> class which is a copy of the source instance.
        /// </summary>
        /// <param name="source"> The source to clone the new instance from. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public WaterFlowFMRestartFile(WaterFlowFMRestartFile source)
        {
            Ensure.NotNull(source, nameof(source));

            path = source.path;
            pathInfo = source.pathInfo;
            StartTime = source.StartTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaterFlowFMRestartFile"/> class.
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
        public WaterFlowFMRestartFile(string path) => Path = path;

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
        /// Gets or sets the start time of this restart file.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets whether or not the restart file is a map file.
        /// </summary>
        public bool IsMapFile => Name.EndsWith(FileConstants.MapFileExtension);

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// Copies the file to the specified <paramref name="destinationPath"/>.
        /// </summary>
        /// <param name="destinationPath"> The destination directory. </param>
        /// <param name="switchTo">Whether this instance should be switched to the new path./></param>
        /// <exception cref="ArgumentException">
        /// Throws when <paramref name="destinationPath"/> contains invalid characters such as ", &, or |.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Throws when the specified <paramref name="destinationPath"/> exceeds the system-defined maximum length.
        /// </exception>
        /// <remarks>
        /// If <paramref name="destinationPath"/> is <c>null</c> or empty, equals the current file path
        /// or this <see cref="WaterFlowFMRestartFile"/> does not exist, the method returns.
        /// </remarks>
        /// <remarks>
        /// The target directory of <paramref name="destinationPath"/> will be created without
        /// overwriting the existing one.
        /// </remarks>
        public void CopyTo(string destinationPath, bool switchTo)
        {
            if (string.IsNullOrEmpty(destinationPath) || !Exists)
            {
                return;
            }
            
            var destinationFileInfo = new FileInfo(destinationPath);

            CreateParentDirectory(destinationFileInfo);

            if (IsSamePath(destinationFileInfo))
            {
                return;
            }

            pathInfo.CopyTo(destinationFileInfo.FullName, true);

            if (switchTo)
            {
                Path = destinationFileInfo.FullName;
            }
        }

        private static void CreateParentDirectory(FileInfo destinationFileInfo)
        {
            DirectoryInfo parentDirInfo = destinationFileInfo.Directory;
            if (parentDirInfo == null)
            {
                throw new InvalidOperationException("The file cannot map to a drive.");
            }

            parentDirInfo.Create();
        }

        private bool IsSamePath(FileInfo fileInfo) => pathInfo?.FullName == fileInfo?.FullName;
    }
}