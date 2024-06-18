using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.InitialField;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField
{
    /// <summary>
    /// Custom validator for GUI specific validation.
    /// Validates for unsupported property values.
    /// </summary>
    public sealed class InitialFieldDataConfigValidator : IValidator<InitialFieldData>
    {
        private static readonly HashSet<InitialFieldQuantity> supportedQuantities = new HashSet<InitialFieldQuantity>
        {
            InitialFieldQuantity.BedLevel,
            InitialFieldQuantity.WaterLevel,
            InitialFieldQuantity.FrictionCoefficient
        };

        /// <inheritdoc/>
        public ValidationResult Validate(InitialFieldData initialField)
        {
            Ensure.NotNull(initialField, nameof(initialField));

            if (HasUnsupportedQuantity(initialField))
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.Quantity, initialField.Quantity);
                return ValidationResult.Fail(message);
            }

            if (HasUnsupportedDataFileType(initialField))
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.DataFileType, initialField.DataFileType);
                return ValidationResult.Fail(message);
            }

            if (HasUnsupportedAveragingType(initialField))
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.AveragingType, initialField.AveragingType);
                return ValidationResult.Fail(message);
            }

            return ValidationResult.Success;
        }

        private static bool HasUnsupportedQuantity(InitialFieldData initialField)
            => !supportedQuantities.Contains(initialField.Quantity);

        private static bool HasUnsupportedDataFileType(InitialFieldData initialField)
            => initialField.DataFileType == InitialFieldDataFileType.OneDField;

        private static bool HasUnsupportedAveragingType(InitialFieldData initialField)
            => initialField.AveragingType == InitialFieldAveragingType.Median;

        private static string GetUnsupportedValueMessage(string propertyName, Enum value)
            => GetUnsupportedValueMessage(propertyName, value.GetDescription());

        private static string GetUnsupportedValueMessage(string propertyName, string value)
            => string.Format(Resources.Property_0_contains_unsupported_value_1_, propertyName, value);
    }
}