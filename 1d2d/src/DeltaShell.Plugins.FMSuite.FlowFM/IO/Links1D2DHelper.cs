using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Link1d2d;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class Links1D2DHelper
    {
        public const int MISSING_INDEX = -1;
        private static readonly ILog Log = LogManager.GetLogger(typeof(Links1D2DHelper));

       public static void SetGeometry1D2DLinks(IEnumerable<ILink1D2D> listOfLinks, DelftTools.Functions.Generic.IVariable<INetworkLocation> networkLocations, IList<Cell> gridCells)
        {
            var networkLocationsValues = networkLocations.Values.ToArray();
            if (!networkLocationsValues.Any() || !gridCells.Any()) return;
            foreach (var link in listOfLinks)
            {
                if (link.DiscretisationPointIndex > networkLocationsValues.Length - 1)
                {
                    Log.Error(Resources.Links1D2DHelper_SetGeometry1D2DLinks__1d2d_link_discretization_point_problem_ + Resources.Links1D2DHelper_SetGeometry1D2DLinks__Cannot_create_geometry_of_1d2d_link__link_Name__from_network_discretization_point__link_DiscretisationPointIndex__to_grid_cell__link_FaceIndex_);
                    continue;
                }
                var fromNode = networkLocationsValues[link.DiscretisationPointIndex];
                if (link.FaceIndex > gridCells.Count -1)
                {
                    Log.Error(Resources.Links1D2DHelper_SetGeometry1D2DLinks__1d2d_link_grid_cell_problem_ + Resources.Links1D2DHelper_SetGeometry1D2DLinks__Cannot_create_geometry_of_1d2d_link__link_Name__from_network_discretization_point__link_DiscretisationPointIndex__to_grid_cell__link_FaceIndex_);
                    continue;
                }
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
