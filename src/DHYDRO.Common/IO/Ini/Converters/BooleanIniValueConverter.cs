using System;
using DHYDRO.Common.Extensions;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Converts boolean values to and from string representations suitable for INI files.
    /// </summary>
    internal sealed class BooleanIniValueConverter : IIniValueConverter<bool>
    {
        /// <inheritdoc/>
        public string ConvertToString(bool value)
            => Convert.ToString(value);

        /// <inheritdoc/>
        public bool ConvertFromString(string value)
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }

            if (int.TryParse(value, out int flag))
            {
                return Convert.ToBoolean(flag);
            }

            if (value.EqualsCaseInsensitive("yes"))
            {
                return true;
            }

            if (value.EqualsCaseInsensitive("no"))
            {
                return false;
            }

            throw new FormatException($"String '{value}' was not recognized as a valid boolean.");
        }
    }
}