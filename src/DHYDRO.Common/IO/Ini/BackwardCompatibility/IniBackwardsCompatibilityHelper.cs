using System;
using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.Guards;
using DHYDRO.Common.Logging;
using DHYDRO.Common.Properties;

namespace DHYDRO.Common.IO.Ini.BackwardCompatibility
{
    /// <summary>
    /// <see cref="IniBackwardsCompatibilityHelper"/> provides the methods to update
    /// properties and sections based upon a provided <see cref="IIniBackwardsCompatibilityConfigurationValues"/>.
    /// </summary>
    public class IniBackwardsCompatibilityHelper
    {
        private const StringComparison caseInsensitiveComparison = StringComparison.InvariantCultureIgnoreCase;
        private readonly IIniBackwardsCompatibilityConfigurationValues configurationValues;

        /// <summary>
        /// Creates a new <see cref="IniBackwardsCompatibilityHelper"/>.
        /// </summary>
        /// <param name="configurationValues">The configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configurationValues"/> is <c>null</c>.
        /// </exception>
        public IniBackwardsCompatibilityHelper(IIniBackwardsCompatibilityConfigurationValues configurationValues)
        {
            Ensure.NotNull(configurationValues, nameof(configurationValues));

            this.configurationValues = configurationValues;
        }

        /// <summary>
        /// Determines whether the provided <paramref name="propertyKey"/> is currently considered obsolete.
        /// </summary>
        /// <param name="propertyKey">The property key to check.</param>
        /// <returns>
        /// <c>true</c> if the specified property key is obsolete; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="propertyKey"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Note that property names are case-insensitive and will be matched as such.
        /// </remarks>
        public bool IsObsoletePropertyKey(string propertyKey)
        {
            Ensure.NotNull(propertyKey, nameof(propertyKey));

            return configurationValues.ObsoleteProperties.Contains(propertyKey.ToLowerInvariant());
        }

        /// <summary>
        /// Determines whether the provided <paramref name="propertyKey"/> is currently considered obsolete.
        /// </summary>
        /// <param name="propertyKey">The property name to check.</param>
        /// <param name="section">The section the property belongs to.</param>
        /// <returns>
        /// <c>true</c> if the specified property name is obsolete; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <remarks>
        /// Note that property names are case-insensitive and will be matched as such.
        /// </remarks>
        public bool IsConditionalObsoletePropertyKey(string propertyKey, IniSection section)
        {
            Ensure.NotNull(propertyKey, nameof(propertyKey));
            Ensure.NotNull(section, nameof(section));

            if (configurationValues.ConditionalObsoleteProperties.TryGetValue(propertyKey.ToLowerInvariant(), out string conditionalProperty))
            {
                return section.Properties.Any(property => property.IsKeyEqualTo(conditionalProperty));
            }

            return false;
        }

        /// <summary>
        /// Determines whether the provided property has an unsupported value.
        /// </summary>
        /// <param name="sectionName"> The section name. </param>
        /// <param name="propertyKey"> The property key. </param>
        /// <param name="value"> The property value. </param>
        /// <returns> Whether or not the property value is unsupported. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// All arguments are compared with a case-insensitive comparison.
        /// </remarks>
        public bool IsUnsupportedPropertyValue(string sectionName, string propertyKey, string value)
        {
            Ensure.NotNull(sectionName, nameof(sectionName));
            Ensure.NotNull(propertyKey, nameof(propertyKey));
            Ensure.NotNull(value, nameof(value));

            return configurationValues.UnsupportedPropertyValues
                                      .Any(v => IsUnsupportedPropertyValue(v, sectionName, propertyKey, value));
        }

        private static bool IsUnsupportedPropertyValue(IniPropertyInfo propertyInfo,
                                                       string sectionName,
                                                       string propertyKey,
                                                       string value)
        {
            return propertyInfo.Section.Equals(sectionName, caseInsensitiveComparison) &&
                   propertyInfo.Property.Equals(propertyKey, caseInsensitiveComparison) &&
                   propertyInfo.Value.Equals(value, caseInsensitiveComparison);
        }

        /// <summary>
        /// Get the mapping of <paramref name="propertyKey"/> if one exists, else null.
        /// </summary>
        /// <param name="propertyKey"> Key of the property to be updated. </param>
        /// <param name="logHandler"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="propertyKey"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="propertyKey"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Note that property names are case-insensitive and will be matched as such.
        /// </remarks>
        public string GetUpdatedPropertyKey(string propertyKey, ILogHandler logHandler = null)
        {
            Ensure.NotNull(propertyKey, nameof(propertyKey));

            return GetUpdatedKey(propertyKey, configurationValues.LegacyPropertyMapping, logHandler);
        }

        private static string GetUpdatedKey(string propertyKey,
                                            IReadOnlyDictionary<string, NewPropertyData> mapping,
                                            ILogHandler logHandler)
        {
            string propertyKeyLower = propertyKey.ToLower();

            if (!mapping.ContainsKey(propertyKeyLower))
            {
                return null;
            }

            string mappedKey = mapping[propertyKeyLower].Key;
            logHandler?.ReportWarningFormat(Resources.Backwards_Compatibility_0_has_been_updated_to_1_,
                                            propertyKey,
                                            mappedKey);

            return mappedKey;
        }

        /// <summary>
        /// Get the mapping of <paramref name="sectionName"/> if one exists, else null.
        /// </summary>
        /// <param name="sectionName"> Name of the section to be updated. </param>
        /// <param name="logHandler"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="sectionName"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sectionName"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Note that section names are case-insensitive and will be matched as such.
        /// </remarks>
        public string GetUpdatedSectionName(string sectionName, ILogHandler logHandler = null)
        {
            Ensure.NotNull(sectionName, nameof(sectionName));

            return GetUpdatedName(sectionName, configurationValues.LegacySectionMapping, logHandler);
        }

        private static string GetUpdatedName(string oldName,
                                             IReadOnlyDictionary<string, string> mapping,
                                             ILogHandler logHandler)
        {
            string oldNameLower = oldName.ToLower();

            if (!mapping.ContainsKey(oldNameLower))
            {
                return null;
            }

            string newName = mapping[oldNameLower];
            logHandler?.ReportWarningFormat(Resources.Backwards_Compatibility_0_has_been_updated_to_1_,
                                            oldName,
                                            newName);

            return newName;
        }

        /// <summary>
        /// Removes the obsolete properties from the given section.
        /// For each removed property a warning is reported.
        /// </summary>
        /// <param name="section">The INI section to delete the obsolete properties from. </param>
        /// <param name="logHandler"> The log handler to log messages with. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>
        /// </exception>
        public void RemoveObsoletePropertiesWithWarning(IniSection section, ILogHandler logHandler)
        {
            Ensure.NotNull(section, nameof(section));
            Ensure.NotNull(logHandler, nameof(logHandler));

            foreach (IniProperty property in section.Properties.ToArray())
            {
                if (!IsObsoletePropertyKey(property.Key) && !IsConditionalObsoletePropertyKey(property.Key, section))
                {
                    continue;
                }

                logHandler.ReportWarning(string.Format(Resources.Key__0__is_deprecated_and_automatically_removed_from_model_, property.Key));
                section.RemoveProperty(property);
            }
        }

        /// <summary>
        /// Updates a property to its latest version.
        /// </summary>
        /// <param name="property">The <see cref="IniSection"/> to update.</param>
        /// <param name="section">The <see cref="IniSection"/> the property belongs to.</param>
        /// <param name="logHandler">The log handler to log messages with.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public void UpdateProperty(IniProperty property, IniSection section, ILogHandler logHandler)
        {
            Ensure.NotNull(property, nameof(property));
            Ensure.NotNull(section, nameof(section));
            Ensure.NotNull(logHandler, nameof(logHandler));

            if (!IsLegacyProperty(property))
            {
                return;
            }

            NewPropertyData newData = configurationValues.LegacyPropertyMapping[property.Key.ToLower()];

            string newKey = GetUpdatedPropertyKey(property.Key);

            IPropertyUpdater updater = newData.Updater;
            updater.UpdateProperty(property.Key, newKey, section, logHandler);
        }

        private bool IsLegacyProperty(IniProperty property)
        {
            return configurationValues.LegacyPropertyMapping.ContainsKey(property.Key.ToLower());
        }
    }
}