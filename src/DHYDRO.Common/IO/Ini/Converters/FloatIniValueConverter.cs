using System.Globalization;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.IO.Ini.Converters
{
    /// <summary>
    /// Converts float values to and from string representations suitable for INI files.
    /// </summary>
    internal sealed class FloatIniValueConverter : IIniValueConverter<float>
    {
        /// <inheritdoc />
        public string ConvertToString(float value)
            => value.ToString("e7", CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public float ConvertFromString(string value)
        {
            Ensure.NotNull(value, nameof(value));

            return float.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}