using System;
using DelftTools.Hydro.Properties;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Geometries
{
    /// <summary>
    /// Contains extensions methods for <see cref="IBranch"/> for geometric calculations.
    /// </summary>
    public static class BranchExtensions
    {
        /// <summary>
        /// Gets the coordinates at the specified <paramref name="chainage"/> on the <paramref name="branch"/>.
        /// </summary>
        /// <param name="branch"> The branch for which to calculate the coordinate. </param>
        /// <param name="chainage"> The chainage at which to calculate the coordinate. </param>
        /// <returns> The coordinate at the specified <paramref name="chainage"/>. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="branch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="chainage"/> is negative, <see cref="double.NaN"/>
        /// or <see cref="double.IsInfinity"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the node coordinates of the <paramref name="branch"/> cannot be retrieved,
        /// for example when the branch does not have any nodes.
        /// </exception>
        public static Coordinate GetCoordinate(this IBranch branch, double chainage)
        {
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNegative(chainage, nameof(chainage));
            Ensure.NotNaN(chainage, nameof(chainage));
            Ensure.NotInfinity(chainage, nameof(chainage));

            Coordinate a = branch.Source?.Geometry?.Coordinate;
            Coordinate b = branch.Target?.Geometry?.Coordinate;

            ThrowIfNull(a, branch.Name);
            ThrowIfNull(b, branch.Name);

            return GetCoordinateAtDistance(a, b, chainage);
        }

        private static void ThrowIfNull(Coordinate c, string branchName)
        {
            if (c == null)
            {
                throw new ArgumentException(string.Format(Resources.BranchExtensions_Cannot_determine_node_coordinates, branchName));
            }
        }

        private static Coordinate GetCoordinateAtDistance(Coordinate a, Coordinate b, double distance)
        {
            if (distance == 0)
            {
                return new Coordinate(a.X, a.Y);
            }

            double dX = a.X - b.X;
            double dY = a.Y - b.Y;

            double lineLength = Math.Sqrt((dX * dX) + (dY * dY));
            double lineSlope = dY / dX;
            double lineIntercept = a.Y - (lineSlope * a.X);

            double x = a.X - ((distance * dX) / lineLength);
            double y = (lineSlope * x) + lineIntercept;

            return new Coordinate(x, y);
        }
    }
}