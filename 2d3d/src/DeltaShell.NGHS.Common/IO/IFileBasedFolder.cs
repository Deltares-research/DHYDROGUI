using System;
using System.ComponentModel;
using System.IO;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.Common.IO
{
    /// <summary>
    /// This interface represents a folder that may be located on disk.
    /// </summary>
    /// <seealso cref="DelftTools.Utils.IO.IFileBased"/>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
    public interface IFileBasedFolder : IFileBased, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the full path of the folder.
        /// </summary>
        /// <value>
        /// The full path.
        /// </value>
        string FullPath { get; }

        /// <summary>
        /// Indicates whether the folder at the <see cref="FullPath"/> of this <see cref="IFileBasedFolder"/> exists.
        /// </summary>
        /// <value>
        /// <c>true</c> if the folder exists; otherwise, <c>false</c>.
        /// </value>
        bool Exists { get; }

        /// <summary>
        /// Moves the content of the directory to the specified folder at <paramref name="destinationPath"/>
        /// </summary>
        /// <param name="destinationPath"> The destination path. </param>
        /// <param name="deleteIfExists"> Whether the destination folder should be deleted when it already exists. </param>
        /// <param name="switchTo">Whether this instance should be switched to <paramref name="destinationPath"/></param>
        /// <exception cref="ArgumentException">
        /// Throws when <paramref name="destinationPath"/> contains invalid characters such as ", &, or |.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Throws when the specified <paramref name="destinationPath"/> exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Throws when <paramref name="deleteIfExists"/> is <c>true</c> and the folder at the <seealso cref="Path"/>
        /// of the current instance is a subfolder of the folder at the specified <paramref name="destinationPath"/>
        /// or when the folder at the specified <paramref name="destinationPath"/> is a subfolder of the folder at
        /// the <see cref="Path"/> of the current instance.
        /// </exception>
        /// <remarks>If <paramref name="destinationPath"/> is <c>null</c> or empty or equals the current path, the method returns.</remarks>
        /// <remarks>
        /// If <paramref name="deleteIfExists"/> is <c>false</c> and the folder at <paramref name="destinationPath"/>
        /// exists, the source and destination directory are merged.
        /// </remarks>
        void MoveTo(string destinationPath, bool deleteIfExists, bool switchTo = false);

        /// <summary>
        /// Determines whether the folder contains a file with name <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"> Name of the file. </param>
        /// <param name="filePath">
        /// When this method returns <c> true </c>, it contains the full file path of the file with the
        /// specified <paramref name="fileName"/>.
        /// </param>
        /// <returns>
        /// <c> true </c> if the folder contains the file; otherwise, <c> false </c>.
        /// </returns>
        bool ContainsFile(string fileName, out string filePath);
    }
}