using System.IO.Abstractions;
using System.Text;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Validation;
using Deltares.Infrastructure.IO;
using DHYDRO.Common.Properties;

namespace DHYDRO.Common.IO.ExtForce
{
    /// <summary>
    /// External forcing data validator.
    /// </summary>
    public sealed class ExtForceDataValidator : IValidator<ExtForceData>
    {
        private readonly IFileSystem fileSystem;

        private bool hasErrors;
        private StringBuilder messages;
        private ExtForceData extForceData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtForceDataValidator"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        public ExtForceDataValidator(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Validates the provided <see cref="ExtForceData"/> object.
        /// Validation rules:
        /// - quantity must be provided.
        /// - data file name must be provided.
        /// - data file must be a valid path.
        /// - data file type must be provided.
        /// - interpolation method must be provided.
        /// - operand must be provided.
        /// </summary>
        /// <param name="value">The object to validate.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> object indicating whether the validation was successful.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public ValidationResult Validate(ExtForceData value)
        {
            Ensure.NotNull(value, nameof(value));

            hasErrors = false;
            messages = new StringBuilder();
            extForceData = value;

            ValidateQuantity();
            ValidateFileName();
            ValidateFileType();
            ValidateMethod();
            ValidateOperand();

            return hasErrors
                       ? ValidationResult.Fail(messages.ToString().Trim())
                       : ValidationResult.Success;
        }

        private void ValidateQuantity()
        {
            if (string.IsNullOrWhiteSpace(extForceData.Quantity))
            {
                ReportErrorMissingPropertyValue(ExtForceFileConstants.Keys.Quantity, extForceData.LineNumber);
                hasErrors = true;
            }
        }

        private void ValidateFileName()
        {
            if (string.IsNullOrWhiteSpace(extForceData.FileName))
            {
                ReportErrorMissingPropertyValue(ExtForceFileConstants.Keys.FileName, extForceData.LineNumber);
                hasErrors = true;
            }
            else if (!fileSystem.File.Exists(GetFilePath()))
            {
                ReportError(string.Format(Resources.Forcing_file_does_not_exist_0_, extForceData.FileName), extForceData.LineNumber);
                hasErrors = true;
            }
        }

        private string GetFilePath()
        {
            if (string.IsNullOrWhiteSpace(extForceData.ParentDirectory))
            {
                return extForceData.FileName;
            }

            return fileSystem.GetAbsolutePath(extForceData.ParentDirectory, extForceData.FileName);
        }

        private void ValidateFileType()
        {
            if (extForceData.FileType == null)
            {
                ReportErrorMissingPropertyValue(ExtForceFileConstants.Keys.FileType, extForceData.LineNumber);
                hasErrors = true;
            }
        }

        private void ValidateMethod()
        {
            if (extForceData.Method == null)
            {
                ReportErrorMissingPropertyValue(ExtForceFileConstants.Keys.Method, extForceData.LineNumber);
                hasErrors = true;
            }
        }

        private void ValidateOperand()
        {
            if (string.IsNullOrWhiteSpace(extForceData.Operand))
            {
                ReportErrorMissingPropertyValue(ExtForceFileConstants.Keys.Operand, extForceData.LineNumber);
                hasErrors = true;
            }
        }

        private void ReportErrorMissingPropertyValue(string propertyName, int lineNumber)
        {
            ReportError(string.Format(Resources.Property_0_must_be_provided, propertyName), lineNumber);
        }

        private void ReportError(string message, int lineNumber)
        {
            messages.AppendFormat(Resources._0_Line_1_, message, lineNumber);
            messages.AppendLine();
        }
    }
}