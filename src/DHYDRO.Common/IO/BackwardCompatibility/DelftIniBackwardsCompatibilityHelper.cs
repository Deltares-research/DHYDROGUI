using System;
using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.Guards;
using DHYDRO.Common.Logging;
using DHYDRO.Common.Properties;

namespace DHYDRO.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// <see cref="DelftIniBackwardsCompatibilityHelper"/> provides the methods to update
    /// properties and categories based upon a provided <see cref="IDelftIniBackwardsCompatibilityConfigurationValues"/>.
    /// </summary>
    public class DelftIniBackwardsCompatibilityHelper
    {
        private const StringComparison caseInsensitiveComparison = StringComparison.InvariantCultureIgnoreCase;
        private readonly IDelftIniBackwardsCompatibilityConfigurationValues configurationValues;

        /// <summary>
        /// Creates a new <see cref="DelftIniBackwardsCompatibilityHelper"/>.
        /// </summary>
        /// <param name="configurationValues">The configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configurationValues"/> is <c>null</c>.
        /// </exception>
        public DelftIniBackwardsCompatibilityHelper(IDelftIniBackwardsCompatibilityConfigurationValues configurationValues)
        {
            Ensure.NotNull(configurationValues, nameof(configurationValues));
            this.configurationValues = configurationValues;
        }

        /// <summary>
        /// Determines whether the provided <paramref name="propertyName"/> is currently considered obsolete.
        /// </summary>
        /// <param name="propertyName">The property name to check.</param>
        /// <returns>
        /// <c>true</c> if the specified property name is obsolete; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="propertyName"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Note that property names are case-insensitive and will be matched as such.
        /// </remarks>
        public bool IsObsoletePropertyName(string propertyName)
        {
            Ensure.NotNull(propertyName, nameof(propertyName));

            return configurationValues.ObsoleteProperties.Contains(propertyName.ToLowerInvariant());
        }

        /// <summary>
        /// Determines whether the provided property has an unsupported value.
        /// </summary>
        /// <param name="category"> The category name. </param>
        /// <param name="property"> The property name. </param>
        /// <param name="value"> The property value. </param>
        /// <returns> Whether or not the property value is unsupported. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// All arguments are compared with a case-insensitive comparison.
        /// </remarks>
        public bool IsUnsupportedPropertyValue(string category, string property, string value)
        {
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(property, nameof(property));
            Ensure.NotNull(value, nameof(value));

            return configurationValues.UnsupportedPropertyValues
                                      .Any(v => IsUnsupportedPropertyValue(v, category, property, value));
        }

        /// <summary>
        /// Get the mapping of <paramref name="propertyName"/> if one exists, else null.
        /// </summary>
        /// <param name="propertyName"> Name of the property to be updated. </param>
        /// <param name="logHandler"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="propertyName"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="propertyName"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Note that property names are case-insensitive and will be matched as such.
        /// </remarks>
        public string GetUpdatedPropertyName(string propertyName, ILogHandler logHandler = null)
        {
            Ensure.NotNull(propertyName, nameof(propertyName));

            return GetUpdatedName(propertyName, configurationValues.LegacyPropertyMapping, logHandler);
        }

        /// <summary>
        /// Get the mapping of <paramref name="categoryName"/> if one exists, else null.
        /// </summary>
        /// <param name="categoryName"> Name of the category to be updated. </param>
        /// <param name="logHandler"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="categoryName"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="categoryName"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Note that categories names are case-insensitive and will be matched as such.
        /// </remarks>
        public string GetUpdatedCategoryName(string categoryName, ILogHandler logHandler = null)
        {
            Ensure.NotNull(categoryName, nameof(categoryName));

            return GetUpdatedName(categoryName, configurationValues.LegacyCategoryMapping, logHandler);
        }

        private static string GetUpdatedName(string propertyName,
                                             IReadOnlyDictionary<string, string> mapping,
                                             ILogHandler logHandler)
        {
            string propertyNameLower = propertyName.ToLower();

            if (!mapping.ContainsKey(propertyNameLower))
            {
                return null;
            }

            string mappedName = mapping[propertyNameLower];
            logHandler?.ReportWarningFormat(Resources.Backwards_Compatibility_0_has_been_updated_to_1_,
                                            propertyName,
                                            mappedName);

            return mappedName;
        }

        private static bool IsUnsupportedPropertyValue(DelftIniPropertyInfo propertyInfo,
                                                       string category,
                                                       string property,
                                                       string value)
        {
            return propertyInfo.Category.Equals(category, caseInsensitiveComparison) &&
                   propertyInfo.Property.Equals(property, caseInsensitiveComparison) &&
                   propertyInfo.Value.Equals(value, caseInsensitiveComparison);
        }
    }
}