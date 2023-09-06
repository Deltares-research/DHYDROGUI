using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// <see cref="DelftIniBackwardsCompatibilityHelper"/> provides the methods to update
    /// properties and sections based upon a provided <see cref="IDelftIniBackwardsCompatibilityConfigurationValues"/>.
    /// </summary>
    public class DelftIniBackwardsCompatibilityHelper
    {
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
        /// Get the mapping of <paramref name="propertyKey"/> if one exists, else null.
        /// </summary>
        /// <param name="propertyKey"> Name of the property to be updated. </param>
        /// <param name="logHandler"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="propertyKey"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when
        /// <param name="propertyKey"/>
        /// is null.
        /// </exception>
        /// <remarks>
        /// Note that property keys are case-insensitive and will be matched as such.
        /// </remarks>
        public string GetUpdatedPropertyKey(string propertyKey, ILogHandler logHandler = null)
        {
            Ensure.NotNull(propertyKey, nameof(propertyKey));
            return GetUpdatedKey(propertyKey, configurationValues.LegacyPropertyMapping, logHandler);
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
        /// Thrown when
        /// <param name="sectionName"/>
        /// is null.
        /// </exception>
        /// <remarks>
        /// Note that section names are case-insensitive and will be matched as such.
        /// </remarks>
        public string GetUpdatedSectionName(string sectionName, ILogHandler logHandler = null)
        {
            Ensure.NotNull(sectionName, nameof(sectionName));
            return GetUpdatedKey(sectionName, configurationValues.LegacySectionMapping, logHandler);
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

                logHandler.ReportWarning(string.Format(Resources.Key_0_is_deprecated_and_automatically_removed_from_model, property.Key));
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

        private static string GetUpdatedKey(string propertyKey,
                                            IReadOnlyDictionary<string, string> mapping,
                                            ILogHandler logHandler)
        {
            string propertyKeyLower = propertyKey.ToLower();

            if (!mapping.ContainsKey(propertyKeyLower))
            {
                return null;
            }

            string mappedKey = mapping[propertyKeyLower];
            logHandler?.ReportWarningFormat(Resources.DelftIniBackwardsCompatibilityHelper_GetUpdatedKey_Backwards_Compatibility____0___has_been_updated_to___1__,
                                            propertyKey,
                                            mappedKey);

            return mappedKey;
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
            logHandler?.ReportWarningFormat(Resources.DelftIniBackwardsCompatibilityHelper_GetUpdatedKey_Backwards_Compatibility____0___has_been_updated_to___1__,
                                            propertyKey,
                                            mappedKey);

            return mappedKey;
        }
    }
}