using System.IO.Abstractions;
using DelftTools.Utils.Guards;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Validation
{
    /// <summary>
    /// Validator for files referenced by laterals from the boundary external forcing file.
    /// </summary>
    public sealed class LateralFileValidator
    {
        private readonly FilePathValidator filePathValidator;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initialize a new instance of the <see cref="LateralFileValidator"/> class.
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
        public LateralFileValidator(string referencePath, string parentFilePath, ILogHandler logHandler, IFileSystem fileSystem)
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
        /// <param name="lateralDTO">The lateral object to validate.</param>
        /// <returns>
        /// <c>true</c> if all file references are valid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateralDTO"/> is <c>null</c>.
        /// </exception>
        public bool Validate(LateralDTO lateralDTO)
        {
            Ensure.NotNull(lateralDTO, nameof(lateralDTO));

            if (lateralDTO.Discharge?.Mode != SteerableMode.TimeSeries)
            {
                return true;
            }

            return ValidateFileReference(lateralDTO.Discharge.TimeSeriesFilename, BndExtForceFileConstants.DischargeKey, lateralDTO.LineNumber);
        }

        private bool ValidateFileReference(string fileReference, string property, int lineNumber)
        {
            var filePathInfo = new FilePathInfo(fileReference, property, lineNumber);

            ValidationResult result = filePathValidator.Validate(filePathInfo);
            if (!result.Valid)
            {
                logHandler.ReportError(result.Message);
                return false;
            }

            return true;
        }
    }
}