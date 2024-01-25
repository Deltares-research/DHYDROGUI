using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization
{
    /// <summary>
    /// Custom validator for GUI specific validation.
    /// Validates for unsupported property values.
    /// </summary>
    public sealed class SupportedInitialFieldValidator : IValidator<InitialField>
    {
        private readonly HashSet<InitialFieldQuantity> supportedQuantities = new HashSet<InitialFieldQuantity>
        {
            InitialFieldQuantity.BedLevel,
            InitialFieldQuantity.WaterLevel,
            InitialFieldQuantity.FrictionCoefficient
        };

        /// <inheritdoc/>
        public ValidationResult Validate(InitialField value)
        {
            Ensure.NotNull(value, nameof(value));

            if (!supportedQuantities.Contains(value.Quantity))
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.Quantity,
                                                            value.Quantity.GetDescription());
                return ValidationResult.Fail(message);
            }

            if (value.DataFileType == InitialFieldDataFileType.OneDField)
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.DataFileType,
                                                            value.DataFileType.GetDescription());
                return ValidationResult.Fail(message);
            }

            if (value.AveragingType == InitialFieldAveragingType.Median)
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.AveragingType,
                                                            value.AveragingType.GetDescription());
                return ValidationResult.Fail(message);
            }

            return ValidationResult.Success;
        }

        private static string GetUnsupportedValueMessage(string propertyName, string value)
        {
            return string.Format(Resources.Property_0_contains_unsupported_value_1_, propertyName, value);
        }
    }
}