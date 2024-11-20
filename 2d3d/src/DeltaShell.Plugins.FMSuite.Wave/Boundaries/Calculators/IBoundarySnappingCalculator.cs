using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="IBoundarySnappingCalculator"/> defines a set of equations
    /// to snap coordinates to a <see cref="IGridBoundary"/>.
    /// </summary>
    public interface IBoundarySnappingCalculator
    {
        /// <summary>
        /// Gets the <see cref="IGridBoundary"/> of this <see cref="IGridBoundary"/>.
        /// </summary>
        IGridBoundary GridBoundary { get; }

        /// <summary>
        /// Gets the <see cref="IDistanceCalculator"/> used to calculate the
        /// distances between points.
        /// </summary>
        /// <value>
        /// The distance calculator.
        /// </value>
        /// <remarks>
        /// The type of <see cref="IDistanceCalculator"/> should be dependent on
        /// the type of coordinate system of the <see cref="IGridBoundary"/>
        /// </remarks>
        IDistanceCalculator DistanceCalculator { get; }

        /// <summary>
        /// Calculates and returns the set of closest <see cref="GridBoundaryCoordinate"/>
        /// to the provided <paramref name="coordinateToSnap"/> within the observed grid.
        /// </summary>
        /// <param name="coordinateToSnap">The coordinate to snap to the grid.</param>
        /// <param name="tolerance">The allowed tolerance.</param>
        /// <returns>
        /// The set of closest <see cref="GridBoundaryCoordinate"/> if the provided coordinate
        /// can be snapped, an empty list otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="coordinateToSnap"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// If tolerance is <c>null</c>, then it will not be taken into account.
        /// </remarks>
        IEnumerable<GridBoundaryCoordinate> SnapCoordinateToGridBoundaryCoordinate(Coordinate coordinateToSnap,
                                                                                   double? tolerance = null);

        /// <summary>
        /// Gets the coordinate that corresponds with the given <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPoint"/> is <c>null</c>.
        /// </exception>
        /// <returns>
        /// The coordinate of the location of the <paramref name="supportPoint"/>.
        /// </returns>
        Coordinate CalculateCoordinateFromSupportPoint(SupportPoint supportPoint);

        /// <summary>
        /// Calculates the distance between two boundary indices.
        /// </summary>
        /// <param name="indexA">The first index.</param>
        /// <param name="indexB">The second index.</param>
        /// <param name="gridSide">The grid side.</param>
        /// <returns>
        /// The distance between <paramref name="indexA"/> and <paramref name="indexB"/>.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="gridSide"/> is not defined.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="indexA"/> or <paramref name="indexB"/> are smaller than 0
        /// or when they are equal to or larger than the number of coordinates of the <see cref="IGridBoundary"/>
        /// at the specified
        /// <param name="gridSide"></param>
        /// </exception>
        double CalculateDistanceBetweenBoundaryIndices(int indexA, int indexB, GridSide gridSide);
    }
}