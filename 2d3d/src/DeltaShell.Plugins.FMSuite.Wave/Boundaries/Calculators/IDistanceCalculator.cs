using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="IDistanceCalculator"/> defines the methods to calculate
    /// the distance between two coordinates. The actual implementation might
    /// differ depending on the type of coordinate system (e.g. Cartesian
    /// coordinates, or spherical coordinates etc), and whether aspects like
    /// curvature of the earth should be taken into account or not.
    /// </summary>
    public interface IDistanceCalculator
    {
        /// <summary>
        /// Calculates the squared distance between <paramref name="coordinateA"/>
        /// and <paramref name="coordinateB"/>.
        /// </summary>
        /// <param name="coordinateA">The coordinate a.</param>
        /// <param name="coordinateB">The coordinate b.</param>
        /// <returns>
        /// The squared distance between <paramref name="coordinateA"/> and
        /// <paramref name="coordinateB"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        double CalculateDistanceSquared(Coordinate coordinateA, Coordinate coordinateB);

        /// <summary>
        /// Calculates the distance between <paramref name="coordinateA"/> and
        /// <paramref name="coordinateB"/>.
        /// </summary>
        /// <param name="coordinateA">The coordinate a.</param>
        /// <param name="coordinateB">The coordinate b.</param>
        /// <returns>
        /// The distance between <paramref name="coordinateA"/> and
        /// <paramref name="coordinateB"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        double CalculateDistance(Coordinate coordinateA, Coordinate coordinateB);
    }
}