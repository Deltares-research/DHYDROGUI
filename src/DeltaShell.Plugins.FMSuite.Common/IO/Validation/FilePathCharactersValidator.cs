using System;
using System.IO.Abstractions;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.Common.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Validation
{
    /// <summary>
    /// Validates the characters in file paths.
    /// </summary>
    public sealed class FilePathCharactersValidator : IValidator<FilePathInfo>
    {
        private readonly IFileSystem fileSystem;
        private readonly string parentFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathExistenceValidator"/> class.
        /// </summary>
        /// <param name="parentFilePath">The path to the parent file that contains the file reference.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public FilePathCharactersValidator(string parentFilePath)
            : this(parentFilePath, new FileSystem())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathExistenceValidator"/> class.
        /// </summary>
        /// <param name="parentFilePath">The path to the parent file that contains the file reference.</param>
        /// <param name="fileSystem">Provides access to the file system. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileSystem"/> is <c>null</c>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public FilePathCharactersValidator(string parentFilePath, IFileSystem fileSystem)
        {
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.parentFilePath = parentFilePath;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Validates the file reference in the specified object for invalid characters.
        /// </summary>
        /// <param name="value">The file path information to validate.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> indicating whether the file reference contains invalid characters.
        /// When the file reference contains only valid characters, <see cref="ValidationResult.Valid"/> is <c>true</c>;
        /// Otherwise, <see cref="ValidationResult.Valid"/> is <c>false</c>
        /// and <see cref="ValidationResult.Message"/> contains the corresponding message.
        /// </returns>
        public ValidationResult Validate(FilePathInfo value)
        {
            return ContainsInvalidCharacters(value.FileReference) ? 
                       ValidationResult.Fail(GetMessageInvalidCharacters(value)) : 
                       ValidationResult.Success;
        }

        private bool ContainsInvalidCharacters(string fileReference)
        {
            Ensure.NotNullOrWhiteSpace(fileReference, nameof(fileReference));

            char[] invalidPathChars = fileSystem.Path.GetInvalidPathChars()
                                                .Concat(new[] { '*', '?' })
                                                .ToArray();

            return fileReference.IndexOfAny(invalidPathChars) >= 0;
        }

        private string GetMessageInvalidCharacters(FilePathInfo filePathInfo)
        {
            string message = string.Format(Resources.File_reference_0_contains_invalid_characters_but_is_defined_in_1_, filePathInfo.FileReference, parentFilePath) + Environment.NewLine +
                             string.Format(Resources.See_property_0_line_1_, filePathInfo.PropertyName, filePathInfo.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;

            return message;
        }
    }
}