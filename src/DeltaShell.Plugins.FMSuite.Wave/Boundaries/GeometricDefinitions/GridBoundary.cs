using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridBoundary"/> defines the grid boundaries given a grid.
    /// It further provides several convenience functions to ease the difficulty
    /// working with the boundaries of a grid.
    /// </summary>
    /// <remarks>
    /// The grid is assumed to be structured as follows.
    ///
    ///                    -p
    ///   ( 0, Grid.NMax ) -- ( Grid.MMax, Grid.NMAX)
    /// ^   |                          |
    /// |   |                          | |
    ///     |                          | v 
    ///   ( 0,         0 ) -- ( Grid.NMax, 0 )
    ///                    d-
    ///
    /// The coordinates are structured in a clock-wise fashion.
    /// </remarks>
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
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="grid"/> has a <c>Size1</c> or
        /// <c>Size2</c> of smaller than 2.
        /// </exception>
        public GridBoundary(IDiscreteGridPointCoverage grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));

            }

            if (grid.Size1 < 2 || grid.Size2 < 2)
            {
                throw new ArgumentException($"{nameof(grid)} should contain at least 2 points in each dimension.");
            }

            observedGrid = grid;
            boundaries = new Dictionary<GridSide, IReadOnlyList<GridCoordinate>>
            {
                {GridSide.West,  GetBoundaryAtSide(GridSide.West).ToList()  },
                {GridSide.North, GetBoundaryAtSide(GridSide.North).ToList() },
                {GridSide.East,  GetBoundaryAtSide(GridSide.East).ToList()  },
                {GridSide.South, GetBoundaryAtSide(GridSide.South).ToList() },
            };
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

        private IEnumerable<GridCoordinate> GetBoundaryAtSide(GridSide side)
        {
            int sizeM = observedGrid.Size1;
            int sizeN = observedGrid.Size2;

            int mMax = sizeM - 1;
            int nMax = sizeN - 1;

            IEnumerable<GridCoordinate> allCoordinates;
            switch (side)
            {
                case GridSide.West:
                    allCoordinates = Enumerable.Range(0, sizeN)
                                               .Select(n => new GridCoordinate(0, n));
                    break;
                case GridSide.North:
                    allCoordinates = Enumerable.Range(0, sizeM)
                                               .Select(m => new GridCoordinate(m, nMax));
                    break;
                case GridSide.East:
                    allCoordinates = Enumerable.Range(0, sizeN)
                                               .Select(n => new GridCoordinate(mMax, nMax - n));
                    break;
                case GridSide.South:
                    allCoordinates = Enumerable.Range(0, sizeM)
                                               .Select(m => new GridCoordinate(mMax - m, 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return allCoordinates.Where(c => !IsDryPoint(c));
        }

        private bool IsDryPoint(GridCoordinate gridCoordinate)
        {
            double x = observedGrid.X.Values[gridCoordinate.X, gridCoordinate.Y];
            double y = observedGrid.Y.Values[gridCoordinate.X, gridCoordinate.Y];

            return WaveDomainHelper.IsDryPoint(x, y);
        }
    }
}