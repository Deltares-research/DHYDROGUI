using System;
using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
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
        /// <exception cref="ArgumentNullException">
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
        /// <exception cref="ArgumentNullException">
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
        /// <exception cref="ArgumentNullException">
        /// Thrown
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the number of <paramref name="polygonVertices"/> is smaller than three.
        /// </exception>
        internal static bool IsCounterClockwisePolygon(params Coordinate[] polygonVertices)
        {
            Ensure.NotNull(polygonVertices, nameof(polygonVertices));

            if (polygonVertices.Length < 3)
            {
                throw new InvalidOperationException("Cannot calculate the ordering of line segment or point");
            }

            // Assuming standard cartesian coordinates, we calculate the area
            // of the polygon defined by the polygonVertices. The sign of the
            // calculated value is determined by the traversal order. If the
            // vertices are ordered counter-clockwise then the summation will be
            // negative and if the ordering is clockwise, then the sign will be
            // negative. We leverage this fact to determine the ordering of the
            // polygonVertices.
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
        /// Gets the normalised normal associated with the vector defined
        /// by <paramref name="coordinate1"/> minus <paramref name="coordinate0"/>
        /// by rotating 90 degrees clockwise.
        /// </summary>
        /// <param name="coordinate0">The first coordinate.</param>
        /// <param name="coordinate1">The second coordinate.</param>
        /// <returns>
        /// The normal associated with the vector defined by
        /// <paramref name="coordinate1"/> minus <paramref name="coordinate0"/>.
        /// </returns>
        internal static Vector2D GetNormal(Coordinate coordinate0,
                                           Coordinate coordinate1) =>
            Vector2D.Create(coordinate1.Y - coordinate0.Y,
                            coordinate0.X - coordinate1.X).Normalize();

        /// <summary>
        /// Gets the value associated with the vector in the
        /// provided <paramref name="valueVectorPairs"/> closest aligned to the
        /// <paramref name="referenceVector"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="valueVectorPairs">The value-vector pairs.</param>
        /// <param name="referenceVector">The reference vector.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The <typeparamref name="T"/> value in <paramref name="valueVectorPairs"/> of
        /// which the vector aligns most with the provided <paramref name="referenceVector"/>.
        /// If <paramref name="valueVectorPairs"/> is empty, then <paramref name="defaultValue"/>
        /// is returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="valueVectorPairs"/> or <paramref name="referenceVector"/>
        /// is <c>null</c>.
        /// </exception>
        internal static T GetValueClosestAlignedWithVector<T>(IEnumerable<Tuple<T, Vector2D>> valueVectorPairs,
                                                              Vector2D referenceVector,
                                                              T defaultValue)
        {
            Ensure.NotNull(valueVectorPairs, nameof(valueVectorPairs));
            Ensure.NotNull(referenceVector, nameof(referenceVector));

            // The dot product between two normalized vectors is equal to cos theta
            // where theta is the angle between the vectors. Any dot product will
            // thus lie between -1 and 1, where the highest value will correspond with
            // the smallest angle between the referenceVector and the normal associated with
            // the value. We leverage this to find the result value.
            Vector2D referenceNormalized = referenceVector.Normalize();

            double largestDotProduct = -1.0;
            T result = defaultValue;

            foreach ((T value, Vector2D vector) in valueVectorPairs)
            {
                double dotProduct = vector.Normalize()
                                          .Dot(referenceNormalized);

                if (largestDotProduct >= dotProduct)
                {
                    continue;
                }

                largestDotProduct = dotProduct;
                result = value;
            }

            return result;
        }

        private static void Deconstruct<T1, T2>(this Tuple<T1, T2> tuple, out T1 t1, out T2 t2)
        {
            t1 = tuple.Item1;
            t2 = tuple.Item2;
        }
    }
}