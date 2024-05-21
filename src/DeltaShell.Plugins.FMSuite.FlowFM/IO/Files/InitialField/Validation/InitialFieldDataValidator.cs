using System.IO.Abstractions;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Validation
{
    /// <summary>
    /// Initial field data validator that checks for correct kernel and supported GUI user input.
    /// </summary>
    public sealed class InitialFieldDataValidator
    {
        private readonly ILogHandler logHandler;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialFieldDataValidator"/> class.
        /// </summary>
        /// <param name="logHandler">The log handler to report user messages with.</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> or <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        public InitialFieldDataValidator(ILogHandler logHandler, IFileSystem fileSystem)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.logHandler = logHandler;
            this.fileSystem = fileSystem;
        }
        
        /// <summary>
        /// Gets or sets the optional initial field validator for custom GUI validation.
        /// </summary>
        public IValidator<InitialFieldData> FieldValidator { get; set; }

        /// <summary>
        /// Validates the provided <see cref="InitialFieldData"/> object.
        /// Validation rules:
        /// - custom validation must succeed.
        /// - quantity must be provided.
        /// - data file must be provided.
        /// - data file must be a valid path.
        /// - data file type must be provided.
        /// - interpolation method must be provided, if data file type is not 1dField.
        /// - constant interpolation can and must only be used for polygon data file type.
        /// - value must be provided for polygon data file type.
        /// - averaging num min must be 1 or higher, if interpolation is averaging.
        /// </summary>
        /// <param name="initialFieldData"> The object to validate. </param>
        /// <returns>
        /// A boolean indicating whether or not the validation was successful.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialFieldData"/> is <c>null</c>.
        /// </exception>
        public bool Validate(InitialFieldData initialFieldData)
        {
            Ensure.NotNull(initialFieldData, nameof(initialFieldData));

            var hasErrors = false;

            ValidationResult fieldValidationResult = FieldValidator?.Validate(initialFieldData);
            if (fieldValidationResult != null && !fieldValidationResult.Valid)
            {
                ReportError(fieldValidationResult.Message, initialFieldData.LineNumber);
                hasErrors = true;
            }

            ValidateQuantity(initialFieldData, ref hasErrors);
            ValidateDataFile(initialFieldData, ref hasErrors);
            ValidateDataFileType(initialFieldData, ref hasErrors);

            if (initialFieldData.DataFileType == InitialFieldDataFileType.OneDField)
            {
                return !hasErrors;
            }

            ValidateInterpolationMethod(initialFieldData, ref hasErrors);

            return !hasErrors;
        }

        private void ValidateQuantity(InitialFieldData initialFieldData, ref bool hasErrors)
        {
            if (initialFieldData.Quantity == InitialFieldQuantity.None)
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.Quantity, initialFieldData.LineNumber);
                hasErrors = true;
            }
        }

        private void ValidateDataFile(InitialFieldData initialFieldData, ref bool hasErrors)
        {
            if (string.IsNullOrWhiteSpace(initialFieldData.DataFile))
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.DataFile, initialFieldData.LineNumber);
                hasErrors = true;
            }
            else if (!fileSystem.File.Exists(GetDataFilePath(initialFieldData)))
            {
                ReportError(string.Format(Resources.Initial_field_data_file_does_not_exist_0_, initialFieldData.DataFile), initialFieldData.LineNumber);
                hasErrors = true;
            }
        }
        
        private string GetDataFilePath(InitialFieldData initialFieldData)
        {
            if (string.IsNullOrWhiteSpace(initialFieldData.ParentDataDirectory))
            {
                return initialFieldData.DataFile;
            }
            
            return fileSystem.GetAbsolutePath(initialFieldData.ParentDataDirectory, initialFieldData.DataFile);
        }

        private void ValidateDataFileType(InitialFieldData initialFieldData, ref bool hasErrors)
        {
            if (initialFieldData.DataFileType == InitialFieldDataFileType.None)
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.DataFileType, initialFieldData.LineNumber);
                hasErrors = true;
            }

            if (initialFieldData.DataFileType == InitialFieldDataFileType.Polygon)
            {
                ValidateWhenPolygonDataFileType(initialFieldData, ref hasErrors);
            }
        }

        private void ValidateWhenPolygonDataFileType(InitialFieldData initialFieldData, ref bool hasErrors)
        {
            if (double.IsNaN(initialFieldData.Value))
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.Value, initialFieldData.LineNumber);
                hasErrors = true;
            }

            if (initialFieldData.InterpolationMethod != InitialFieldInterpolationMethod.Constant)
            {
                ReportError(string.Format(Resources.Property_0_should_be_1_when_2_is_3_,
                                          InitialFieldFileConstants.Keys.InterpolationMethod,
                                          InitialFieldInterpolationMethod.Constant.GetDescription(),
                                          InitialFieldFileConstants.Keys.DataFileType,
                                          InitialFieldDataFileType.Polygon.GetDescription()), initialFieldData.LineNumber);
                hasErrors = true;
            }
        }

        private void ValidateInterpolationMethod(InitialFieldData initialFieldData, ref bool hasErrors)
        {
            if (initialFieldData.InterpolationMethod == InitialFieldInterpolationMethod.None)
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod, initialFieldData.LineNumber);
                hasErrors = true;
            }

            if (initialFieldData.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                ValidateWhenAveragingInterpolationMethod(initialFieldData, ref hasErrors);
            }

            if (initialFieldData.InterpolationMethod == InitialFieldInterpolationMethod.Constant)
            {
                ValidateWhenConstantInterpolationMethod(initialFieldData, ref hasErrors);
            }
        }

        private void ValidateWhenConstantInterpolationMethod(InitialFieldData initialFieldData, ref bool hasErrors)
        {
            if (initialFieldData.DataFileType != InitialFieldDataFileType.Polygon)
            {
                ReportError(string.Format(Resources.Property_0_can_only_be_1_when_2_is_3_,
                                          InitialFieldFileConstants.Keys.InterpolationMethod,
                                          InitialFieldInterpolationMethod.Constant.GetDescription(),
                                          InitialFieldFileConstants.Keys.DataFileType,
                                          InitialFieldDataFileType.Polygon.GetDescription()), initialFieldData.LineNumber);
                hasErrors = true;
            }
        }

        private void ValidateWhenAveragingInterpolationMethod(InitialFieldData initialFieldData, ref bool hasErrors)
        {
            if (initialFieldData.AveragingNumMin < 1)
            {
                ReportError(string.Format(Resources.Property_0_must_be_1_or_higher, InitialFieldFileConstants.Keys.AveragingNumMin), initialFieldData.LineNumber);
                hasErrors = true;
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