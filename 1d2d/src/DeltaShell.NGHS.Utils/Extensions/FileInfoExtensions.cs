using System.IO;

namespace DeltaShell.NGHS.Utils.Extensions
{
    /// <summary>
    /// Contains extensions methods for <see cref="FileInfo"/>.
    /// </summary>
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Copies the file to the specified directory.
        /// </summary>
        /// <param name="file"> The file info of the file that should be copied. </param>
        /// <param name="directory"> The directory info of the directory to which the file should be copied. </param>
        /// <param name="overwrite"> <c>true</c> to allow an existing file to be overwritten; otherwise, <c>false</c>.</param>
        /// <returns></returns>
        /// <exception cref="IOException">
        /// Thrown when the directory cannot be created, the copy action fails
        /// or when the file at the target location already exists while <paramref name="overwrite"/>
        /// is <c>false</c>.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// Thrown when the caller does not have the required permission.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        public static FileInfo CopyToDirectory(this FileInfo file, DirectoryInfo directory, bool overwrite)
        {
            if (directory.EqualsDirectory(file.Directory))
            {
                return file;
            }

            directory.Create();

            string copyFilePath = Path.Combine(directory.FullName, file.Name);
            return file.CopyTo(copyFilePath, overwrite);
        }
    }
}