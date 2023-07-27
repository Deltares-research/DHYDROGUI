using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// <see cref="DelftIniBackwardsCompatibilityHelper"/> provides the methods to update
    /// properties and categories based upon a provided <see cref="IDelftIniBackwardsCompatibilityConfigurationValues"/>.
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
        /// Get the mapping of <paramref name="propertyName"/> if one exists, else null.
        /// </summary>
        /// <param name="propertyName"> Name of the property to be updated. </param>
        /// <param name="logHandler"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="propertyName"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when
        /// <param name="propertyName"/>
        /// is null.
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
        /// Thrown when
        /// <param name="categoryName"/>
        /// is null.
        /// </exception>
        /// <remarks>
        /// Note that categories names are case-insensitive and will be matched as such.
        /// </remarks>
        public string GetUpdatedCategoryName(string categoryName, ILogHandler logHandler = null)
        {
            Ensure.NotNull(categoryName, nameof(categoryName));
            return GetUpdatedName(categoryName, configurationValues.LegacyCategoryMapping, logHandler);
        }

        /// <summary>
        /// Removes the obsolete properties from the given category.
        /// For each removed property a warning is reported.
        /// </summary>
        /// <param name="category">The Delft INI category to delete the obsolete properties from. </param>
        /// <param name="logHandler"> The log handler to log messages with. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any argument is <c>null</c>
        /// </exception>
        public void RemoveObsoletePropertiesWithWarning(DelftIniCategory category, ILogHandler logHandler)
        {
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(logHandler, nameof(logHandler));

            foreach (DelftIniProperty property in category.Properties.ToArray())
            {
                if (!IsObsoletePropertyName(property.Name))
                {
                    continue;
                }

                logHandler.ReportWarning(string.Format(Resources.Key_0_is_deprecated_and_automatically_removed_from_model, property.Name));
                category.RemoveProperty(property);
            }
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
            logHandler?.ReportWarningFormat(Resources.DelftIniBackwardsCompatibilityHelper_GetUpdatedName_Backwards_Compatibility____0___has_been_updated_to___1__,
                                            propertyName,
                                            mappedName);

            return mappedName;
        }
    }
}