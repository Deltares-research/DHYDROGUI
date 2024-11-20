using System;
using System.IO.Abstractions;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Validation;
using Deltares.Infrastructure.IO;
using DeltaShell.Plugins.FMSuite.Common.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Validation
{
    /// <summary>
    /// Validates for the existence of file paths.
    /// </summary>
    public sealed class FilePathExistenceValidator : IValidator<FilePathInfo>
    {
        private readonly IFileSystem fileSystem;
        private readonly string parentFilePath;
        private readonly string referencePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathExistenceValidator"/> class.
        /// </summary>
        /// <param name="referencePath">The reference path used for each file path validation.</param>
        /// <param name="parentFilePath">The path to the parent file that contains the file reference.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public FilePathExistenceValidator(string referencePath, string parentFilePath)
            : this(referencePath, parentFilePath, new FileSystem())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathExistenceValidator"/> class.
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
        public FilePathExistenceValidator(string referencePath, string parentFilePath, IFileSystem fileSystem)
        {
            Ensure.NotNullOrWhiteSpace(referencePath, nameof(referencePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.referencePath = referencePath;
            this.parentFilePath = parentFilePath;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Validates the file reference in the specified object for existence.
        /// </summary>
        /// <param name="value">The file path information to validate.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> indicating whether the file exists.
        /// When the file exists, <see cref="ValidationResult.Valid"/> is <c>true</c>;
        /// Otherwise, <see cref="ValidationResult.Valid"/> is <c>false</c>
        /// and <see cref="ValidationResult.Message"/> contains the corresponding message.
        /// </returns>
        public ValidationResult Validate(FilePathInfo value)
        {
            return !IsExistingFileReference(value.FileReference) ? 
                       ValidationResult.Fail(GetMessageMissingFileReference(value)) : 
                       ValidationResult.Success;
        }

        private bool IsExistingFileReference(string fileReference)
        {
            string filePath = GetAbsolutePath(fileReference);
            return fileSystem.File.Exists(filePath);
        }

        private string GetMessageMissingFileReference(FilePathInfo filePathInfo)
        {
            string filePath = GetAbsolutePath(filePathInfo.FileReference);
            string message = string.Format(Resources.File_at_location_0_does_not_exist_but_is_defined_in_1_, filePath, parentFilePath) + Environment.NewLine +
                             string.Format(Resources.See_property_0_line_1_, filePathInfo.PropertyName, filePathInfo.LineNumber) + " " + Resources.Data_for_this_item_is_dropped;

            return message;
        }

        private string GetAbsolutePath(string fileName)
        {
            return fileSystem.GetAbsolutePath(referencePath, fileName);
        }
    }
}