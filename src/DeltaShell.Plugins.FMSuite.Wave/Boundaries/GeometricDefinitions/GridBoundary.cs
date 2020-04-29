using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridBoundary"/> defines the grid boundaries given a grid.
    /// It further provides several convenience functions to ease the difficulty
    /// working with the boundaries of a grid.
    /// </summary>
    public class GridBoundary : IGridBoundary
    {
        private readonly IDiscreteGridPointCoverage observedGrid;
        private readonly IReadOnlyDictionary<GridSide, IReadOnlyList<GridCoordinate>> boundaries;

        /// <summary>
        /// Creates a new instance of the <see cref="GridBoundary"/>.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="grid"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the <paramref name="grid"/> has a <c>Size1</c> or
        /// <c>Size2</c> of smaller than 2.
        /// </exception>
        public GridBoundary(IDiscreteGridPointCoverage grid)
        {
            Ensure.NotNull(grid, nameof(grid));
            if (grid.Size1 < 2 || grid.Size2 < 2)
            {
                throw new System.ArgumentException($"{nameof(grid)} should contain at least 2 points in each dimension.");
            }

            observedGrid = grid;

            IReadOnlyList<GridCoordinate>[] sides = GetOrderedSides();

            int worldEastIndex = GetIndexOfWorldEast(sides);

            boundaries = new Dictionary<GridSide, IReadOnlyList<GridCoordinate>>
            {
                {GridSide.East,  sides[worldEastIndex]},
                {GridSide.North, sides[(worldEastIndex + 1) % 4]},
                {GridSide.West,  sides[(worldEastIndex + 2) % 4]},
                {GridSide.South, sides[(worldEastIndex + 3) % 4]},
            };
        }

        public IEnumerable<GridBoundaryCoordinate> this[GridSide gridSide] =>
            Enumerable.Range(0, boundaries[gridSide].Count).Select(x => new GridBoundaryCoordinate(gridSide, x));

        public IEnumerable<GridBoundaryCoordinate> GetGridEnvelope()
        {
            GridSide[] sides =
            {
                GridSide.East,
                GridSide.North,
                GridSide.West,
                GridSide.South
            };

            return sides.SelectMany(x => this[x]);
        }

        public Coordinate GetWorldCoordinateFromBoundaryCoordinate(GridBoundaryCoordinate boundaryCoordinate)
        {
            Ensure.NotNull(boundaryCoordinate, nameof(boundaryCoordinate));

            GridCoordinate gridCoordinate = boundaries[boundaryCoordinate.GridSide]
                                                      [boundaryCoordinate.Index];
            return observedGrid.GetCoordinateAt(gridCoordinate);
        }

        private IReadOnlyList<GridCoordinate>[] GetOrderedSides()
        {
            int sizeM = observedGrid.Size1;
            int sizeN = observedGrid.Size2;

            int mMax = sizeM - 1;
            int nMax = sizeN - 1;


            bool isCounterClockWise = CartesianOrientationCalculatorHelper.IsCounterClockwisePolygon(observedGrid.GetCoordinateAt(0, 0),
                                                                                                     observedGrid.GetCoordinateAt(mMax, 0),
                                                                                                     observedGrid.GetCoordinateAt(mMax, nMax),
                                                                                                     observedGrid.GetCoordinateAt(0, nMax));

            IEnumerable<GridCoordinate> coordinatesGridEast = Enumerable.Range(0, sizeN)
                                                                        .Select(n => new GridCoordinate(mMax, n))
                                                                        .Where(x => !IsDryPoint(x));
            IEnumerable<GridCoordinate> coordinatesGridNorth = Enumerable.Range(0, sizeM)
                                                                         .Select(m => new GridCoordinate(mMax - m, nMax))
                                                                         .Where(x => !IsDryPoint(x));
            IEnumerable<GridCoordinate> coordinatesGridWest = Enumerable.Range(0, sizeN)
                                                                        .Select(n => new GridCoordinate(0, nMax - n))
                                                                        .Where(x => !IsDryPoint(x));
            IEnumerable<GridCoordinate> coordinatesGridSouth = Enumerable.Range(0, sizeM)
                                                                         .Select(m => new GridCoordinate(m, 0))
                                                                         .Where(x => !IsDryPoint(x));

            IReadOnlyList<GridCoordinate>[] sides;

            if (isCounterClockWise)
            {
                sides = new IReadOnlyList<GridCoordinate>[]
                {
                    coordinatesGridEast.ToList(),
                    coordinatesGridNorth.ToList(),
                    coordinatesGridWest.ToList(),
                    coordinatesGridSouth.ToList(),
                };
            }
            else
            {
                sides = new IReadOnlyList<GridCoordinate>[]
                {
                    coordinatesGridEast.Reverse().ToList(),
                    coordinatesGridSouth.Reverse().ToList(),
                    coordinatesGridWest.Reverse().ToList(),
                    coordinatesGridNorth.Reverse().ToList(),
                };
            }

            return sides;
        }

        private int GetIndexOfWorldEast(IEnumerable<IReadOnlyList<GridCoordinate>> sides)
        {
            Vector2D GetNormalFromSide(IReadOnlyList<GridCoordinate> x) =>
                CartesianOrientationCalculatorHelper.GetNormal(observedGrid.GetCoordinateAt(x.First()), 
                                                               observedGrid.GetCoordinateAt(x.Last()));

            Vector2D[] normals = sides.Select(GetNormalFromSide).ToArray();

            var worldEastIndex = 0;
            var referenceEast = Vector2D.Create(1.0, 0.0);

            double largestDotProduct = -1.0;

            for (var i = 0; i < 4; i++)
            {
                double dotProduct = normals[i].Dot(referenceEast);

                if (!(largestDotProduct < dotProduct))
                {
                    continue;
                }

                largestDotProduct = dotProduct;
                worldEastIndex = i;
            }

            return worldEastIndex;
        }

        private bool IsDryPoint(GridCoordinate coordinate)
        {
            Coordinate worldCoordinate = observedGrid.GetCoordinateAt(coordinate.X, coordinate.Y);
            return WaveDomainHelper.IsDryPoint(worldCoordinate.X, worldCoordinate.Y);
        }
    }
}