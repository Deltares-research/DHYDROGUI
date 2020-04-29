using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="CartesianOrientationCalculatorHelper"/> implements several geometric
    /// methods related to the orientation of Cartesian grids.
    /// </summary>

    internal static class CartesianOrientationCalculatorHelper
    {
        /// <summary>
        /// Gets the world coordinate at the specified <paramref name="x"/>
        /// and <paramref name="y"/> of the <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// A new <see cref="Coordinate"/> containing the world coordinate x and y values.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="grid"/> is <c>null</c>.
        /// </exception>
        internal static Coordinate GetCoordinateAt(this IDiscreteGridPointCoverage grid, int x, int y)
        {
            Ensure.NotNull(grid, nameof(grid));
            return new Coordinate(grid.X.Values[x, y], grid.Y.Values[x, y]);
        }

        /// <summary>
        /// Gets the world coordinate at the specified grid <paramref name="coordinate"/>
        /// of the <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="coordinate">The coordinate.</param>
        /// <returns>
        /// A new <see cref="Coordinate"/> containing the world coordinate x and y values.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="grid"/> or <paramref name="coordinate"/> is <c>null</c>.
        /// </exception>
        internal static Coordinate GetCoordinateAt(this IDiscreteGridPointCoverage grid, GridCoordinate coordinate)
        {
            Ensure.NotNull(coordinate, nameof(coordinate));
            return grid.GetCoordinateAt(coordinate.X, coordinate.Y);
        }

        /// <summary>
        /// Determines whether the specified polygon vertices are ordered counter-clockwise.
        /// </summary>
        /// <param name="polygonVertices">The polygon vertices.</param>
        /// <returns>
        /// <c>true</c> if the polygon vertices are ordered counter-clockwise; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the number of <paramref name="polygonVertices"/> is smaller than three.
        /// </exception>
        internal static bool IsCounterClockwisePolygon(params Coordinate[] polygonVertices)
        {
            Ensure.NotNull(polygonVertices, nameof(polygonVertices));

            if (polygonVertices.Length < 3)
            {
                throw new System.InvalidOperationException("Cannot calculate the ordering of line segment or point");
            }

            var sum = 0.0;

            for (var i = 1; i <= polygonVertices.Length; i++)
            {
                Coordinate coordinate1 = polygonVertices[i % polygonVertices.Length];
                Coordinate coordinate0 = polygonVertices[i - 1];

                sum += (coordinate1.X - coordinate0.X) * (coordinate1.Y + coordinate0.Y);
            }

            return sum < 0.0;
        }

        /// <summary>
        /// Gets the normalised normal associated with the line segment defined
        /// by <paramref name="coordinate0"/> and <paramref name="coordinate1"/>.
        /// </summary>
        /// <param name="coordinate0">The coordinate0.</param>
        /// <param name="coordinate1">The coordinate1.</param>
        /// <returns>
        /// The normal associated with the line segment defined by
        /// <paramref name="coordinate0"/> and <paramref name="coordinate1"/>.
        /// </returns>
        internal static Vector2D GetNormal(Coordinate coordinate0,
                                           Coordinate coordinate1) =>
            Vector2D.Create(coordinate1.Y - coordinate0.Y, 
                            coordinate0.X - coordinate1.X).Normalize();
    }
}