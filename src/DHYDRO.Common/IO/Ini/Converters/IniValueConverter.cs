using System;
using System.Collections.Generic;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Provides methods for converting values from and to string representations suitable for INI files.
    /// </summary>
    internal static class IniValueConverter
    {
        private static readonly Dictionary<Type, object> converters = new Dictionary<Type, object>();

        /// <summary>
        /// Converts the specified value to its string representation for INI serialization.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <typeparam name="T">The type of the value to convert.</typeparam>
        /// <returns>The string representation of the value.</returns>
        public static string ConvertToString<T>(T value)
            where T : IConvertible
        {
            return Create<T>().ConvertToString(value);
        }

        /// <summary>
        /// Converts the specified string representation to a value of the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <param name="value">The string representation of the value.</param>
        /// <returns>The converted value of the specified type.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When <paramref name="value"/> does not represent a valid format.</exception>
        public static T ConvertFromString<T>(string value)
            where T : IConvertible
        {
            Ensure.NotNull(value, nameof(value));

            return Create<T>().ConvertFromString(value.Trim());
        }

        private static IIniValueConverter<T> Create<T>()
            where T : IConvertible
        {
            object converter;

            if (typeof(T) == typeof(bool))
            {
                converter = ResolveConverter(typeof(BooleanIniValueConverter));
            }
            else if (typeof(T) == typeof(double))
            {
                converter = ResolveConverter(typeof(DoubleIniValueConverter));
            }
            else if (typeof(T) == typeof(float))
            {
                converter = ResolveConverter(typeof(FloatIniValueConverter));
            }
            else if (typeof(T) == typeof(DateTime))
            {
                converter = ResolveConverter(typeof(DateTimeIniValueConverter));
            }
            else if (typeof(T).IsEnum)
            {
                converter = ResolveConverter(typeof(EnumIniValueConverter<>).MakeGenericType(typeof(T)));
            }
            else
            {
                converter = ResolveConverter(typeof(DefaultIniValueConverter<T>));
            }

            return (IIniValueConverter<T>)converter;
        }

        private static object ResolveConverter(Type converterType)
        {
            if (converters.TryGetValue(converterType, out object converter))
            {
                return converter;
            }

            converter = Activator.CreateInstance(converterType);
            converters.Add(converterType, converter);

            return converter;
        }
    }
}