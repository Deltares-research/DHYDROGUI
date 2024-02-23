using System;
using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.Extensions;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Converts enum values to and from string representations suitable for INI files.
    /// </summary>
    internal sealed class EnumIniValueConverter<T> : IIniValueConverter<T>
        where T : Enum
    {
        /// <inheritdoc/>
        public string ConvertToString(T value)
            => value.ToString();

        /// <inheritdoc/>
        public T ConvertFromString(string value)
        {
            if (TryParseByValue(value, out T enumValue) || TryGetByDescription(value, out enumValue))
            {
                return enumValue;
            }

            throw new FormatException($"String '{value}' was not recognized as a valid enum.");
        }

        private static bool TryParseByValue(string value, out T enumValue)
        {
            try
            {
                enumValue = (T)Enum.Parse(typeof(T), value, true);
                return Enum.IsDefined(typeof(T), enumValue);
            }
            catch
            {
                enumValue = default;
                return false;
            }
        }

        private static bool TryGetByDescription(string description, out T enumValue)
        {
            IEnumerable<T> enumValues = Enum.GetValues(typeof(T)).Cast<T>();
            IEnumerable<T> matches = enumValues.Where(x => description.EqualsCaseInsensitive(x.GetDescription())).ToArray();

            enumValue = matches.FirstOrDefault();
            return matches.Any();
        }
    }
}