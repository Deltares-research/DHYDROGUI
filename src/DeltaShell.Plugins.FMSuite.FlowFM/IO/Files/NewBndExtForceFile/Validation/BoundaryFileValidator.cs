using System.IO.Abstractions;
using DelftTools.Utils.Guards;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation
{
    /// <summary>
    /// Validator for files referenced by boundaries from the boundary external forcing file.
    /// </summary>
    public sealed class BoundaryFileValidator
    {
        private readonly FilePathValidator filePathValidator;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initialize a new instance of the <see cref="BoundaryFileValidator"/> class.
        /// </summary>
        /// <param name="referencePath">
        /// The reference path, which is the external forcing file or MDU file dependent on the
        /// PathsRelativeToParent option in the MDU.
        /// </param>
        /// <param name="parentFilePath"> The external forcing file path. </param>
        /// <param name="logHandler">The log handler to report user messages with.</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> or <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="referencePath"/> or <paramref name="parentFilePath"/> is <c>null</c>.
        /// </exception>
        public BoundaryFileValidator(string referencePath, string parentFilePath, ILogHandler logHandler, IFileSystem fileSystem)
        {
            Ensure.NotNullOrWhiteSpace(referencePath, nameof(referencePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.logHandler = logHandler;
            filePathValidator = FilePathValidator.CreateDefault(referencePath, parentFilePath, fileSystem);
        }

        /// <summary>
        /// Checks the validity of file references for the specified object.
        /// Issues that are encountered are logged to the user.
        /// </summary>
        /// <param name="boundaryDTO">The boundary object to validate.</param>
        /// <returns>
        /// <c>true</c> if all file references are valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryDTO"/> is <c>null</c>.
        /// </exception>
        public bool Validate(BoundaryDTO boundaryDTO)
        {
            Ensure.NotNull(boundaryDTO, nameof(boundaryDTO));

            var isValid = true;

            if (string.IsNullOrWhiteSpace(boundaryDTO.LocationFile))
            {
                ReportErrorMissingPropertyValue(BndExtForceFileConstants.LocationFileKey, boundaryDTO.LineNumber);
                isValid = false;
            }
            else
            {
                ValidateFileReference(boundaryDTO.LocationFile, BndExtForceFileConstants.LocationFileKey, boundaryDTO.LineNumber, ref isValid);
            }

            foreach (string forcingFile in boundaryDTO.ForcingFiles)
            {
                ValidateFileReference(forcingFile, BndExtForceFileConstants.ForcingFileKey, boundaryDTO.LineNumber, ref isValid);
            }

            return isValid;
        }

        private void ValidateFileReference(string fileReference, string property, int lineNumber, ref bool isValid)
        {
            var filePathInfo = new FilePathInfo(fileReference, property, lineNumber);

            ValidationResult result = filePathValidator.Validate(filePathInfo);
            if (!result.Valid)
            {
                logHandler.ReportError(result.Message);
                isValid = false;
            }
        }

        private void ReportErrorMissingPropertyValue(string propertyName, int lineNumber)
        {
            ReportError(string.Format(Resources.Property_0_must_be_provided, propertyName), lineNumber);
        }

        private void ReportError(string message, int lineNumber)
        {
            logHandler.ReportError(string.Format(Resources._0_Line_1_, message, lineNumber));
        }
    }
}