using System.Globalization;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Converts double values to and from string representations suitable for INI files.
    /// </summary>
    internal sealed class DoubleIniValueConverter : IIniValueConverter<double>
    {
        /// <inheritdoc />
        public string ConvertToString(double value)
            => value.ToString("e7", CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public double ConvertFromString(string value)
        {
            Ensure.NotNull(value, nameof(value));

            return double.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}