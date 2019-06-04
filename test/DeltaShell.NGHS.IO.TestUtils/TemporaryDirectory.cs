using System;
using DelftTools.Utils.IO;


namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// A TestUtility class to create a temporary directory.
    /// </summary>
    /// <remarks>
    /// Constructing a new <see cref="TemporaryDirectory"/> will create a new
    /// temporary directory within the %TEMP% folder. This folder can be found
    /// at <see cref="Path"/>. Upon disposing this temporary directory, and all
    /// of its contents will be removed again from the file system.
    ///
    /// As such, the suggested usage of this class is:
    ///
    /// <code>
    /// using(var tempDir = new TemporaryDirectory())
    /// {
    ///     ...
    /// }
    /// </code>
    ///
    /// Keep in mind that if there exist any locked files within the temporary
    /// directory, an exception will be generated.
    /// </remarks>
    /// <seealso cref="System.IDisposable" />
    public sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; private set; }

        /// <summary>
        /// Construct a new <see cref="TemporaryDirectory"/> with a randomised
        /// path within %TEMP%.
        /// </summary>
        public TemporaryDirectory()
        {
            Path = FileUtils.CreateTempDirectory();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose()
        {
            if (disposedValue)
            {
                return;
            }

            FileUtils.DeleteIfExists(Path);
            Path = string.Empty;

            disposedValue = true;
        }

        ~TemporaryDirectory() {
            // Do not change this code. Put cleanup code in Dispose() above.
            Dispose();
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
