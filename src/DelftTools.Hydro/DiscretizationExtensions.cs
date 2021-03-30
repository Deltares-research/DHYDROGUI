using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
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

        /// <summary>
        /// Adds the given <paramref name="locationToAdd"/> to the <paramref name="discretization"/> if they are not already present (based on geometry)
        /// </summary>
        /// <param name="discretization">Discretization to add the locations to</param>
        /// <param name="locationToAdd">Locations that need to be added</param>
        public static void AddNetworkLocationsIfNotAlreadyCreated(this IDiscretization discretization, IEnumerable<NetworkLocation> locationToAdd)
        {
            var locations = new HashSet<Coordinate>(discretization.Locations.Values.Select(l => l.Geometry?.Coordinate));
            
            foreach (var location in locationToAdd)
            {
                var coordinate = location.Geometry?.Coordinate;
                if (coordinate == null)
                {
                    Log.Warn($"No geometry set for {location.Name}");
                    continue;
                }

                if (!locations.Contains(coordinate))
                {
                    discretization.Locations.AddValues(new[] { location });
                    locations.Add(coordinate);
                }
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

        public static void UpdateNetworkLocations(this IDiscretization networkDiscretization, IEnumerable<INetworkLocation> newLocations, bool merge = true)
        {
            // remember network locations the user has fixed.
            var currentLocations = networkDiscretization.Locations.Values.ToArray();

            var fixedOffsetNetworkLocations = new HashSet<INetworkLocation>(currentLocations
                                              .Where(networkDiscretization.IsFixedPoint));

            // Merge existing locations and remove locations with the same geometry
            var networkLocations = newLocations as INetworkLocation[] ?? newLocations.ToArray();
            

            networkDiscretization.BeginEdit(new DefaultEditAction("Setting values"));
            networkDiscretization.Clear();
            if (merge)
            {
                var locationsMerged = networkLocations
                                      .Union(currentLocations)
                                      .GroupBy(lv => lv.Geometry?.Coordinate)
                                      .Select(crdGroup =>
                                                  crdGroup.Select(nl => fixedOffsetNetworkLocations.Contains(nl) ? nl : null).FirstOrDefault() ??
                                                  crdGroup.Min())
                                      .OrderBy(l => l)
                                      .ToArray();
                FunctionHelper.SetValuesRaw<INetworkLocation>(networkDiscretization.Locations, locationsMerged);
                FunctionHelper.SetValuesRaw(networkDiscretization.Components[0], Enumerable.Repeat(0d, locationsMerged.Length));
            }
            else
            {
                var locationsMerged = networkLocations
                                      .GroupBy(lv => lv.Geometry?.Coordinate)
                                      .Select(crdGroup =>
                                                  crdGroup.Select(nl => fixedOffsetNetworkLocations.Contains(nl) ? nl : null).FirstOrDefault() ??
                                                  crdGroup.Min())
                                      .OrderBy(l => l)
                                      .ToArray();
                FunctionHelper.SetValuesRaw<INetworkLocation>(networkDiscretization.Locations, locationsMerged);
                FunctionHelper.SetValuesRaw(networkDiscretization.Components[0], Enumerable.Repeat(0d, locationsMerged.Length));
            }

            fixedOffsetNetworkLocations.ForEach(networkDiscretization.ToggleFixedPoint);
            networkDiscretization.EndEdit();

            // force refresh of caching (location dictionary) -> new locations are added
            TypeUtils.SetField(networkDiscretization, "updateLocationsDictionary", true);
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