using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Link1d2d;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class Links1D2DHelper
    {
        public const int MISSING_INDEX = -1;

        public static void SetGeometry1D2DLinks(IEnumerable<ILink1D2D> listOfLinks, DelftTools.Functions.Generic.IVariable<INetworkLocation> networkLocations, IList<Cell> gridCells)
        {
            if (!networkLocations.Values.Any() || !gridCells.Any()) return;

            foreach (var link in listOfLinks)
            {
                var fromNode = networkLocations.Values[link.DiscretisationPointIndex];
                var toCell = gridCells[link.FaceIndex];
                link.Geometry = new LineString(new[] { fromNode.Geometry.Coordinate, toCell.Center });
            }
        }

        public static void SetIndexes1D2DLinks(IEnumerable<ILink1D2D> listOfLinks, IDiscretization networkDiscretization, UnstructuredGrid grid, double tolerance = 0.0)
        {
            if (networkDiscretization == null || !networkDiscretization.Locations.Values.Any() || grid == null || !grid.Cells.Any()) return;
            
            var link1D2Ds = listOfLinks.ToList();
            foreach (var link in link1D2Ds)
            {
                var line = link.Geometry as ILineString;
                if (line == null) continue;

                link.DiscretisationPointIndex = FindCalculationPointIndex(line.StartPoint, networkDiscretization, tolerance);
                link.FaceIndex = FindCellIndex(line.EndPoint, grid);
                link.Link1D2DIndex = link1D2Ds.IndexOf(link);
            }
        }

        public static int FindCalculationPointIndex(IPoint startPointLink, IDiscretization networkDiscretization, double tolerance = 0.0, IList<bool> locationsMask = null)
        {
            bool hasMask = locationsMask != null;
            var locations = networkDiscretization.Locations.Values.ToArray();

            for (int i = 0; i < locations.Length; i++)
            {
                if (hasMask && !locationsMask[i])
                {
                    continue;
                }

                if (IsPointEqual(locations[i], startPointLink, tolerance))
                {
                    return i;
                }
            }

            return MISSING_INDEX;
        }

        public static int FindCalculationPointIndex(Coordinate coordinate, IDiscretization networkDiscretization, double tolerance)
        {
            return FindCalculationPointIndex(new Point(coordinate.X, coordinate.Y), networkDiscretization, tolerance);
        }

        private static bool IsPointEqual(IFeature l, IGeometry pointLink, double tolerance = 0.0)
        {
            return l.Geometry.EqualsExact(pointLink, tolerance);
        }

        public static int FindCellIndex(IPoint point, UnstructuredGrid grid)
        {
            return grid.GetCellIndexForCoordinate(point.Coordinate) ?? MISSING_INDEX;
        }

        public static int FindCellIndex(Coordinate coordinate, UnstructuredGrid grid)
        {
            return FindCellIndex(new Point(coordinate.X, coordinate.Y), grid);
        }
    }
}
