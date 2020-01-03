using System;
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="coordinateToSnap"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// If tolerance is <c>null</c>, then it will not be taken into account.
        /// </remarks>
        IEnumerable<GridBoundaryCoordinate> SnapCoordinateToGridBoundaryCoordinate(Coordinate coordinateToSnap,
                                                                                   double? tolerance = null);

        /// <summary>
        /// Gets the coordinate at the given <see cref="distance"/> from
        /// the start of the <see cref="IWaveBoundaryGeometricDefinition"/>.
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <param name="gridSide">The grid side.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="distance"/> is smaller than 0.
        /// </exception>
        /// <returns>
        /// The coordinate at the given <paramref name="distance"/>.
        /// </returns>
        Coordinate CalculateCoordinateFromDistance(double distance, GridSide gridSide);
    }
}