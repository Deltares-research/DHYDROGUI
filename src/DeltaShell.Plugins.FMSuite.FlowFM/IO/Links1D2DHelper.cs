using System.Collections.Generic;
using System.Linq;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class Links1D2DHelper
    {
        private const double SNAP_DISTANCE = 9.0; //
        public const int MISSING_INDEX = -1; //

        public static void SetGeometry1D2DLinks(IList<WaterFlowFM1D2DLink> listOfLinks, IDiscretization networkDiscretization, UnstructuredGrid grid)
        {
            if (networkDiscretization != null && networkDiscretization.Locations.Values.Any() && grid != null && grid.Cells.Any())
            {
                foreach (var link in listOfLinks)
                {
                    var fromNode = networkDiscretization.Locations.Values[link.DiscretisationPointIndex];
                    var toCell = grid.Cells[link.FaceIndex];
                    link.Geometry = new LineString(new[] { fromNode.Geometry.Coordinate, toCell.Center });
                }
            }
        }

        public static void SetIndexes1D2DLinks(IList<WaterFlowFM1D2DLink> listOfLinks, IDiscretization networkDiscretization, UnstructuredGrid grid)
        {
            if (networkDiscretization != null && networkDiscretization.Locations.Values.Any() && grid != null && grid.Cells.Any())
            {
                foreach (var link in listOfLinks)
                {
                    var line = link.Geometry as ILineString;
                    if (line != null)
                    {
                        var descritisationPoint = line.StartPoint;
                        var descritisationIndex = FindCalculationPointIndex(descritisationPoint, networkDiscretization);
                        link.DiscretisationPointIndex = descritisationIndex ?? MISSING_INDEX;

                        var cellPoint = line.EndPoint;
                        var cellIndex = FindCellIndex(cellPoint, grid);
                        link.FaceIndex = cellIndex ?? MISSING_INDEX;
                    }
                }
            }
        }

        private static int? FindCalculationPointIndex(IPoint startPointLink, IDiscretization networkDiscretization)
        {
            var location = networkDiscretization.Locations.Values.FirstOrDefault(
                networkLocation => IsPointEqual(networkLocation, startPointLink));

            if (location == null) return null;

            return networkDiscretization.Locations.Values.IndexOf(location);
        }


        private static bool IsPointEqual(INetworkLocation l, IPoint pointLink)
        {
            return l.Geometry.EqualsExact(pointLink);
        }

        private static bool IsPointInSnapTolerance(INetworkLocation l, IPoint pointLink)
        { 
            return l.Geometry.EqualsExact(pointLink,SNAP_DISTANCE);
        }

        private static int? FindCellIndex(IPoint point, UnstructuredGrid grid)
        {
            return grid.GetCellIndexForCoordinate(point.Coordinate);
        }
    }
}
