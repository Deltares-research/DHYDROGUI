using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.IO.TestUtils
{
    /// <summary>
    /// A TestUtility class to create a temporary directory.
    /// </summary>
    /// <remarks>
    /// Constructing a new <see cref="TemporaryDirectory"/> will create a new
    /// temporary directory within the %TEMP% folder. This folder can be found
    /// at <see cref="Path"/>. Upon disposing this temporary directory, and all
    /// of its contents will be removed again from the file system.
    /// As such, the suggested usage of this class is:
    /// <code>
    /// using(var tempDir = new TemporaryDirectory())
    /// {
    ///     ...
    /// }
    /// </code>
    /// Keep in mind that if there exist any locked files within the temporary
    /// directory, an exception will be generated.
    /// </remarks>
    /// <seealso cref="IDisposable"/>
    public sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        /// Construct a new <see cref="TemporaryDirectory"/> with a randomised
        /// path within %TEMP%.
        /// </summary>
        public TemporaryDirectory()
        {
            Path = FileUtils.CreateTempDirectory();
        }

        /// <summary>
        /// Get the absolute path of this <see cref="TemporaryDirectory"/>.
        /// </summary>
        /// <value>
        /// The absolute path of this <see cref="TemporaryDirectory"/>.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Creates a new directory at the relative path.
        /// </summary>
        /// <param name="relativeDirPath">The relative directory path.</param>
        /// <returns>The full directory path of the created directory.</returns>
        public string CreateDirectory(string relativeDirPath)
        {
            string targetDirPath = System.IO.Path.Combine(Path, relativeDirPath);
            FileUtils.CreateDirectoryIfNotExists(targetDirPath);

            return targetDirPath;
        }

        /// <summary>
        /// Copies all test data to temporary directory.
        /// </summary>
        /// <param name="relativeTestDataFilePaths">The relative test data file paths.</param>
        /// <returns> List with file paths of copies in temp </returns>
        public List<string> CopyAllTestDataToTempDirectory(params string[] relativeTestDataFilePaths)
        {
            var copiesInTempFilePathList = new List<string>();

            foreach (string relativeTestDataFilePath in relativeTestDataFilePaths)
            {
                string copyInTempFilePath = CopyTestDataFileToTempDirectory(relativeTestDataFilePath);
                copiesInTempFilePathList.Add(copyInTempFilePath);
            }

            return copiesInTempFilePathList;
        }

        /// <summary>
        /// Copies the test data file to temporary directory.
        /// </summary>
        /// <param name="relativeTestDataFilePath">The relative test data file path.</param>
        /// <returns> file path of copy in temp</returns>
        public string CopyTestDataFileToTempDirectory(string relativeTestDataFilePath)
        {
            string sourceFilePath = TestHelper.GetTestFilePath(relativeTestDataFilePath);

            string fileName = System.IO.Path.GetFileName(relativeTestDataFilePath);
            string copyFilePath = System.IO.Path.Combine(Path, fileName);

            FileUtils.CopyFile(sourceFilePath, copyFilePath);

            return copyFilePath;
        }

        /// <summary>
        /// Copies a directory to the temporary directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>The path of the copied directory in the temporary directory.</returns>
        public string CopyDirectoryToTempDirectory(string directoryPath)
        {
            string directoryName = System.IO.Path.GetFileName(directoryPath);
            string targetDirPath = System.IO.Path.Combine(Path, directoryName);

            FileUtils.CopyDirectory(directoryPath, targetDirPath);

            return targetDirPath;
        }

        /// <summary>
        /// Copies the test data directory of a file to a temporary directory.
        /// </summary>
        /// <param name="relativeTestDataFilePath">The relative test data file path.</param>
        /// <returns> Directory path of copy in temp</returns>
        public string CopyTestDataFileAndDirectoryToTempDirectory(string relativeTestDataFilePath)
        {
            string sourceFilePath = TestHelper.GetTestFilePath(relativeTestDataFilePath);
            string sourceDirectoryPath = System.IO.Path.GetDirectoryName(sourceFilePath);

            string sourceDirectoryName = System.IO.Path.GetFileName(sourceDirectoryPath);
            string targetDirectoryPath = System.IO.Path.Combine(Path, sourceDirectoryName);

            FileUtils.CopyDirectory(sourceDirectoryPath, targetDirectoryPath);

            string sourceFileName = System.IO.Path.GetFileName(sourceFilePath);
            return System.IO.Path.Combine(targetDirectoryPath, sourceFileName);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        private void Dispose()
        {
            FileUtils.DeleteIfExists(Path);
        }
    }
}
