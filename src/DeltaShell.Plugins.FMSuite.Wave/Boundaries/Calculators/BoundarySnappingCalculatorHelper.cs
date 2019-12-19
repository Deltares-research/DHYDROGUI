using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="BoundarySnappingCalculatorHelper"/> contains several more
    /// complex functions required by the BoundarySnappingCalculator, which should
    /// not be part of the public API.
    /// </summary>
    internal static class BoundarySnappingCalculatorHelper
    {
        /// <summary>
        /// Finds the distance and sequence of indices of the coordinates within <paramref name="coordinates"/>
        /// closest to <paramref name="coordinateSrc"/>, given <paramref name="distanceCalculator"/>.
        /// </summary>
        /// <param name="distanceCalculator">The distance calculator.</param>
        /// <param name="coordinateSrc">The coordinate source.</param>
        /// <param name="coordinates">The sequence of coordinates to evaluate the distance to.</param>
        /// <returns>
        /// The minimal distance found between <paramref name="coordinates"/> and <paramref name="coordinateSrc"/>,
        /// and the sequence of indices of coordinates in <paramref name="coordinates"/> that are at this distance
        /// given the <see cref="distanceCalculator"/>.
        /// <remarks>
        /// It is a assumed that no parameter is <c>null</c>, the behaviour is undefined in case this happens.
        /// </remarks>
        internal static Tuple<IEnumerable<int>, double> FindClosestIndices(IDistanceCalculator distanceCalculator,
                                                                         Coordinate coordinateSrc,
                                                                         IList<Coordinate> coordinates)
        {
            double smallestDistanceSq = double.PositiveInfinity;
            var smallestIndices = new List<int>();

            for (var i = 0; i < coordinates.Count; i++)
            {
                double newDistSq =
                    distanceCalculator.CalculateDistance(coordinateSrc,
                                                         coordinates[i]);

                if (newDistSq > smallestDistanceSq)
                {
                    continue;
                }

                if (newDistSq < smallestDistanceSq)
                {
                    smallestDistanceSq = newDistSq;
                    smallestIndices.Clear();
                }

                smallestIndices.Add(i);
            }

            return new Tuple<IEnumerable<int>, double>(smallestIndices, smallestDistanceSq);
        }

        /// <summary>
        /// Determines whether this <paramref name="coordinate"/> is defined.
        /// </summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <returns>
        ///   <c>true</c> if the specified coordinate is defined; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The behaviour of this method is undefined if <paramref name="coordinate"/> is <c>null</c>.
        /// </remarks>
        internal static bool IsDefined(this Coordinate coordinate)
        {
            return !(double.IsNaN(coordinate.X) ||
                     double.IsNaN(coordinate.Y) ||
                     double.IsInfinity(coordinate.X) ||
                     double.IsInfinity(coordinate.Y));
        }
    }
}