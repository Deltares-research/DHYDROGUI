using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Utils.IO;
using IOPath = System.IO.Path;

namespace DeltaShell.NGHS.Common.IO
{
    /// <summary>
    /// This class represents a folder may be located on disk.
    /// </summary>
    /// <seealso cref="IFileBasedFolder"/>
    public class FileBasedFolder : IFileBasedFolder
    {
        /// <remarks>
        /// Please do not remove this field. This field is necessary for NHibernate purposes.
        /// </remarks>
        private long id;

        private DirectoryInfo pathInfo;
        private string path;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>
        /// Please do not remove the virtual keyword. This keyword is necessary for NHibernate purposes.
        /// </remarks>
        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBasedFolder"/> class.
        /// </summary>
        public FileBasedFolder() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBasedFolder"/> class with the specified <paramref name="folderPath"/>.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="folderPath"/> contains invalid characters such as ", &, >, or |, or if <paramref name="folderPath"/> is
        /// empty.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        public FileBasedFolder(string folderPath)
        {
            Path = folderPath;
        }

        /// <summary>
        /// Gets the full path of the folder.
        /// </summary>
        /// <value>
        /// The full path.
        /// </value>
        public virtual string FullPath => pathInfo?.FullName;

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> contains invalid characters such as ", &, >, or |, or if <paramref name="value"/> is empty.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        public virtual string Path
        {
            get => path;
            set
            {
                DirectoryInfo newPathInfo = value != null
                                                ? new DirectoryInfo(value)
                                                : null;

                if (IsSamePath(newPathInfo))
                {
                    return;
                }

                pathInfo = newPathInfo;
                path = value;

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the paths.
        /// </summary>
        /// <value>
        /// The paths.
        /// </value>
        public virtual IEnumerable<string> Paths
        {
            get
            {
                yield return Path;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is file critical.
        /// </summary>
        /// <value>
        /// <c> true </c> if this instance is file critical; otherwise, <c> false </c>.
        /// </value>
        public virtual bool IsFileCritical => false;

        /// <summary>
        /// Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value>
        /// <c> true </c> if this instance is open; otherwise, <c> false </c>.
        /// </value>
        public virtual bool IsOpen => false;

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public virtual bool CopyFromWorkingDirectory { get; } = true;

        /// <summary>
        /// Indicates whether the folder at the <see cref="FullPath"/> of this <see cref="IFileBasedFolder"/> exists.
        /// </summary>
        /// <value>
        /// <c>true</c> if the folder exists; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Exists => pathInfo?.Exists ?? false;

        /// <summary>
        /// Copies the whole directory to the specified <paramref name="destinationPath"/>.
        /// </summary>
        /// <param name="destinationPath"> The destination path. </param>
        /// <exception cref="ArgumentException">
        /// Throws when <paramref name="destinationPath"/> contains invalid characters such as ", &, or |.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// Throws when the specified <paramref name="destinationPath"/> exceeds the system-defined maximum length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Throws when the folder at the <seealso cref="Path"/> of the current instance is a subfolder
        /// of the folder at the specified <paramref name="destinationPath"/>.
        /// </exception>
        /// <remarks>If <paramref name="destinationPath"/> is <c>null</c> or empty or equals the current path, the method returns.</remarks>
        /// <remarks>If the folder at <paramref name="destinationPath"/> exists, it will be deleted and replaced.</remarks>
        public virtual void CopyTo(string destinationPath)
        {
            if (string.IsNullOrEmpty(destinationPath))
            {
                return;
            }

            var destinationDirInfo = new DirectoryInfo(destinationPath);

            if (!CanPerformFileOperationAt(destinationDirInfo))
            {
                return;
            }

            if (IsSubFolderOf(pathInfo, destinationDirInfo))
            {
                throw new InvalidOperationException("Cannot delete destination folder when source folder is a subfolder of the destination folder.");
            }

            FileUtils.CreateDirectoryIfNotExists(destinationPath, true);

            FileUtils.CopyAll(pathInfo, destinationDirInfo, string.Empty);
        }

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
        public virtual void MoveTo(string destinationPath, bool deleteIfExists, bool switchTo = false)
        {
            if (string.IsNullOrEmpty(destinationPath))
            {
                return;
            }

            var destinationDirInfo = new DirectoryInfo(destinationPath);

            if (!CanPerformFileOperationAt(destinationDirInfo))
            {
                return;
            }

            if (deleteIfExists && IsSubFolderOf(pathInfo, destinationDirInfo))
            {
                throw new InvalidOperationException("Cannot delete destination folder when source folder is a subfolder of the destination folder.");
            }

            if (IsSubFolderOf(destinationDirInfo, pathInfo))
            {
                throw new InvalidOperationException("Cannot move source folder when destination folder is a subfolder of the source folder.");
            }

            string destinationFullPath = destinationDirInfo.FullName;

            FileUtils.CreateDirectoryIfNotExists(destinationFullPath, deleteIfExists);

            MoveTo(destinationDirInfo);

            if (switchTo)
            {
                SwitchTo(destinationFullPath);
            }
        }

        /// <summary>
        /// Switches to the specified <paramref name="newPath"/>
        /// </summary>
        /// <param name="newPath"> The new path. </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="newPath"/> contains invalid characters such as ", &, >, or |, or if <paramref name="newPath"/> is
        /// empty.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        public virtual void SwitchTo(string newPath)
        {
            Path = newPath;
        }

        /// <summary>
        /// Deletes the folder from disk at <see cref="Path"/>
        /// </summary>
        public virtual void Delete()
        {
            FileUtils.DeleteIfExists(Path);
        }

        public virtual void CreateNew(string path)
        {
            // This class should only reference a folder, not create a new one.
        }

        public virtual void Close()
        {
            // This method is irrelevant for folders.
        }

        public virtual void Open(string path)
        {
            // This method is irrelevant for folders.
        }

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
        public virtual bool ContainsFile(string fileName, out string filePath)
        {
            filePath = string.Empty;

            if (string.IsNullOrEmpty(fileName) || !pathInfo.Exists)
            {
                return false;
            }

            FileInfo retrievedFileInfo = pathInfo.EnumerateFiles(fileName, SearchOption.AllDirectories).FirstOrDefault();

            if (retrievedFileInfo != null)
            {
                filePath = retrievedFileInfo.FullName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MoveTo(DirectoryInfo destinationDirInfo)
        {
            bool onSameVolume = string.Equals(destinationDirInfo.Root.FullName,
                                              pathInfo.Root.FullName,
                                              StringComparison.OrdinalIgnoreCase);

            if (onSameVolume)
            {
                MoveAllTo(destinationDirInfo);
            }
            else
            {
                FileUtils.CopyAll(pathInfo, destinationDirInfo, string.Empty);
            }

            Delete();
        }

        private void MoveAllTo(DirectoryInfo destinationDir)
        {
            var srcAndDestDirQueue = new Queue<Tuple<DirectoryInfo, DirectoryInfo>>();
            srcAndDestDirQueue.Enqueue(new Tuple<DirectoryInfo, DirectoryInfo>(pathInfo, destinationDir));

            while (srcAndDestDirQueue.Any())
            {
                Tuple<DirectoryInfo, DirectoryInfo> srcAndDestDir = srcAndDestDirQueue.Dequeue();

                DirectoryInfo srcDir = srcAndDestDir.Item1;
                DirectoryInfo destDir = srcAndDestDir.Item2;

                if (!destDir.Exists)
                {
                    srcDir.MoveTo(destDir.FullName);
                }
                else
                {
                    foreach (FileInfo file in srcDir.EnumerateFiles())
                    {
                        string fileDestPath = IOPath.Combine(destDir.FullName, file.Name);
                        FileUtils.DeleteIfExists(fileDestPath);
                        file.MoveTo(fileDestPath);
                    }

                    foreach (DirectoryInfo directoryInSrc in srcDir.EnumerateDirectories())
                    {
                        string destDirPath = IOPath.Combine(destDir.FullName, directoryInSrc.Name);
                        srcAndDestDirQueue.Enqueue(new Tuple<DirectoryInfo, DirectoryInfo>(directoryInSrc, new DirectoryInfo(destDirPath)));
                    }
                }
            }
        }

        private bool CanPerformFileOperationAt(DirectoryInfo directoryInfo)
        {
            return Exists && !IsSamePath(directoryInfo);
        }

        private static bool IsSubFolderOf(DirectoryInfo subDir, DirectoryInfo parentDir)
        {
            DirectoryInfo parentDirectory = subDir.Parent;
            while (parentDirectory != null)
            {
                if (parentDirectory.FullName == parentDir.FullName)
                {
                    return true;
                }

                parentDirectory = parentDirectory.Parent;
            }

            return false;
        }

        private bool IsSamePath(DirectoryInfo directoryInfo) => pathInfo?.FullName == directoryInfo?.FullName;
    }
}