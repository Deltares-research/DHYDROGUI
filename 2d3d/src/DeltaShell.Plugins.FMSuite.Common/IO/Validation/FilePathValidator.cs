using System.IO.Abstractions;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Validation;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Validation
{
    /// <summary>
    /// Provides validation for file paths.
    /// </summary>
    public sealed class FilePathValidator : Validator<FilePathInfo>
    {
        /// <summary>
        /// Create a new default instance of the <see cref="FilePathValidator"/> class.
        /// The default validatior validates for:
        /// - <see cref="FilePathCharactersValidator"/>
        /// - <see cref="FilePathExistenceValidator"/>
        /// </summary>
        /// <param name="referencePath">The reference path used for each file path validation.</param>
        /// <param name="parentFilePath">The path to the parent file that contains the file reference.</param>
        /// <param name="fileSystem">Provides access to the file system. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileSystem"/> is <c>null</c>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public static FilePathValidator CreateDefault(string referencePath, string parentFilePath, IFileSystem fileSystem)
        {
            Ensure.NotNullOrWhiteSpace(referencePath, nameof(referencePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            var validator = new FilePathValidator();
            validator.AddValidator(new FilePathCharactersValidator(parentFilePath, fileSystem));
            validator.AddValidator(new FilePathExistenceValidator(referencePath, parentFilePath, fileSystem));

            return validator;
        }
    }
}