using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.Extensions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// Class for validating 3D layer properties
    /// </summary>
    public static class WaterFlowFM3DLayerPropertyValidator
    {
        /// <summary>
        /// Validates the given 3D layer property.
        /// </summary>
        /// <remarks>
        /// The validation of some 3D layer properties are dependent on the values of other properties from
        /// the same category.
        /// </remarks>
        /// <param name="propertyToValidate">The 3D layer property to validate.</param>
        /// <param name="allProperties">All properties from the same category as the property to validate.</param>
        /// <returns>An error message. Returns an empty string if the property is valid.</returns>
        /// <exception cref="ArgumentException">Thrown when any argument is <c>null</c>.</exception>
        public static string Validate(WaterFlowFMProperty propertyToValidate,
                                      IEnumerable<WaterFlowFMProperty> allProperties)
        {
            Ensure.NotNull(propertyToValidate, nameof(propertyToValidate));
            Ensure.NotNull(allProperties, nameof(allProperties));

            if (!propertyToValidate.IsEnabled(allProperties))
            {
                return string.Empty;
            }

            string propertyName = propertyToValidate.PropertyDefinition.MduPropertyName.ToLower();
            switch (propertyName)
            {
                case KnownProperties.DzTop:
                    return ValidateThatValueIsLargerThanZero(propertyToValidate);
                case KnownProperties.FloorLevTopLay:
                case KnownProperties.DzTopUniAboveZ:
                    return ValidateThatValueIsLessThanZero(propertyToValidate);
                case KnownProperties.NumTopSig:
                    return ValidateThatValueIsBetweenZeroAndKmx(propertyToValidate, allProperties);
                default:
                    throw new ArgumentException(string.Format(Resources.WaterFlowFM3DLayerPropertyValidator_Cannot_validate_property, propertyName));
            }
        }

        private static string ValidateThatValueIsLargerThanZero(WaterFlowFMProperty property)
        {
            string propertyValueAsString = property.GetValueAsString();
            var value = (double)FMParser.FromString(propertyValueAsString, property.PropertyDefinition.DataType);

            if (value <= 0)
            {
                return string.Format(Resources.Parameter__0__should_be_more_than_zero, property.PropertyDefinition.MduPropertyName);
            }

            return string.Empty;
        }

        private static string ValidateThatValueIsLessThanZero(WaterFlowFMProperty property)
        {
            string propertyValueAsString = property.GetValueAsString();
            var propertyValue = (double)FMParser.FromString(propertyValueAsString, property.PropertyDefinition.DataType);

            if (propertyValue >= 0)
            {
                return string.Format(Resources.Parameter__0__should_be_less_than_zero, property.PropertyDefinition.MduPropertyName);
            }

            return string.Empty;
        }

        private static string ValidateThatValueIsBetweenZeroAndKmx(WaterFlowFMProperty numTopSigProperty,
                                                                   IEnumerable<WaterFlowFMProperty> allProperties)
        {
            string numTopSigPropertyAsString = numTopSigProperty.GetValueAsString();
            var numTopSigPropertyValue = (int)FMParser.FromString(numTopSigPropertyAsString, numTopSigProperty.PropertyDefinition.DataType);

            WaterFlowFMProperty kmxProperty = allProperties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.EqualsCaseInsensitive(KnownProperties.Kmx));
            if (kmxProperty is null)
            {
                throw new ArgumentException(string.Format(Resources.Kmx_property_is_required_to_validate__0__, KnownProperties.NumTopSig));
            }

            string kmxPropertyAsString = kmxProperty.GetValueAsString();
            var kmxValue = (int)FMParser.FromString(kmxPropertyAsString,
                                                    kmxProperty.PropertyDefinition.DataType);

            if (numTopSigPropertyValue < 0 || numTopSigPropertyValue > kmxValue)
            {
                return string.Format(Resources.Parameter__0__should_be_between_0_and__1___the_current_value_of__2___,
                                     KnownProperties.NumTopSig, kmxValue, KnownProperties.Kmx);
            }

            return string.Empty;
        }
    }
}