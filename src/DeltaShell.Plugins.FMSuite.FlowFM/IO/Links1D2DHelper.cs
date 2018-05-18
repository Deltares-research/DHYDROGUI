using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class Links1D2DHelper
    {
        private const double SNAP_DISTANCE = 9.0; //
        public const int MISSING_INDEX = -1; //

        public static void SetGeometry1D2DLinks(IEnumerable<WaterFlowFM1D2DLink> listOfLinks, DelftTools.Functions.Generic.IVariable<INetworkLocation> networkLocations, IList<Cell> gridCells)
        {
            if (!networkLocations.Values.Any() || !gridCells.Any()) return;

            foreach (var link in listOfLinks)
            {
                var fromNode = networkLocations.Values[link.DiscretisationPointIndex];
                var toCell = gridCells[link.FaceIndex];
                link.Geometry = new LineString(new[] { fromNode.Geometry.Coordinate, toCell.Center });
            }
        }

        public static void SetIndexes1D2DLinks(IEnumerable<WaterFlowFM1D2DLink> listOfLinks, IDiscretization networkDiscretization, UnstructuredGrid grid)
        {
            if (networkDiscretization == null || !networkDiscretization.Locations.Values.Any() || grid == null || !grid.Cells.Any()) return;

            foreach (var link in listOfLinks)
            {
                var line = link.Geometry as ILineString;
                if (line == null) continue;

                link.DiscretisationPointIndex = FindCalculationPointIndex(line.StartPoint, networkDiscretization);
                link.FaceIndex = FindCellIndex(line.EndPoint, grid); 
            }
        }

        private static int FindCalculationPointIndex(IPoint startPointLink, IDiscretization networkDiscretization)
        {
            var location = networkDiscretization.Locations.Values.FirstOrDefault(
                networkLocation => IsPointEqual(networkLocation, startPointLink));

            return location == null ? MISSING_INDEX : networkDiscretization.Locations.Values.IndexOf(location);
        }


        private static bool IsPointEqual(IFeature l, IGeometry pointLink)
        {
            return l.Geometry.EqualsExact(pointLink);
        }

        private static bool IsPointInSnapTolerance(INetworkLocation l, IPoint pointLink)
        { 
            return l.Geometry.EqualsExact(pointLink,SNAP_DISTANCE);
        }

        private static int FindCellIndex(IPoint point, UnstructuredGrid grid)
        {
            return grid.GetCellIndexForCoordinate(point.Coordinate) ?? MISSING_INDEX;
        }
    }
}
