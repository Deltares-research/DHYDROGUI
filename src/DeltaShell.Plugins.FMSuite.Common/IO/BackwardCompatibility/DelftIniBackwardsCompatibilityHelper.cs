using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Handlers;
using Microsoft.Scripting.Runtime;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// <see cref="DelftIniBackwardsCompatibilityHelper"/> provides the methods to update
    /// properties and categories based upon a provided <see cref="IDelftIniBackwardsCompatibilityConfig"/>.
    /// </summary>
    public class DelftIniBackwardsCompatibilityHelper
    {
        private readonly IDelftIniBackwardsCompatibilityConfig config;

        /// <summary>
        /// Creates a new <see cref="DelftIniBackwardsCompatibilityHelper"/>.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="config"/> is <c>null</c>.
        /// </exception>
        public DelftIniBackwardsCompatibilityHelper([NotNull]IDelftIniBackwardsCompatibilityConfig config)
        {
            Ensure.NotNull(config, nameof(config));
            this.config = config;
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
        /// Thrown when <param name="propertyName"/> is null.
        /// </exception>
        public string GetUpdatedPropertyName([NotNull]string propertyName, ILogHandler logHandler = null)
        {
            Ensure.NotNull(propertyName, nameof(propertyName));
            return GetUpdatedName(propertyName, config.LegacyPropertyMapping, logHandler);
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
        /// Thrown when <param name="categoryName"/> is null.
        /// </exception>
        public string GetUpdatedCategoryName([NotNull]string categoryName, ILogHandler logHandler = null)
        {
            Ensure.NotNull(categoryName, nameof(categoryName));
            return GetUpdatedName(categoryName, config.LegacyCategoryMapping, logHandler);
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
            logHandler?.ReportWarningFormat("Backwards Compatibility: '{0}' has been updated to '{1}'",
                                            propertyName,
                                            mappedName);

            return mappedName;
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
        public bool IsObsoletePropertyName([NotNull]string propertyName)
        {
            Ensure.NotNull(propertyName, nameof(propertyName));
            return config.ObsoleteProperties.Contains(propertyName.ToLowerInvariant());
        }
    }
}