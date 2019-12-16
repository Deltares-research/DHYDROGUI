using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
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
            IEnumerable<Coordinate> distinctCoordinates = coordinates.Distinct(new Coordinate2DEqualityComparer());

            if (distinctCoordinates.Count() < 2)
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


        public IWaveBoundaryGeometricDefinition GetGeometricDefinition(IEnumerable<GridBoundaryCoordinate> snappedCoordinates)
        {
            IEnumerable<IGrouping<GridSide, GridBoundaryCoordinate>> groupedCoordinates =
                snappedCoordinates.GroupBy(x => x.GridSide)
                                  .Where(group => group.Count() >= 2);

            IWaveBoundaryGeometricDefinition candidate = null;

            foreach (IGrouping<GridSide, GridBoundaryCoordinate> coordinateGroup in groupedCoordinates)
            {
                int first = coordinateGroup.Min(x => x.Index);
                int last = coordinateGroup.Max(x => x.Index);

                if (first == last ||
                    (candidate != null &&
                     last - first < candidate.EndingIndex - candidate.StartingIndex))
                {
                    continue;
                }

                candidate = new WaveBoundaryGeometricDefinition(first,
                                                                last,
                                                                coordinateGroup.Key);
            }

            return candidate;
        }
    }
}