using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public static class DiscretizationExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DiscretizationExtensions));

        public static void AddNetworkDiscretizationCalculationLocationIfNotAlreadyCreated(this IDiscretization discretization, NetworkLocation toLocation)
        {
            var locations = new HashSet<Coordinate>(discretization.Locations.Values.Select(l => l.Geometry?.Coordinate));
            var locationGeometry = toLocation.Geometry;
            if (locationGeometry == null) Log.Warn($"No geometry set for {toLocation.Name}");
            if (!locations.Contains(locationGeometry?.Coordinate))
            {
                discretization.Locations.AddValues(new[] { toLocation });
            }
        }

        public static void AddLocations(this IDiscretization discretization, IEnumerable<INetworkLocation> locationsToAdd)
        {
            var autoSortedValue = discretization.Locations.IsAutoSorted;
            discretization.Locations.IsAutoSorted = false;
            discretization.SetLocations(discretization.Locations.Values.Concat(locationsToAdd).ToList());
            discretization.Locations.IsAutoSorted = autoSortedValue;
        }

        public static void RemoveLocations(this IDiscretization discretization, IEnumerable<INetworkLocation> locationsToRemove) 
        {
            var locationsToSet = discretization.Locations.Values.Except(locationsToRemove).ToList();

            discretization.Locations.Clear();
            discretization.SetLocations(locationsToSet);
        }

        public static IEnumerable<INetworkLocation> GenerateSewerConnectionNetworkLocations(this IDiscretization discretization)
        {
            var hydroNetwork = discretization.GetHydroNetwork();
            if (hydroNetwork == null) yield break;

            foreach (var sewerConnection in hydroNetwork.SewerConnections)
            {
                yield return new NetworkLocation(sewerConnection, 0.0);

                if (sewerConnection?.Length > 0)
                {
                    yield return new NetworkLocation(sewerConnection, sewerConnection.Length);
                }
            }
        }
        
        public static IEnumerable<INetworkLocation> GetDuplicatePointsOnManholes(this IDiscretization discretization)
        {
            var duplicateLocations = new List<INetworkLocation>();
            var hydroNetwork = discretization.GetHydroNetwork();
            if (hydroNetwork == null) return duplicateLocations;

            foreach (var manhole in hydroNetwork.Manholes)
            {
                if (manhole.IsOnSingleBranch)
                    continue;

                var nodeLocations = GetNetworkLocationAtNode(discretization, manhole);
                if (nodeLocations.Count <= 1) 
                    continue;

                if (manhole.Compartments.Count == 1)
                {
                    duplicateLocations.AddRange(nodeLocations.Skip(1));
                }
                else
                {
                    // points per compartment
                    var pointsPerCompartment = nodeLocations.GroupBy(GetCompartmentForLocation);

                    foreach (var compartmentPoints in pointsPerCompartment.Where(p => p.Key != null))
                    {
                        duplicateLocations.AddRange(compartmentPoints.Skip(1));
                    }
                }
            }

            return duplicateLocations;
        }

        private static ICompartment GetCompartmentForLocation(INetworkLocation location)
        {
            if (!(location.Branch is ISewerConnection sewerConnection))
                return null;

            if (Math.Abs(location.Chainage) < 1e-8)
            {
                return sewerConnection.SourceCompartment;
            }

            if (Math.Abs(sewerConnection.Length - location.Chainage) < 1e-8)
            {
                return sewerConnection.TargetCompartment;
            }

            return null;
        }

        private static List<INetworkLocation> GetNetworkLocationAtNode(this IDiscretization discretization, INode node)
        {
            var branches = node.IncomingBranches
                .Select(b => new {branch = b, incoming = true})
                .Concat(node.OutgoingBranches.Select(b => new {branch = b, incoming = false}));

            return branches
                .Select(b =>
                {
                    var locations = discretization.GetLocationsForBranch(b.branch);
                    if (locations.Count == 0)
                    {
                        return null;
                    }

                    // check if first or last is at begin or end node location
                    var index = b.incoming ? locations.Count - 1 : 0;
                    var atBeginOrEnd = b.incoming
                        ? Math.Abs(locations[index].Chainage - b.branch.Length) < 1e-8
                        : Math.Abs(locations[index].Chainage) < 1e-8;

                    return atBeginOrEnd
                        ? locations[index]
                        : null;
                })
                .Where(l => l != null)
                .ToList();
        }

        public static IEnumerable<INetworkLocation> GetDuplicatePointsOnHydroNodes(this IDiscretization discretization)
        {
            var duplicateLocations = new List<INetworkLocation>();
            var hydroNetwork = discretization.GetHydroNetwork();
            if (hydroNetwork == null) return duplicateLocations;
            
            foreach (var node in hydroNetwork.HydroNodes)
            {
                if (node.IsOnSingleBranch)
                    continue;

                var nodeLocations = discretization.GetNetworkLocationAtNode(node);
                if (nodeLocations.Count <= 1) 
                    continue;

                duplicateLocations.AddRange(nodeLocations.Skip(1));
            }

            return duplicateLocations;
        }

        private static IHydroNetwork GetHydroNetwork(this IDiscretization discretization)
        {
            var hydroNetwork = discretization.Network as IHydroNetwork;
            if (hydroNetwork == null)
            {
                Log.Error("Could not find network to generate grid.");
            }

            return hydroNetwork;
        }
    }
}