using System;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.IO.TestUtils
{
    /// <summary>
    /// A TestUtility class to create a temporary directory.
    /// </summary>
    /// <remarks>
    /// Constructing a new <see cref="TemporaryDirectory" /> will create a new
    /// temporary directory within the %TEMP% folder. This folder can be found
    /// at <see cref="Path" />. Upon disposing this temporary directory, and all
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
    /// <seealso cref="IDisposable" />
    public sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        /// Get the absolute path of this <see cref="TemporaryDirectory" />.
        /// </summary>
        /// <value>
        /// The absolute path of this <see cref="TemporaryDirectory" />.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Construct a new <see cref="TemporaryDirectory" /> with a randomised
        /// path within %TEMP%.
        /// </summary>
        public TemporaryDirectory()
        {
            Path = FileUtils.CreateTempDirectory();
        }

        private void Dispose()
        {
            FileUtils.DeleteIfExists(Path);
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}
