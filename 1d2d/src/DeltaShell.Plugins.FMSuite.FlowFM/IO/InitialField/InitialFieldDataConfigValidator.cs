using System;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Validation;
using Deltares.Infrastructure.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.InitialField;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField
{
    /// <summary>
    /// Custom validator for GUI specific validation.
    /// Validates for unsupported property values.
    /// </summary>
    public sealed class InitialFieldDataConfigValidator : IValidator<InitialFieldData>
    {
        private readonly WaterFlowFMModelDefinition modelDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialFieldDataConfigValidator"/> class.
        /// </summary>
        /// <param name="modelDefinition">The model definition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> is <c>null</c>.
        /// </exception>
        public InitialFieldDataConfigValidator(WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));

            this.modelDefinition = modelDefinition;
        }

        /// <inheritdoc/>
        public ValidationResult Validate(InitialFieldData initialField)
        {
            Ensure.NotNull(initialField, nameof(initialField));

            if (HasUnsupportedQuantity(initialField))
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.Quantity, initialField.Quantity);
                return ValidationResult.Fail(message);
            }

            if (HasUnsupportedAveragingType(initialField))
            {
                string message = GetUnsupportedValueMessage(InitialFieldFileConstants.Keys.AveragingType, initialField.AveragingType);
                return ValidationResult.Fail(message);
            }

            if (HasInvalidFrictionType(initialField))
            {
                return ValidationResult.Fail(GetInvalidFrictionTypeMessage());
            }

            return ValidationResult.Success;
        }

        private static bool HasUnsupportedQuantity(InitialFieldData initialField)
            => !InitialFieldFileQuantities.SupportedQuantities.Keys.Contains(initialField.Quantity);

        private static bool HasUnsupportedAveragingType(InitialFieldData initialField)
            => initialField.AveragingType == InitialFieldAveragingType.Median;

        private bool HasInvalidFrictionType(InitialFieldData initialField)
        {
            if (initialField.Quantity != InitialFieldQuantity.FrictionCoefficient)
            {
                return false;
            }

            var initialFieldFrictionType = (int)initialField.FrictionType;
            var modelFrictionType = (int)modelDefinition.GetModelProperty(KnownProperties.FrictionType).Value;

            return initialFieldFrictionType != modelFrictionType;
        }

        private static string GetUnsupportedValueMessage<T>(string propertyName, T value) where T : Enum
            => GetUnsupportedValueMessage(propertyName, value.GetDescription());

        private static string GetUnsupportedValueMessage(string propertyName, string value)
            => string.Format(Resources.Property_0_contains_unsupported_value_1_, propertyName, value);

        private static string GetInvalidFrictionTypeMessage()
            => Resources.Friction_type_does_not_match_the_expected_uniform_model_friction_type_;
    }
}