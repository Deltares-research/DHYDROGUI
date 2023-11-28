using System;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Specifies an interface for converting values from and to string representations suitable for INI files.
    /// </summary>
    /// <typeparam name="T">The type of the value to convert from/to.</typeparam>
    internal interface IIniValueConverter<T>
        where T : IConvertible
    {
        /// <summary>
        /// Converts the specified value to its string representation for INI serialization.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string representation of the value.</returns>
        string ConvertToString(T value);

        /// <summary>
        /// Converts the specified string representation to a value of the specified type.
        /// </summary>
        /// <param name="value">The string representation of the value.</param>
        /// <returns>The converted value of the specified type.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">When <paramref name="value"/> does not represent a valid format.</exception>
        T ConvertFromString(string value);
    }
}