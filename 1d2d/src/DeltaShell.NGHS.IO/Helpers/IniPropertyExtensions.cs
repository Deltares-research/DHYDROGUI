using System;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils.Extensions;
using log4net;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class IniPropertyExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(IniPropertyExtensions));

        /// <summary>
        /// Reads the boolean value of the property.
        /// </summary>
        /// <param name="property"> The property to read the value from. </param>
        /// <returns> If convertible, the boolean value; otherwise, false. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="property"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Logs an error when the property value cannot be parsed to a boolean.
        /// </remarks>
        public static bool ReadBooleanValue(this IniProperty property)
        {
            Ensure.NotNull(property, nameof(property));

            if (property.Value.EqualsCaseInsensitive("true"))
            {
                return true;
            }

            if (property.Value.EqualsCaseInsensitive("false"))
            {
                return false;
            }

            if (int.TryParse(property.Value, out int intVal))
            {
                return intVal != 0;
            }

            log.Error(string.Format(Resources.IniPropertyExtensionMethods_Cannot_parse_value_for_property, 
                                    property.Value, nameof(Boolean), property.Key, property.LineNumber));
            return false;
        }
    }
}