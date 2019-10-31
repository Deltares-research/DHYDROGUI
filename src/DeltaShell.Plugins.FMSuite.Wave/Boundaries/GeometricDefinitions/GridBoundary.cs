using System;
using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridBoundary"/> defines the grid boundaries given a grid.
    /// It further provides several convenience functions to ease the difficulty
    /// working with the boundaries of a grid.
    /// </summary>
    public class GridBoundary
    {
        private readonly IDiscreteGridPointCoverage observedGrid;
        private readonly IDictionary<GridSide, IReadOnlyList<GridCoordinate>> boundaries;


        /// <summary>
        /// Creates a new instance of the <see cref="GridBoundary"/>.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any argument is <c>null</c>.
        /// </exception>
        public GridBoundary(IDiscreteGridPointCoverage grid)
        {
            observedGrid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        /// <summary>
        /// Get the set of <see cref="GridCoordinate"/> specifying the boundary
        /// of the grid at the specified <paramref name="gridSide"/>.
        /// </summary>
        /// <value>
        /// The <see cref="IReadOnlyList{GridCoordinate}"/> specifying the
        /// boundary of the grid at the specified <paramref name="gridSide"/>.
        /// </value>
        /// <param name="gridSide">The grid side.</param>
        /// <returns>
        /// The set of <see cref="GridCoordinate"/> specifying the boundary of
        /// the grid at the specified <paramref name="gridSide"/>.
        /// </returns>
        public IReadOnlyList<GridCoordinate> this[GridSide gridSide] => boundaries[gridSide];
    }
}