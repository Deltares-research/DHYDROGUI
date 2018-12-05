using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
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

            var handledLinks = new List<ILink1D2D>();
            foreach (var link in listOfLinks)
            {
                var line = link.Geometry as ILineString;
                if (line == null) continue;

                link.DiscretisationPointIndex = FindCalculationPointIndex(line.EndPoint, networkDiscretization, handledLinks, tolerance);
                link.FaceIndex = FindCellIndex(line.StartPoint, grid);
                handledLinks.Add(link);
            }
        }

        private static int FindCalculationPointIndex(IPoint startPointLink, IDiscretization networkDiscretization, IList<ILink1D2D> handledLinks, double tolerance = 0.0, IList<bool> locationsMask = null)
        {
            var locationIndex = MISSING_INDEX;
            if (locationsMask == null)
            {
                var locations = networkDiscretization.Locations.Values.Where(
                    networkLocation => IsPointEqual(networkLocation, startPointLink, tolerance)).ToArray();

                if (locations.Length > 1)
                {
                    var discretisationPointIndices = locations.Select(loc => networkDiscretization.Locations.Values.IndexOf(loc));
                    foreach (var index in discretisationPointIndices)
                    {
                        if (!handledLinks.Select(link => link.DiscretisationPointIndex).Contains(index))
                        {
                            return index;
                        }
                    }
                }
                
                locationIndex = networkDiscretization.Locations.Values.IndexOf(locations[0]);
            }
            else
            {
                var locations = networkDiscretization.Locations.Values.ToList();
                for (int i = 0; i < locations.Count; i++)
                {
                    if (locationsMask[i])
                    {
                        if (IsPointEqual(locations[i], startPointLink, tolerance))
                        {
                            locationIndex = i;
                            break;
                        }
                    }
                }
            }
            return locationIndex;
        }

        public static int FindCalculationPointIndex(IPoint startPointLink, IDiscretization networkDiscretization, double tolerance = 0.0, IList<bool> locationsMask = null)
        {
            var locationIndex = MISSING_INDEX;
            if (locationsMask == null)
            {
                var location = networkDiscretization.Locations.Values.FirstOrDefault(
                    networkLocation => IsPointEqual(networkLocation, startPointLink, tolerance));
                locationIndex = networkDiscretization.Locations.Values.IndexOf(location);
            }
            else
            {
                var locations = networkDiscretization.Locations.Values.ToList();
                for (int i = 0; i < locations.Count; i++)
                {
                    if (locationsMask[i])
                    {
                        if (IsPointEqual(locations[i], startPointLink, tolerance))
                        {
                            locationIndex = i;
                            break;
                        }
                    }
                }
            }
            return locationIndex;
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
