using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Validation
{
    /// <summary>
    /// Represents information about a file reference.
    /// </summary>
    public struct FilePathInfo
    {
        /// <summary>
        /// Initialize a new instance of <see cref="FilePathInfo"/>.
        /// </summary>
        /// <param name="fileReference">The file reference. Can be a relative path or an absolute path. </param>
        /// <param name="propertyName"> The name of the property that contains the file path. </param>
        /// <param name="lineNumber"> The line number of the property or section. </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="lineNumber"/> is a negative number.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="fileReference"/> or <paramref name="propertyName"/> is <c>null</c> or white space.
        /// </exception>
        public FilePathInfo(string fileReference, string propertyName, int lineNumber)
        {
            Ensure.NotNullOrWhiteSpace(fileReference, nameof(fileReference));
            Ensure.NotNullOrWhiteSpace(propertyName, nameof(propertyName));
            Ensure.NotNegative(lineNumber, nameof(lineNumber));

            FileReference = fileReference;
            PropertyName = propertyName;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// The file reference. Can be a relative path or an absolute path.
        /// </summary>
        public string FileReference { get; }

        /// <summary>
        /// The name of the corresponding property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The line number of the data in the file.
        /// </summary>
        public int LineNumber { get; }
    }
}