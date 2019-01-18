using System;
using System.IO;
using DelftTools.Utils.IO;


namespace DeltaShell.NGHS.TestUtils
{
    /// <inheritdoc />
    /// <summary>
    /// TemporaryDirectory provides a convenient way to generate directories within the TEMP folder using the using pattern.
    /// </summary>
    /// <remarks>
    /// TemporaryDirectory creates a new empty temporary directory upon construction. This temporary directory is deleted
    /// again, when the instance is being disposed of. The path to this directory is available through the Path variable.
    /// This class can be used as follows:
    /// 
    /// <c>
    /// using (var tempDir = new TemporaryDirectory()) // creates a new empty directory
    /// {
    ///     ...
    ///     someFunctionUsingTheTempDirPath(tempDir.Path);
    ///     ...
    /// } // disposes of the temporary directory through Dispose()
    /// </c>
    /// </remarks>
    /// <seealso cref="T:System.IDisposable" />
    public sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
        /// </summary>
        /// <remarks>
        /// This creates a new directory within the %TEMP% folder.
        /// </remarks>
        /// <exception cref="DirectoryNotFoundException"> No directory could be constructed. </exception>
        public TemporaryDirectory()
        {
            Path = FileUtils.CreateTempDirectory() ?? throw new DirectoryNotFoundException();
        }

        /// <summary>
        /// Gets the path of the temporary directory of this TemporaryDirectory.
        /// </summary>
        /// <value> The path to the temporary directory of this TemporaryDirectory. </value>
        /// <remarks> [NotNull] </remarks>
        public string Path { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// Removes the created temporary directory of this TemporaryDirectory.
        /// </summary>
        public void Dispose()
        {
            FileUtils.DeleteIfExists(Path);
        }
    }
}
