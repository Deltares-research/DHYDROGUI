using System.IO;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Restart;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart
{
    /// <summary>
    /// A restart file used in the RTC model.
    /// </summary>
    [Entity]
    public class RealTimeControlRestartFile : Unique<long>, IRestartFile
    {
        private string name;

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/>.
        /// </summary>
        public RealTimeControlRestartFile()
            : this(string.Empty, null)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/> which is a copy of the source instance.
        /// </summary>
        /// <param name="source"> The source to clone the new instance from. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public RealTimeControlRestartFile(RealTimeControlRestartFile source)
        {
            Ensure.NotNull(source, nameof(source));

            name = source.name;
            Content = source.Content;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RealTimeControlRestartFile"/>.
        /// </summary>
        /// <param name="name">The name of the restart file.</param>
        /// <param name="content">The content of the restart file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="name"/> is <c>null</c>.</exception>
        public RealTimeControlRestartFile(string name, string content)
        {
            Ensure.NotNull(name, nameof(name));

            Name = name;
            Content = content;
        }

        #region IRestartFile
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when set <paramref name="value"/> is <c>null</c>.</exception>
        public string Name
        {
            get => name;
            set
            {
                Ensure.NotNull(value, nameof(value));

                name = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        public bool IsEmpty => Content == null;
        #endregion

        /// <summary>
        /// Gets the content of the restart file
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Create a new instance and set the Name and Content properties to filename and file content, respectively.
        /// </summary>
        /// <param name="filePath">The restart file path. </param>
        /// <returns>A new instance of <see cref="RealTimeControlRestartFile"/>.</returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="filePath" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="filePath" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        ///         <paramref name="filePath" /> specified a file that is read-only.
        /// -or-
        /// This operation is not supported on the current platform.
        /// -or-
        /// <paramref name="filePath" /> specified a directory.
        /// -or-
        /// The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="filePath" /> was not found.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="filePath" /> is in an invalid format.</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        public static RealTimeControlRestartFile CreateFromFile(string filePath)
        {
            Ensure.NotNull(filePath, nameof(filePath));
            return new RealTimeControlRestartFile(Path.GetFileName(filePath), File.ReadAllText(filePath));
        }
    
}
}