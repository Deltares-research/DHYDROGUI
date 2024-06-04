using System.Collections.Generic;
using DelftTools.Utils.Guards;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.IO.InitialField;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField
{
    /// <summary>
    /// Custom validator for GUI specific validation.
    /// Validates for unsupported property values.
    /// </summary>
    public sealed class SupportedInitialFieldDataValidator : IValidator<InitialFieldData>
    {
        private readonly HashSet<InitialFieldQuantity> supportedQuantities = new HashSet<InitialFieldQuantity>
        {
            InitialFieldQuantity.BedLevel,
            InitialFieldQuantity.WaterLevel,
            InitialFieldQuantity.FrictionCoefficient
        };

        /// <inheritdoc/>
        public ValidationResult Validate(InitialFieldData value)
        {
            Ensure.NotNull(value, nameof(value));

            if (!supportedQuantities.Contains(value.Quantity))
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.Quantity,
                                                            value.Quantity.GetDescription(),
                                                            value.LineNumber);
                return ValidationResult.Fail(message);
            }

            if (value.DataFileType == InitialFieldDataFileType.OneDField)
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.DataFileType,    
                                                            value.DataFileType.GetDescription(),
                                                            value.LineNumber);
                return ValidationResult.Fail(message);
            }

            if (value.AveragingType == InitialFieldAveragingType.Median)
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.AveragingType,
                                                            value.AveragingType.GetDescription(),
                                                            value.LineNumber);
                return ValidationResult.Fail(message);
            }

            return ValidationResult.Success;
        }

        private static string GetUnsupportedValueMessage(string propertyName, string value, int lineNumber)
        {
            return string.Format(Resources.Property_0_contains_unsupported_value_1_Line_2_, propertyName, value, lineNumber);
        }
    }
}