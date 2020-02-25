using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Coverages;
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
            var locationsToSet = discretization.Locations.Values.ToList();

            locationsToRemove.ForEach(l => locationsToSet.Remove(l));

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

        public static IEnumerable<INetworkLocation> GetDuplicateLocations(this IDiscretization discretization)
        {
            var hydroNetwork = discretization.GetHydroNetwork();
            if (hydroNetwork == null) return new List<INetworkLocation>();

            var duplicateLocations = new List<INetworkLocation>();
            foreach (var node in hydroNetwork.HydroNodes)
            {
                if (node.IsOnSingleBranch)
                {
                    continue;
                }

                var branches = node.IncomingBranches.Select(b => new { branch = b, incoming = true })
                    .Concat(node.OutgoingBranches.Select(b => new { branch = b, incoming = false }));

                var nodeLocations = branches.Select(b =>
                {
                    var locations = discretization.GetLocationsForBranch(b.branch);
                    if (locations.Count == 0)
                    {
                        return null;
                    }

                    var index = b.incoming ? locations.Count - 1 : 0;
                    return locations[index];
                }).Where(l => l != null);

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