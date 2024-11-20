using System.IO.Abstractions;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation
{
    /// <summary>
    /// Validator class for a <see cref="BoundaryDTO"/> that checks for correct user input.
    /// </summary>
    public sealed class BoundaryValidator
    {
        private readonly BoundaryFileValidator fileValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundaryValidator"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <param name="fileSystem"> Provides access to the file system. </param>
        /// <param name="parentDataDirectory">
        /// The parent directory of the reference file, which is the external forcing file or
        /// MDU file dependent on the PathsRelativeToParent option in the MDU.
        /// </param>
        /// <param name="parentFilePath"> The external forcing file path. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="parentDataDirectory"/> or <paramref name="parentFilePath"/> is <c>null</c>.
        /// </exception>
        public BoundaryValidator(ILogHandler logHandler, IFileSystem fileSystem, string parentDataDirectory, string parentFilePath)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(fileSystem, nameof(fileSystem));
            Ensure.NotNullOrWhiteSpace(parentDataDirectory, nameof(parentDataDirectory));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));

            fileValidator = new BoundaryFileValidator(parentDataDirectory, parentFilePath, logHandler, fileSystem);
        }

        /// <summary>
        /// Validates the provided <see cref="BoundaryDTO"/> object.
        /// Validation rules:
        /// - locationFile must be provided.
        /// - forcingFile must be provided.
        /// - all file reference should exist.
        /// </summary>
        /// <param name="boundaryDTO"> The object to validate. </param>
        /// <returns>
        /// A boolean indication whether or not the validation was successful.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryDTO"/> is <c>null</c>.
        /// </exception>
        public bool Validate(BoundaryDTO boundaryDTO)
        {
            Ensure.NotNull(boundaryDTO, nameof(boundaryDTO));

            return fileValidator.Validate(boundaryDTO);
        }
    }
}