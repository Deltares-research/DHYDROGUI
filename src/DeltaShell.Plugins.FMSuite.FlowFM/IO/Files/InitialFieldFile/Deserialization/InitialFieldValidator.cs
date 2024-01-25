using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization
{
    /// <summary>
    /// Validator class for a <see cref="InitialField"/> that checks for correct kernel and supported GUI user input.
    /// </summary>
    public sealed class InitialFieldValidator
    {
        private readonly IValidator<InitialField> customValidator;
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialFieldValidator"/> class.
        /// </summary>
        /// <param name="logHandler"> The log handler to report user messages with. </param>
        /// <param name="customValidator"> Optional initial field validator for custom GUI validation. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        public InitialFieldValidator(ILogHandler logHandler, IValidator<InitialField> customValidator = null)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            this.logHandler = logHandler;
            this.customValidator = customValidator;
        }

        /// <summary>
        /// Validates the provided <see cref="InitialField"/> object.
        /// Validation rules:
        /// - custom validation must succeed.
        /// - quantity must be provided.
        /// - data file must be provided.
        /// - data file type must be provided.
        /// - interpolation method must be provided, if data file type is not 1dField.
        /// - constant interpolation can and must only be used for polygon data file type.
        /// - value must be provided for polygon data file type.
        /// - averaging num min must be 1 or higher, if interpolation is averaging.
        /// </summary>
        /// <param name="initialField"> The object to validate. </param>
        /// <param name="lineNumber"> The corresponding file line number. </param>
        /// <returns>
        /// A boolean indicating whether or not the validation was successful.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialField"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="lineNumber"/> is a negative number.
        /// </exception>
        public bool Validate(InitialField initialField, int lineNumber)
        {
            Ensure.NotNull(initialField, nameof(initialField));
            Ensure.NotNegative(lineNumber, nameof(lineNumber));

            var hasErrors = false;

            ValidationResult customValidationResult = customValidator?.Validate(initialField);
            if (customValidationResult != null && !customValidationResult.Valid)
            {
                ReportError(customValidationResult.Message, lineNumber);
                hasErrors = true;
            }

            ValidateQuantity(initialField, lineNumber, ref hasErrors);
            ValidateDataFile(initialField, lineNumber, ref hasErrors);
            ValidateDataFileType(initialField, lineNumber, ref hasErrors);

            if (initialField.DataFileType == InitialFieldDataFileType.OneDField)
            {
                return !hasErrors;
            }

            ValidateInterpolationMethod(initialField, lineNumber, ref hasErrors);

            return !hasErrors;
        }

        private void ValidateQuantity(InitialField initialField, int lineNumber, ref bool hasErrors)
        {
            if (initialField.Quantity == InitialFieldQuantity.None)
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.Quantity, lineNumber);
                hasErrors = true;
            }
        }

        private void ValidateDataFile(InitialField initialField, int lineNumber, ref bool hasErrors)
        {
            if (string.IsNullOrWhiteSpace(initialField.DataFile))
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.DataFile, lineNumber);
                hasErrors = true;
            }
        }

        private void ValidateDataFileType(InitialField initialField, int lineNumber, ref bool hasErrors)
        {
            if (initialField.DataFileType == InitialFieldDataFileType.None)
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.DataFileType, lineNumber);
                hasErrors = true;
            }

            if (initialField.DataFileType == InitialFieldDataFileType.Polygon)
            {
                ValidateWhenPolygonDataFileType(initialField, lineNumber, ref hasErrors);
            }
        }

        private void ValidateWhenPolygonDataFileType(InitialField initialField, int lineNumber, ref bool hasErrors)
        {
            if (double.IsNaN(initialField.Value))
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.Value, lineNumber);
                hasErrors = true;
            }

            if (initialField.InterpolationMethod != InitialFieldInterpolationMethod.Constant)
            {
                ReportError(string.Format(Resources.Property_0_should_be_1_when_2_is_3_,
                                          InitialFieldFileConstants.Keys.InterpolationMethod,
                                          InitialFieldInterpolationMethod.Constant.GetDescription(),
                                          InitialFieldFileConstants.Keys.DataFileType,
                                          InitialFieldDataFileType.Polygon.GetDescription()), lineNumber);
                hasErrors = true;
            }
        }

        private void ValidateInterpolationMethod(InitialField initialField, int lineNumber, ref bool hasErrors)
        {
            if (initialField.InterpolationMethod == InitialFieldInterpolationMethod.None)
            {
                ReportErrorMissingPropertyValue(InitialFieldFileConstants.Keys.InterpolationMethod, lineNumber);
                hasErrors = true;
            }

            if (initialField.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                ValidateWhenAveragingInterpolationMethod(initialField, lineNumber, ref hasErrors);
            }

            if (initialField.InterpolationMethod == InitialFieldInterpolationMethod.Constant)
            {
                ValidateWhenConstantInterpolationMethod(initialField, lineNumber, ref hasErrors);
            }
        }

        private void ValidateWhenConstantInterpolationMethod(InitialField initialField, int lineNumber, ref bool hasErrors)
        {
            if (initialField.DataFileType != InitialFieldDataFileType.Polygon)
            {
                ReportError(string.Format(Resources.Property_0_can_only_be_1_when_2_is_3_,
                                          InitialFieldFileConstants.Keys.InterpolationMethod,
                                          InitialFieldInterpolationMethod.Constant.GetDescription(),
                                          InitialFieldFileConstants.Keys.DataFileType,
                                          InitialFieldDataFileType.Polygon.GetDescription()), lineNumber);
                hasErrors = true;
            }
        }

        private void ValidateWhenAveragingInterpolationMethod(InitialField initialField, int lineNumber, ref bool hasErrors)
        {
            if (initialField.AveragingNumMin < 1)
            {
                ReportError(string.Format(Resources.Property_0_must_be_1_or_higher, InitialFieldFileConstants.Keys.AveragingNumMin), lineNumber);
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