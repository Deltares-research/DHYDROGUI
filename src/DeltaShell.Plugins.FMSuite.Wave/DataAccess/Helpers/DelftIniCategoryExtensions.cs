using System.Globalization;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers
{
    /// <summary>
    /// <see cref="DelftIniCategoryExtensions"/> contains extension methods for a <see cref="IniSection"/>.
    /// </summary>
    public static class DelftIniCategoryExtensions
    {
        private const string spatialDoubleFormat = "F7";

        /// <summary>
        /// Adds a property containing spatial data of a <see cref="Wave.Boundaries.IWaveBoundary"/>
        /// such as geometry or a support point distance. The specified <paramref name="value"/> will
        /// be formatted to a string with 7 decimals.
        /// </summary>
        /// <param name="section">The section which the property will be added to.</param>
        /// <param name="propertyKey">Key of the property.</param>
        /// <param name="value">The property value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="section"/> or <paramref name="propertyKey"/> is <c>null</c>.
        /// </exception>
        public static void AddSpatialProperty(this IniSection section, string propertyKey, double value)
        {
            Ensure.NotNull(section, nameof(section));
            Ensure.NotNull(propertyKey, nameof(propertyKey));

            var formattedValue = SpatialDouble.Round(value).ToString(spatialDoubleFormat, CultureInfo.InvariantCulture);

            section.AddProperty(propertyKey, formattedValue);
        }
    }
}