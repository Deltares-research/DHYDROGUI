using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers
{
    /// <summary>
    /// <see cref="WaveBoundaryFactoryHelper"/> provides the set of methods used
    /// by the <see cref="IWaveBoundaryFactory"/> to obtain the correct wave boundary
    /// data from view data.
    /// </summary>
    /// <seealso cref="IWaveBoundaryFactoryHelper" />
    public class WaveBoundaryFactoryHelper : IWaveBoundaryFactoryHelper
    {
        public IEnumerable<GridBoundaryCoordinate> GetSnappedEndPoints(IBoundarySnappingCalculator boundarySnappingCalculator, 
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

        public IWaveBoundaryGeometricDefinition GetGeometricDefinition(IEnumerable<GridBoundaryCoordinate> snappedCoordinates, IBoundarySnappingCalculator calculator)
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
                    (candidateFound && last - first < endIndexCandidate - startIndexCandidate))
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
                       ? new WaveBoundaryGeometricDefinition(startIndexCandidate, endIndexCandidate, gridSideCandidate, lengthCandidate)
                       : null;
        }

        public IWaveBoundaryConditionDefinition GetConditionDefinition()
        {
            // TODO (MWT): see if this warrants a separate class
            var shape = new JonswapShape {PeakEnhancementFactor = 3.3};
            const BoundaryConditionPeriodType periodType = 
                BoundaryConditionPeriodType.Peak;
            const BoundaryConditionDirectionalSpreadingType directionalSpreading =
                BoundaryConditionDirectionalSpreadingType.Power;
            var dataComponent = new UniformDataComponent(
                new ConstantParameters(0.0, 
                                       1.0, 
                                       0.0, 
                                       4.0));

            return new WaveBoundaryConditionDefinition(shape, 
                                                       periodType, 
                                                       directionalSpreading,
                                                       dataComponent);
        }
    }
}