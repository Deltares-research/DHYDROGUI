using System;
using System.Globalization;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Converts date/time values to and from string representations suitable for INI files.
    /// </summary>
    internal sealed class DateTimeIniValueConverter : IIniValueConverter<DateTime>
    {
        /// <inheritdoc/>
        public string ConvertToString(DateTime value)
            => value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public DateTime ConvertFromString(string value)
            => DateTime.Parse(value, CultureInfo.InvariantCulture);
    }
}