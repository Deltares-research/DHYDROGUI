using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="IBoundarySnappingCalculator"/> defines a set of equations
    /// to snap coordinates to a <see cref="GridBoundary"/>. 
    /// </summary>
    public interface IBoundarySnappingCalculator
    {
        /// <summary>
        /// Gets or sets the <see cref="GridBoundary"/> of this <see cref="GridBoundary"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a <c>null</c> value is set.
        /// </exception>
        GridBoundary GridBoundary { get; set; }

        /// <summary>
        /// Gets the <see cref="IDistanceCalculator"/> used to calculate the
        /// distances between points.
        /// </summary>
        /// <value>
        /// The distance calculator.
        /// </value>
        /// <remarks>
        /// The type of <see cref="IDistanceCalculator"/> should be dependent on
        /// the type of coordinate system of the <see cref="GridBoundary"/>
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
        /// Thrown when any parameter is null.
        /// </exception>
        /// <remarks>
        /// If tolerance is <c>null</c>, then it will not be taken into account.
        /// </remarks>
        IEnumerable<GridBoundaryCoordinate> SnapCoordinateToGridBoundaryCoordinate(Coordinate coordinateToSnap,
                                                                                   double? tolerance = null);
    }
}