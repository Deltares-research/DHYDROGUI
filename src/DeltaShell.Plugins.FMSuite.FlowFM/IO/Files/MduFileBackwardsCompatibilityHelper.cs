using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// Helper for backwards compatibility purposes concerning mdu files.
    /// </summary>
    public static class MduFileBackwardsCompatibilityHelper
    {
        /// <summary>
        /// The category property mapping. This contains the currently
        /// available Backwards Compatibility mappings.
        ///
        /// Each of the keys is supposed to be lower case.
        /// </summary>

        private static readonly IDictionary<string, string> categoryPropertyMapping = new Dictionary<string, string>
        {
            {"model", "General" },
            {"enclosurefile", "GridEnclosureFile"},
            {"trtdt", "DtTrt"},
            {"botlevuni", "BedLevUni"},
            {"botlevtype", "BedLevType"},
            {"mduformatversion", "FileVersion"}
        };

        /// <summary>
        /// Get the mapping of <paramref name="propertyName"/> if one exists, else null.
        /// </summary>
        /// <param name="propertyName"> Name of the property to be updated. </param>
        /// <param name="logHandler"> Optional logger to call if a mapping is returned. </param>
        /// <returns>
        /// IF a mapping for <paramref name="propertyName"/> exists THEN this mapping is returned,
        /// ELSE null.
        /// </returns>
        /// <exception cref="ArgumentNullException"> Thrown when <param name="propertyName"/> is null. </exception>
        public static string GetUpdatedPropertyName(string propertyName, ILogHandler logHandler = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            string propertyNameLower = propertyName.ToLower();

            if (!categoryPropertyMapping.ContainsKey(propertyNameLower))
            {
                return propertyName;
            }

            string mappedName = categoryPropertyMapping[propertyNameLower];
            logHandler?.ReportWarningFormat(Resources.MorphologyFileBackwardsCompatibilityHelper_GetUpdatedPropertyName_Backwards_Compatibility____0___has_been_updated_to___1__,
                                            propertyName,
                                            mappedName);

            return mappedName;
        }
    }
}