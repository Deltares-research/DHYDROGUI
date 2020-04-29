using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="WaveBoundaryGeometricDefinitionFactoryHelper" /> provides the set of methods
    /// to obtain the correct wave boundary geometric definition.
    /// </summary>
    public static class WaveBoundaryGeometricDefinitionFactoryHelper
    {
        /// <summary>
        /// Gets the snapped end points for the specified <paramref name="coordinates" />.
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
        /// Gets the geometric definition with the specified <paramref name="snappedCoordinates" />.
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

            // TODO: improve this method. Discuss with Maarten.
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
    }
}