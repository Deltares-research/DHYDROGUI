using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="IGridBoundary"/> specifies the grid boundaries given a grid.
    /// It further provides several convenience functions to ease the difficulty
    /// working with the boundaries of a grid.
    /// </summary>
    /// <remarks>
    /// The grid is assumed to be structured as follows.
    /// -p
    /// ( 0, Grid.NMax ) -- ( Grid.MMax, Grid.NMAX)
    /// ^   |                          |
    /// |   |                          | |
    /// |                          | v
    /// ( 0,         0 ) -- ( Grid.NMax, 0 )
    /// d-
    /// The coordinates are structured in a clock-wise fashion.
    /// </remarks>
    public interface IGridBoundary
    {
        /// <summary>
        /// Get the set of <see cref="GridBoundaryCoordinate"/> specifying the
        /// boundary of the grid at the specified <paramref name="gridSide"/>.
        /// </summary>
        /// <value>
        /// The <see cref="IReadOnlyList{T}"/> specifying the
        /// boundary of the grid at the specified <paramref name="gridSide"/>.
        /// </value>
        /// <param name="gridSide">The grid side.</param>
        /// <returns>
        /// The set of <see cref="GridBoundaryCoordinate"/> specifying the boundary of
        /// the grid at the specified <paramref name="gridSide"/>.
        /// </returns>
        IEnumerable<GridBoundaryCoordinate> this[GridSide gridSide] { get; }

        /// <summary>
        /// Gets the grid envelope starting from the west side, in a clock-wise fashion.
        /// </summary>
        /// <returns>
        /// The envelope of this <see cref="IGridBoundary"/> starting from the west side
        /// in a clock-wise fashion.
        /// </returns>
        IEnumerable<GridBoundaryCoordinate> GetGridEnvelope();

        /// <summary>
        /// Gets the world coordinate from boundary coordinate.
        /// </summary>
        /// <param name="boundaryCoordinate">The boundary coordinate.</param>
        /// <returns>
        /// The world coordinate location corresponding with <paramref name="boundaryCoordinate"/>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryCoordinate"/> is <c>null</c>.
        /// </exception>
        Coordinate GetWorldCoordinateFromBoundaryCoordinate(GridBoundaryCoordinate boundaryCoordinate);

        /// <summary>
        /// Gets the side closest aligned with the specified <paramref name="referenceNormal"/>.
        /// </summary>
        /// <param name="referenceNormal">The reference normal.</param>
        /// <returns>
        /// The <see cref="GridSide"/> closest aligned with the specified <paramref name="referenceNormal"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="referenceNormal"/> is <c>null</c>;
        /// </exception>
        GridSide GetSideAlignedWithNormal(Vector2D referenceNormal);
    }
}