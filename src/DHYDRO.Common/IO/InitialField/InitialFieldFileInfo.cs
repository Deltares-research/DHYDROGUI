using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// General information of an initial field file.
    /// </summary>
    public sealed class InitialFieldFileInfo
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileInfo"/> class.
        /// </summary>
        /// <param name="fileVersion"> The file version. </param>
        /// <param name="fileType"> The file type. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="fileVersion"/> or <paramref name="fileType"/> is <c>null</c> or white space.
        /// </exception>
        public InitialFieldFileInfo(string fileVersion, string fileType)
        {
            Ensure.NotNullOrWhiteSpace(fileVersion, nameof(fileVersion));
            Ensure.NotNullOrWhiteSpace(fileType, nameof(fileType));

            FileVersion = fileVersion;
            FileType = fileType;
        }

        /// <summary>
        /// The file version.
        /// </summary>
        public string FileVersion { get; }

        /// <summary>
        /// The file type.
        /// </summary>
        public string FileType { get; }
    }
}