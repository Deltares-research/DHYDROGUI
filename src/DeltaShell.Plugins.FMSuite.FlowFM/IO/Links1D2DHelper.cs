using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Link1d2d;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
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
            var link1D2Ds = listOfLinks.ToList();
            foreach (var link in link1D2Ds)
            {
                var line = link.Geometry as ILineString;
                if (line == null) continue;

                link.DiscretisationPointIndex = FindCalculationPointIndex(line.StartPoint, networkDiscretization, handledLinks, tolerance);
                link.FaceIndex = FindCellIndex(line.EndPoint, grid);
                link.Link1D2DIndex = link1D2Ds.IndexOf(link);
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
                
                locationIndex = locations.Length > 0 && networkDiscretization.Locations.Values.Contains(locations[0]) &&
                                //no links at end points of our network, then use missing_index so link will be deleted...tjiske and arthur van dam knows more about this
                                !LocationIsOnEndPointOfNetwork(networkDiscretization, locations[0])
                                    ? networkDiscretization.Locations.Values.IndexOf(locations[0]) : MISSING_INDEX;
            }
            else
            {
                var locations = networkDiscretization.Locations.Values.ToList();
                for (int i = 0; i < locations.Count; i++)
                {
                    if (locationsMask[i] && 
                        IsPointEqual(locations[i], startPointLink, tolerance))
                    {
                        locationIndex = i;
                        break;
                    }
                }
            }
            return locationIndex;
        }

        private static bool LocationIsOnEndPointOfNetwork(IDiscretization networkDiscretization, INetworkLocation location)
        {
            var network = networkDiscretization.Network;
            if (network == null) return true;
            var branch = NetworkHelper.GetNearestBranch(network.Branches, location.Geometry, 0.0001);
            if (branch == null)
            {
                return false;
            }
            // if calculation point is on start or end of the branch and source or target node is connected to another branch then this location is not on an end point of a network
            if (Math.Abs(location.Chainage) < 0.001 && branch.Source.IsConnectedToMultipleBranches ||
                Math.Abs(branch.Length - location.Chainage) < 0.001 && branch.Target.IsConnectedToMultipleBranches)
                return false;
            // if calculation point is on middle of the branch then never on end of network
            if (Math.Abs(location.Chainage) > 0.001 && 
                Math.Abs(branch.Length - location.Chainage) > 0.001) 
                return false;
            return true;
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
                    if (locationsMask[i] && IsPointEqual(locations[i], startPointLink, tolerance))
                    {
                        locationIndex = i;
                        break;
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
