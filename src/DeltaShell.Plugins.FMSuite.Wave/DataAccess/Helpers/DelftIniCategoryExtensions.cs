using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers
{
    /// <summary>
    /// <see cref="DelftIniCategoryExtensions"/> contains extension methods for a <see cref="DelftIniCategory"/>.
    /// </summary>
    public static class DelftIniCategoryExtensions
    {
        private const string spatialDoubleFormat = "F7";

        /// <summary>
        /// Adds a property containing spatial data of a <see cref="Wave.Boundaries.IWaveBoundary"/>
        /// such as geometry or a support point distance. The specified <paramref name="value"/> will
        /// be formatted to a string with 7 decimals.
        /// </summary>
        /// <param name="category">The category which the property will be added to.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The property value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="category"/> or <paramref name="propertyName"/> is <c>null</c>.
        /// </exception>
        public static void AddSpatialProperty(this DelftIniCategory category, string propertyName, double value)
        {
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(propertyName, nameof(propertyName));

            category.AddProperty(propertyName, SpatialDouble.Round(value), null, spatialDoubleFormat);
        }
    }
}