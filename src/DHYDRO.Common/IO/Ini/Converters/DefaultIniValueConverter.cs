using System;
using System.Globalization;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Converts values of a specified type to and from string representations suitable for INI files using the default
    /// conversion methods.
    /// </summary>
    /// <typeparam name="T">The type of the value to convert from/to.</typeparam>
    internal sealed class DefaultIniValueConverter<T> : IIniValueConverter<T>
        where T : IConvertible
    {
        /// <inheritdoc/>
        public string ConvertToString(T value)
            => Convert.ToString(value, CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public T ConvertFromString(string value)
            => (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }
}