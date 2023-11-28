using System;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Converts enum values to and from string representations suitable for INI files.
    /// </summary>
    internal sealed class EnumIniValueConverter<T> : IIniValueConverter<T>
        where T : IConvertible
    {
        /// <inheritdoc />
        public string ConvertToString(T value)
            => value.ToString();

        /// <inheritdoc />
        public T ConvertFromString(string value)
        {
            Ensure.NotNull(value, nameof(value));

            try
            {
                var enumValue = (T)Enum.Parse(typeof(T), value, true);

                if (Enum.IsDefined(typeof(T), enumValue))
                {
                    return enumValue;
                }
            }
            catch
            {
                // ignore
            }

            throw new FormatException($"String '{value}' was not recognized as a valid enum.");
        }
    }
}