using System;
using System.Collections.Generic;
using System.Globalization;

namespace DeltaShell.NGHS.IO.Ini
{
    /// <summary>
    /// Provides methods for converting values from and to string representations suitable for INI files.
    /// </summary>
    public static class IniValueConverter
    {
        private static readonly Dictionary<Type, string> formatStrings = new Dictionary<Type, string>
        {
            { typeof(int), "G" },
            { typeof(float), "e7" },
            { typeof(double), "e7" },
            { typeof(string), "" },
            { typeof(DateTime), "yyyy-MM-dd HH:mm:ss" }
        };

        /// <summary>
        /// Converts the specified value to its string representation for INI serialization.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <typeparam name="T">The type of the value to convert.</typeparam>
        /// <returns>The string representation of the value.</returns>
        public static string ConvertToString<T>(T value)
            where T : IConvertible
        {
            if (formatStrings.TryGetValue(typeof(T), out string format))
            {
                return string.Format(CultureInfo.InvariantCulture, $"{{0:{format}}}", value);
            }

            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the specified string representation to a value of the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <param name="value">The string representation of the value.</param>
        /// <returns>The converted value of the specified type.</returns>
        public static T ConvertFromString<T>(string value)
            where T : IConvertible
        {
            Type targetType = typeof(T);

            if (targetType.IsEnum)
            {
                return (T)Enum.Parse(targetType, value, true);
            }

            return (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
    }
}