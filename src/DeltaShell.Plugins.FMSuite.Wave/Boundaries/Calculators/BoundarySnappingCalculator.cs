using System;
using System.Collections.Generic;
using System.Linq;
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
        public BoundarySnappingCalculator(GridBoundary gridBoundary)
        {
            GridBoundary = gridBoundary ?? throw new ArgumentNullException(nameof(gridBoundary));
            DistanceCalculator = new CartesianDistanceCalculator();
        }

        public GridBoundary GridBoundary
        {
            get => gridBoundary;
            set => gridBoundary = value ?? throw new ArgumentNullException(nameof(value));
        }

        private GridBoundary gridBoundary;

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
            List<Coordinate> gridEnvelopeWorldCoordinates = gridEnvelope.Select(GridBoundary.GetWorldCoordinateFromBoundaryCoordinate)
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
    }
}