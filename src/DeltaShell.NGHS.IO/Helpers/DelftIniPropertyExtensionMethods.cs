using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class DelftIniPropertyExtensionMethods
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DelftIniPropertyExtensionMethods));
        
        public static double[] ParseDoublesFromPropertyValue(this IDelftIniProperty property)
        {
            var propertyStringValues = property.Value.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var propertyDoubleValues = new List<double>();
            foreach (var propertyString in propertyStringValues)
            {
                double propertyDouble;
                if (double.TryParse(propertyString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out propertyDouble))
                {
                    propertyDoubleValues.Add(propertyDouble);
                }
            }
            return propertyDoubleValues.ToArray();
        }

        /// <summary>
        /// Reads the value of the property and converts it to type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="property"> The property to read the value from. </param>
        /// <typeparam name="T"> The converted value type. </typeparam>
        /// <returns> If parsable, the converted value; otherwise, the default value of <typeparamref name="T"/>. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="property"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Logs an error when the property value cannot be parsed to type <typeparamref name="T"/>.
        /// </remarks>>
        public static T ReadValue<T>(this IDelftIniProperty property)
        {
            Ensure.NotNull(property, nameof(property));

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.IsValid(property.Value))
            {
                return (T) converter.ConvertFromInvariantString(property.Value);
            }

            log.Error(string.Format(Resources.DelftIniPropertyExtensionMethods_Cannot_parse_value_for_property, 
                                    property.Value, typeof(T).Name, property.Name, property.LineNumber));
            return default(T);

        }
        
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
        public static bool ReadBooleanValue(this IDelftIniProperty property)
        {
            Ensure.NotNull(property, nameof(property));

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(bool));
            if (converter.IsValid(property.Value))
            {
                return (bool) converter.ConvertFromInvariantString(property.Value);
            }

            converter = TypeDescriptor.GetConverter(typeof(int));
            if (converter.IsValid(property.Value))
            {
                return Convert.ToBoolean(converter.ConvertFromInvariantString(property.Value));
            }

            log.Error(string.Format(Resources.DelftIniPropertyExtensionMethods_Cannot_parse_value_for_property, 
                                    property.Value, nameof(Boolean), property.Name, property.LineNumber));
            return false;

        }
    }
}