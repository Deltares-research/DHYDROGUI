using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using GeoAPI.Geometries;
using NetTopologySuite.Mathematics;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="WaveBoundaryGeometricDefinitionFactoryHelper"/> provides the set of methods
    /// to obtain the correct wave boundary geometric definition.
    /// </summary>
    public static class WaveBoundaryGeometricDefinitionFactoryHelper
    {
        /// <summary>
        /// Gets the snapped end points for the specified <paramref name="coordinates"/>.
        /// </summary>
        /// <param name="boundarySnappingCalculator"> The boundary snapping calculator. </param>
        /// <param name="coordinates"> The coordinates. </param>
        /// <returns> A collection of snapped end point. </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the number of coordinates is smaller than two.
        /// </exception>
        public static IEnumerable<GridBoundaryCoordinate> GetSnappedEndPoints(
            IBoundarySnappingCalculator boundarySnappingCalculator,
            IEnumerable<Coordinate> coordinates)
        {
            List<Coordinate> distinctCoordinates = coordinates.Distinct(new Coordinate2DEqualityComparer()).ToList();

            if (distinctCoordinates.Count < 2)
            {
                throw new ArgumentException("There should be two or more distinct coordinates in coordinates.");
            }

            Coordinate firstCoordinate = distinctCoordinates.First();
            Coordinate lastCoordinate = distinctCoordinates.Last();

            IEnumerable<GridBoundaryCoordinate> firstSnappedCoordinates =
                boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(firstCoordinate);
            IEnumerable<GridBoundaryCoordinate> lastSnappedCoordinates =
                boundarySnappingCalculator.SnapCoordinateToGridBoundaryCoordinate(lastCoordinate);

            return firstSnappedCoordinates.Concat(lastSnappedCoordinates);
        }

        /// <summary>
        /// Gets the geometric definition with the specified <paramref name="snappedCoordinates"/>.
        /// </summary>
        /// <param name="snappedCoordinates"> The snapped coordinates. </param>
        /// <param name="calculator"> The boundary snapping calculator. </param>
        /// <returns> The geometric definition. </returns>
        public static IWaveBoundaryGeometricDefinition GetGeometricDefinition(
            IEnumerable<GridBoundaryCoordinate> snappedCoordinates, IBoundarySnappingCalculator calculator)
        {
            IEnumerable<IGrouping<GridSide, GridBoundaryCoordinate>> groupedCoordinates =
                snappedCoordinates.GroupBy(x => x.GridSide)
                                  .Where(group => group.Count() >= 2);

            var candidateFound = false;

            var startIndexCandidate = 0;
            var endIndexCandidate = 0;
            var gridSideCandidate = GridSide.North;
            double lengthCandidate = 0;

            foreach (IGrouping<GridSide, GridBoundaryCoordinate> coordinateGroup in groupedCoordinates)
            {
                int first = coordinateGroup.Min(x => x.Index);
                int last = coordinateGroup.Max(x => x.Index);

                if (first == last ||
                    candidateFound && last - first < endIndexCandidate - startIndexCandidate)
                {
                    continue;
                }

                candidateFound = true;

                startIndexCandidate = first;
                endIndexCandidate = last;
                gridSideCandidate = coordinateGroup.Key;
                lengthCandidate = calculator.CalculateDistanceBetweenBoundaryIndices(first, last, coordinateGroup.Key);
            }

            return candidateFound
                       ? new WaveBoundaryGeometricDefinition(startIndexCandidate,
                                                             endIndexCandidate,
                                                             gridSideCandidate,
                                                             lengthCandidate)
                       : null;
        }

        /// <summary>
        /// Gets the geometric definition of a boundary located at the specified <paramref name="orientation"/>.
        /// </summary>
        /// <param name="orientation"> The world side of the grid at which the boundary should be located. </param>
        /// <param name="calculator"> The boundary snapping calculator. </param>
        /// <returns> The geometric definition. </returns>
        public static IWaveBoundaryGeometricDefinition GetGeometricDefinition(BoundaryOrientationType orientation,
                                                                              IBoundarySnappingCalculator calculator)
        {
            IGridBoundary gridBoundary = calculator.GridBoundary;

            GridSide side = gridBoundary.GetSideAlignedWithNormal(orientation.ToReferenceNormal());
            int startIndex = gridBoundary[side].First().Index;
            int endIndex = gridBoundary[side].Last().Index;

            double length = calculator.CalculateDistanceBetweenBoundaryIndices(startIndex, endIndex, side);
            return new WaveBoundaryGeometricDefinition(startIndex, endIndex, side, length);
        }

        private static Vector2D ToReferenceNormal(this BoundaryOrientationType orientationType)
        {
            switch (orientationType)
            {
                case BoundaryOrientationType.East:
                    return Vector2D.Create(1.0, 0.0);
                case BoundaryOrientationType.NorthEast:
                    return Vector2D.Create(1.0, 1.0);
                case BoundaryOrientationType.North:
                    return Vector2D.Create(0.0, 1.0);
                case BoundaryOrientationType.NorthWest:
                    return Vector2D.Create(-1.0, 1.0);
                case BoundaryOrientationType.West:
                    return Vector2D.Create(-1.0, 0.0);
                case BoundaryOrientationType.SouthWest:
                    return Vector2D.Create(-1.0, -1.0);
                case BoundaryOrientationType.South:
                    return Vector2D.Create(0.0, -1.0);
                case BoundaryOrientationType.SouthEast:
                    return Vector2D.Create(1.0, -1.0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientationType), orientationType, null);
            }
        }
    }
}