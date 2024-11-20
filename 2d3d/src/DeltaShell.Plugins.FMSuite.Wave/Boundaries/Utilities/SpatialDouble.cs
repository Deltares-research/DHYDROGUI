using System;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities
{
    /// <summary>
    /// <see cref="SpatialDouble"/> contains methods for handling double
    /// values regarding spatial data of the <see cref="IWaveBoundary"/>
    /// such as the geometry or the distance of a <see cref="GeometricDefinitions.SupportPoint"/>.
    /// </summary>
    public static class SpatialDouble
    {
        private const int nDecimalPlaces = 7;

        /// <summary>
        /// Rounds the specified <paramref name="value"/> to a double
        /// with a maximum of 7 decimal places.
        /// </summary>
        /// <param name="value">The value to be rounded.</param>
        /// <returns>The rounded value.</returns>
        public static double Round(double value)
        {
            return Math.Round(value, nDecimalPlaces, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Compares <paramref name="valueA"/> and <paramref name="valueB"/> on equality
        /// with a precision up to 1E-7.
        /// </summary>
        /// <param name="valueA">The first value.</param>
        /// <param name="valueB">The second value.</param>
        /// <return>
        /// A boolean describing whether <paramref name="valueA"/> and <paramref name="valueB"/> are equal.
        /// </return>
        public static bool AreEqual(double valueA, double valueB)
        {
            // there is precision loss when taking the absolute difference,
            // so we compare the rounded values.
            return Round(valueA) == Round(valueB);
        }
    }
}