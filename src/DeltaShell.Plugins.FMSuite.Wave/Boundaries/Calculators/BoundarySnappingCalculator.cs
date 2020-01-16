using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="BoundarySnappingCalculator"/> provides a set of equations
    /// to snap coordinates to a <see cref="GridBoundary"/>. 
    /// </summary>
    public class BoundarySnappingCalculator : IBoundarySnappingCalculator
    {
        /// <summary>
        /// Creates a new <see cref="BoundarySnappingCalculator"/>.
        /// </summary>
        /// <param name="gridBoundary">The grid boundary.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gridBoundary"/> is <c>null</c>.
        /// </exception>
        public BoundarySnappingCalculator(IGridBoundary gridBoundary)
        {
            GridBoundary = gridBoundary ?? throw new ArgumentNullException(nameof(gridBoundary));
            DistanceCalculator = new CartesianDistanceCalculator();
        }

        public IGridBoundary GridBoundary { get; }

        public IDistanceCalculator DistanceCalculator { get; }

        // TODO Verify whether this could make use of the squared value.
        public IEnumerable<GridBoundaryCoordinate> SnapCoordinateToGridBoundaryCoordinate(Coordinate coordinateToSnap,
                                                                                          double? tolerance = null)
        {
            if (coordinateToSnap == null)
            {
                throw new ArgumentNullException(nameof(coordinateToSnap));
            }

            if (!coordinateToSnap.IsDefined())
            {
                return Enumerable.Empty<GridBoundaryCoordinate>();
            }

            List<GridBoundaryCoordinate> gridEnvelope = GridBoundary.GetGridEnvelope().ToList();
            List<Coordinate> gridEnvelopeWorldCoordinates =
                gridEnvelope.Select(GridBoundary.GetWorldCoordinateFromBoundaryCoordinate)
                            .ToList();
            Tuple<IEnumerable<int>, double> closestIndices =
                BoundarySnappingCalculatorHelper.FindClosestIndices(DistanceCalculator,
                                                                    coordinateToSnap,
                                                                    gridEnvelopeWorldCoordinates);

            if (tolerance != null && closestIndices.Item2 > tolerance)
            {
                return Enumerable.Empty<GridBoundaryCoordinate>();
            }

            return closestIndices.Item1.Select(i => gridEnvelope[i]);
        }

        public double CalculateDistanceBetweenBoundaryIndices(int indexA, int indexB, GridSide gridSide)
        {
            Ensure.IsDefined(gridSide, nameof(gridSide));

            ValidateCoordinate(indexA, nameof(indexA), gridSide);
            ValidateCoordinate(indexB, nameof(indexB), gridSide);

            int startIndex = Math.Min(indexA, indexB);
            int endIndex   = Math.Max(indexA, indexB);

            int nCoordinates = endIndex - startIndex + 1;
            Coordinate[] coordinates =
                GridBoundary[gridSide].Skip(startIndex)
                                      .Take(nCoordinates)
                                      .Select(GridBoundary.GetWorldCoordinateFromBoundaryCoordinate)
                                      .ToArray();

            double distance = 0;
            for (var i = 0; i < nCoordinates - 1; i++)
            {
                distance += DistanceCalculator.CalculateDistance(coordinates[i], coordinates[i + 1]);
            }

            return distance;
        }

        private void ValidateCoordinate(int index, string indexName, GridSide gridSide)
        {
            int sideLength = GridBoundary[gridSide].Count();

            if (index < 0 || index >= sideLength)
            {
                throw new ArgumentOutOfRangeException(indexName);
            }
        }

        public Coordinate CalculateCoordinateFromSupportPoint(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));

            IWaveBoundaryGeometricDefinition geometricDefinition = supportPoint.GeometricDefinition;

            int nElements = geometricDefinition.EndingIndex - geometricDefinition.StartingIndex + 1;

            Coordinate[] coordinates = GridBoundary[geometricDefinition.GridSide]
                                       .Skip(geometricDefinition.StartingIndex)
                                       .Take(nElements)
                                       .Select(x => GridBoundary.GetWorldCoordinateFromBoundaryCoordinate(x))
                                       .ToArray();

            return BoundarySnappingCalculatorHelper.CalculateCoordinateFromDistance(supportPoint.Distance, coordinates, DistanceCalculator);
        }
    }
}