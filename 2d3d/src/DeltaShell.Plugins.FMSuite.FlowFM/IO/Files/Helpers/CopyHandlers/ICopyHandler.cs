namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers
{
    /// <summary>
    /// <see cref="ICopyHandler"/> provides a generic interface for handling
    /// the copy operation.
    /// </summary>
    public interface ICopyHandler
    {
        /// <summary>
        /// Copies the file at <paramref name="sourcePath"/> to <paramref name="targetPath"/>.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="targetPath">The target path.</param>
        /// <exception cref="FileCopyException">
        /// Thrown when any exception occurs during copying.
        /// </exception>
        void Copy(string sourcePath, string targetPath);
    }
}