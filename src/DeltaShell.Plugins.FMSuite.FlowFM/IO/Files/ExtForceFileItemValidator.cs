using System.IO.Abstractions;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// Validator for <see cref="ExtForceFileItem"/> object from the old external forcing file.
    /// </summary>
    public sealed class ExtForceFileItemValidator : IValidator<ExtForceFileItem>
    {
        private readonly FilePathValidator filePathValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="ExtForceFileItemValidator"/> class.
        /// </summary>
        /// <param name="referencePath">
        /// The reference path, which is the external forcing file or MDU file dependent on the
        /// PathsRelativeToParent option in the MDU.
        /// </param>
        /// <param name="parentFilePath"> The external forcing file path. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="parentFilePath"/> is <c>null</c>.
        /// </exception>
        public ExtForceFileItemValidator(string referencePath, string parentFilePath)
            : this(referencePath, parentFilePath, new FileSystem())
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="ExtForceFileItemValidator"/> class.
        /// </summary>
        /// <param name="referencePath">
        /// The reference path, which is the external forcing file or MDU file dependent on the
        /// PathsRelativeToParent option in the MDU.
        /// </param>
        /// <param name="parentFilePath"> The external forcing file path. </param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="parentFilePath"/> is <c>null</c>.
        /// </exception>
        public ExtForceFileItemValidator(string referencePath, string parentFilePath, IFileSystem fileSystem)
        {
            Ensure.NotNullOrWhiteSpace(referencePath, nameof(referencePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            filePathValidator = FilePathValidator.CreateDefault(referencePath, parentFilePath, fileSystem);
        }

        /// <summary>
        /// Validate the provided <see cref="ExtForceFileItem"/>.
        /// </summary>
        /// <param name="extForceFileItem">The external forcing file item to validate.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> indicating whether item is valid.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="extForceFileItem"/> is <c>null</c>.
        /// </exception>
        public ValidationResult Validate(ExtForceFileItem extForceFileItem)
        {
            Ensure.NotNull(extForceFileItem, nameof(extForceFileItem));
            return ValidateFileReference(extForceFileItem.FileName, ExtForceFileConstants.FileNameKey, extForceFileItem.LineNumber);
        }

        private ValidationResult ValidateFileReference(string fileReference, string propertyName, int lineNumber)
        {
            var filePathInfo = new FilePathInfo(fileReference, propertyName, lineNumber);
            return filePathValidator.Validate(filePathInfo);
        }
    }
}