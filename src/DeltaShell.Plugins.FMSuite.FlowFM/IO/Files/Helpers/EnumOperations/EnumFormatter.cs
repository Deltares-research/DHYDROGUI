using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.EnumOperations
{
    /// <summary>
    /// Class for enum formatting.
    /// </summary>
    public static class EnumFormatter
    {
        /// <summary>
        /// Gets a formatted string of enum value descriptions of an enum type.
        /// Enum values with an empty description are skipped.
        /// </summary>
        /// <typeparam name="TEnum"> The enum type. </typeparam>
        /// <returns>
        /// A string containing the enum descriptions separated by a comma and space.
        /// </returns>
        public static string GetFormattedDescriptions<TEnum>() where TEnum : Enum
        {
            IEnumerable<TEnum> values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            IEnumerable<string> descriptions = values.Select(v => v.GetDescription()).Where(d => !string.IsNullOrEmpty(d));
            return string.Join(", ", descriptions);
        }
    }
}