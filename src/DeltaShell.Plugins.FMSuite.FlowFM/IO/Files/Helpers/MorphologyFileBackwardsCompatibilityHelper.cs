using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    /// <summary>
    /// A collection of static functions to handle Backwards Compatibility of the Morphology File.
    /// </summary>
    public static class MorphologyFileBackwardsCompatibilityHelper
    {
        /// <summary>
        /// The category property mapping. This contains the currently
        /// available Backwards Compatibility mappings.
        ///
        /// Each of the keys is supposed to be lower case.
        /// </summary>
        
        private static readonly IDictionary<string, string> CategoryPropertyMapping = new Dictionary<string, string>()
        {
            {"bslhd", "Bshld" },
        };

        /// <summary>
        /// Get the mapping of <paramref name="propertyName"/> if one exists, else null.
        /// </summary>
        /// <param name="propertyName"> Name of the property to be updated. </param>
        /// <param name="logger"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="propertyName"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException">propertyName</exception>
        public static string GetUpdatedPropertyName(string propertyName, 
                                                    ILogHandler logHandler = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            string propertyNameLower = propertyName.ToLower();

            if (!CategoryPropertyMapping.ContainsKey(propertyNameLower))
            {
                return null;
            }

            string mappedName = CategoryPropertyMapping[propertyNameLower];
            logHandler?.ReportWarningFormat(Resources.MorphologyFileBackwardsCompatibilityHelper_GetUpdatedPropertyName_Backwards_Compatibility____0___has_been_updated_to___1__,
                                            propertyName, 
                                            mappedName);

            return mappedName;
        }

        /// <summary>
        /// The set of obsolete properties that should be removed from .mor files.
        /// </summary>
        /// <remarks>
        /// Each of these elements is supposed to be lowercase, as the .mor file is
        /// case insensitive.
        /// </remarks>
        private static readonly HashSet<string> obsoleteProperties = new HashSet<string>()
        {
            "neubcmud",
            "neubcsand",
            "eqmbc",
        };

        /// <summary>
        /// Determines whether the provided <paramref name="propertyName"/> is currently considered obsolete.
        /// </summary>
        /// <param name="propertyName">The property name to check.</param>
        /// <returns>
        /// <c>true</c> if the specified property name is obsolete; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsObsoletePropertyName(string propertyName)
        {
            return obsoleteProperties.Contains(propertyName.ToLowerInvariant());
        }
    }
}
