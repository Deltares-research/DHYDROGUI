
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Fnm
{
    /// <summary>
    /// Represents one file referenced in the *.fnm file.
    /// </summary>
    public sealed class FnmSubFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FnmSubFile"/> class.
        /// </summary>
        /// <param name="fileName"> The file name. </param>
        /// <param name="index"> The index of the file in the *.fnm file. </param>
        /// <param name="description"> The file description. </param>
        /// <param name="fileType"> The file type. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="fileName"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is a negative integer.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="description"/> is<c>null</c>
        /// </exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="fileType"/> is not a defined <see cref="FnmSubFileType"/>
        /// </exception>
        public FnmSubFile(string fileName, int index, string description, FnmSubFileType fileType)
        {
            Ensure.NotNullOrEmpty(fileName, nameof(fileName));
            Ensure.NotNegative(index, nameof(index));
            Ensure.NotNull(description, nameof(description));
            Ensure.IsDefined(fileType, nameof(fileType));

            FileName = fileName;
            Index = index;
            Description = description;
            FileType = fileType;
        }

        /// <summary>
        /// Gets or set the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets the file description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the file index
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the file type.
        /// </summary>
        public FnmSubFileType FileType { get; }
    }
}