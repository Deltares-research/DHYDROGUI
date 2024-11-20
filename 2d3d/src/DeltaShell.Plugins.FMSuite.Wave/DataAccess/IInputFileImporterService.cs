namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// <see cref="IInputFileImporterService"/> defines an abstraction over handling
    /// files with respect to the input folder.
    /// </summary>
    public interface IInputFileImporterService
    {
        /// <summary>
        /// Determines whether a file with the specified <paramref name="fileName"/>
        /// already exists within the input folder.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// <c>true</c> if the specified file name already exists within the input
        /// folder; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="fileName"/> is <c>null</c> then <c>false</c> is returned.
        /// </remarks>
        bool HasFile(string fileName);

        /// <summary>
        /// Copies the specified <paramref name="sourceFilePath"/> to the input folder.
        /// </summary>
        /// <param name="sourceFilePath">The path to the source file name.</param>
        /// <param name="fileName">Optional name of the file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="sourceFilePath"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// If no <paramref name="fileName"/> is specified, it will use the file name of
        /// <paramref name="sourceFilePath"/>.
        /// <paramref name="sourceFilePath"/> and <paramref name="fileName"/> are expected
        /// to be valid path strings, if not the behaviour is undefined.
        /// </remarks>
        void CopyFile(string sourceFilePath, string fileName = null);

        /// <summary>
        /// Determines whether the specified file is within the current input folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="path"/> is in the input folder; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <paramref name="path"/> is first transformed into an absolute path. The paths are compared
        /// with <see cref="System.StringComparison.OrdinalIgnoreCase"/>.
        /// </remarks>
        bool IsInInputFolder(string path);

        /// <summary>
        /// Gets the absolute path of the specified <paramref name="relativePath"/> to the input folder. 
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>
        /// The absolute path equal to the <see cref="relativePath"/> to the input folder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="relativePath"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// It is assumed that <see cref="relativePath"/> is a valid path.
        /// </remarks>
        string GetAbsolutePath(string relativePath);

        /// <summary>
        /// Gets the path relative to the input folder.
        /// </summary>
        /// <param name="absolutePath">The absolute path.</param>
        /// <returns>
        /// The path relative to the input folder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="absolutePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="absolutePath"/> is not inside the input folder.
        /// </exception>
        string GetRelativePath(string absolutePath);
    }
}