﻿using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DelftTools.Hydro
{
    public enum BranchNodeType
    {
        Begin,
        End
    }

    public static class DiscretizationExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DiscretizationExtensions));
        private const double chainageDelta = 1e-10;

        public static void AddMissingLocationsForSewerConnections(this IDiscretization discretization, params ISewerConnection[] sewerConnections)
        {
            var locationsToAdd = new List<INetworkLocation>();
            foreach (var sewerConnection in sewerConnections)
            {
                var sourcePoints = sewerConnection.SourceCompartment == null
                                       ? GetNetworkLocationsAtNode(discretization.GetLocationsForBranch, sewerConnection.Source)
                                       : discretization.GetLocationsForCompartment(sewerConnection.SourceCompartment);

                var targetPoints = sewerConnection.TargetCompartment == null
                                       ? GetNetworkLocationsAtNode(discretization.GetLocationsForBranch, sewerConnection.Target)
                                       : discretization.GetLocationsForCompartment(sewerConnection.TargetCompartment);

                bool hasSourcePoints = sourcePoints.Any();
                if (!hasSourcePoints)
                {
                    locationsToAdd.Add(new NetworkLocation(sewerConnection, 0));
                }

                if (!targetPoints.Any() && sewerConnection.Length != 0)
                {
                    locationsToAdd.Add(new NetworkLocation(sewerConnection, sewerConnection.Length));
                }
            }

            discretization.Locations.AddValues(locationsToAdd);
        }

        /// <summary>
        /// Adds missing points for removed branches
        /// (for channels call before remove is done => otherwise computation points of the removed channel are already removed). 
        /// </summary>
        /// <param name="discretization">The discretization to update</param>
        /// <param name="branches">Branches that will be removed</param>
        public static void ReplacePointsForRemovedBranch(this IDiscretization discretization, params IBranch[] branches)
        {
            var locationsToAdd = new List<INetworkLocation>();
            foreach (var branch in branches)
            {
                switch (branch)
                {
                    case IChannel channel:
                        // move node computation points to other branches if available
                        var locationsOnBranch = discretization.GetLocationsForBranch(channel).OrderBy(l => l.Chainage).ToArray();

                        void AddLocationForNode(INode node)
                        {
                            var otherChannel = node.OutgoingBranches.FirstOrDefault(c => c != channel);
                            if (otherChannel != null)
                            {
                                locationsToAdd.Add(new NetworkLocation(otherChannel, 0));
                                return;
                            }

                            otherChannel = node.IncomingBranches.FirstOrDefault(c => c != channel);
                            if (otherChannel != null)
                            {
                                locationsToAdd.Add(new NetworkLocation(otherChannel, otherChannel.Length));
                            }
                        }

                        var location = locationsOnBranch.FirstOrDefault();
                        if (location != null && location.Chainage == 0)
                        {
                            AddLocationForNode(channel.Source);
                        }

                        location = locationsOnBranch.LastOrDefault();
                        if (location != null && IsEndLocation(location))
                        {
                            AddLocationForNode(channel.Target);
                        }

                        break;
                    case ISewerConnection sewerConnection:
                        if (sewerConnection.SourceCompartment != null)
                        {
                            var missingSourceLocation = discretization.GetMissingLocationForCompartment(sewerConnection.SourceCompartment);
                            if (missingSourceLocation != null)
                            {
                                locationsToAdd.Add(missingSourceLocation);
                            }
                        }

                        if (sewerConnection.TargetCompartment != null)
                        {
                            var missingTargetLocation = discretization.GetMissingLocationForCompartment(sewerConnection.TargetCompartment);
                            if (missingTargetLocation != null)
                            {
                                locationsToAdd.Add(missingTargetLocation);
                            }
                        }
                        break;
                }

                discretization.Locations.AddValues(locationsToAdd);
            }
        }

        /// <summary>
        /// Handles switching of compartment for a <see cref="ISewerConnection"/>
        /// </summary>
        /// <param name="discretization">The discretization that needs to be updated</param>
        /// <param name="fromCompartment">Previous compartment</param>
        /// <param name="toCompartment">Current compartment</param>
        public static void HandleCompartmentSwitch(this IDiscretization discretization, ICompartment fromCompartment, ICompartment toCompartment)
        {
            var missingLocation = discretization.GetMissingLocationForCompartment(fromCompartment);
            if (missingLocation != null)
            {
                discretization.Locations.AddValues(new[] { missingLocation });
            }

            missingLocation = discretization.GetMissingLocationForCompartment(toCompartment);
            if (missingLocation != null)
            {
                discretization.Locations.AddValues(new[] { missingLocation });
            }
            else
            {
                var duplicateLocations = discretization.GetLocationsForCompartment(toCompartment).Skip(1).ToArray();

                if (duplicateLocations.Any())
                {
                    discretization.Locations.RemoveValues(new VariableValueFilter<INetworkLocation>(discretization.Locations, duplicateLocations));
                }
            }
        }

        /// <summary>
        /// Update the <paramref name="discretization"/> with the <paramref name="newLocations"/>
        /// </summary>
        /// <param name="discretization">The discretization to update</param>
        /// <param name="newLocations">New locations to add</param>
        /// <param name="merge">True to merge <paramref name="newLocations"/> with the existing discretization points</param>
        public static void UpdateNetworkLocations(this IDiscretization discretization, IEnumerable<INetworkLocation> newLocations, bool merge = true)
        {
            var network = discretization.Network;
            if (network == null) return;

            List<INetworkLocation> locationsToAdd;

            var existingLocations = discretization.Locations.GetValues();
            if (!merge || existingLocations.Count == 0)
            {
                // add only new locations
                locationsToAdd = new List<INetworkLocation>(newLocations);
            }
            else
            {
                locationsToAdd = MergeLocations(discretization, newLocations, existingLocations);
            }

            // cleanup duplicate locations over branches (on nodes)
            CleanupLocationsAtNodes(locationsToAdd, network.Nodes);

            // sort the locations before adding (required for FM to run, see issue UNST-6184)
            locationsToAdd.Sort();

            double[] fixedPointsMask = FixedPointsMask(discretization, locationsToAdd);
            discretization.ResetValues(locationsToAdd, fixedPointsMask);
        }

        /// <summary>
        /// Generates network locations for all sewer connections in <paramref name="discretization"/> network
        /// </summary>
        /// <param name="discretization">Discretization generate points for</param>
        /// <returns>A start and end point for each sewer connection</returns>
        public static IEnumerable<INetworkLocation> GenerateSewerConnectionNetworkLocations(this IDiscretization discretization)
        {
            if (!(discretization.Network is IHydroNetwork hydroNetwork))
            {
                log.Error("Could not find network to generate grid.");
                yield break;
            }

            foreach (var sewerConnection in hydroNetwork.SewerConnections)
            {
                yield return new NetworkLocation(sewerConnection, 0.0);

                if (sewerConnection?.Length > 0)
                {
                    yield return new NetworkLocation(sewerConnection, sewerConnection.Length);
                }
            }
        }

        /// <summary>
        /// Gets locations for the node of a branch 
        /// </summary>
        /// <param name="discretization">The discretization containing the locations</param>
        /// <param name="branch">Branch connected to the node</param>
        /// <param name="nodeType">Node type (start/end node)</param>
        /// <returns>Locations on the begin or end node</returns>
        public static INetworkLocation GetLocationForBranchNode(this IDiscretization discretization, IBranch branch, BranchNodeType nodeType)
        {
            if (branch is ISewerConnection sewerConnection)
            {
                var compartment = nodeType == BranchNodeType.Begin ? sewerConnection.SourceCompartment : sewerConnection.TargetCompartment;
                if (compartment != null) // could be null for combined urban/rural networks
                {
                    return discretization.GetLocationsForCompartment(compartment).FirstOrDefault();
                }
            }

            var node = nodeType == BranchNodeType.Begin ? branch.Source : branch.Target;
            return GetNetworkLocationsAtNode(discretization.GetLocationsForBranch, node).FirstOrDefault();
        }

        /// <summary>
        /// Removes all rural locations, and adds missing the urban nodes
        /// </summary>
        /// <param name="discretization">Discretization to clear</param>
        public static void ClearRuralLocations(this IDiscretization discretization)
        {
            discretization.DoWithPropertySet(nameof(discretization.SegmentGenerationMethod), SegmentGenerationMethod.None, () =>
            {
                discretization.UpdateNetworkLocations(discretization.GenerateSewerConnectionNetworkLocations().ToArray(), false);
            });
        }

        private static INetworkLocation GetMissingLocationForCompartment(this IDiscretization discretization, ICompartment compartment)
        {
            if (compartment == null)
            {
                return null;
            }

            var sourceLocations = discretization.GetLocationsForCompartment(compartment);
            if (sourceLocations.Any())
            {
                return null;
            }

            var firstIncomingConnection = GetIncomingSewerConnectionsForCompartment(compartment).FirstOrDefault();
            if (firstIncomingConnection != null)
            {
                return new NetworkLocation(firstIncomingConnection, firstIncomingConnection.Length);
            }

            var firstOutgoingConnection = GetOutgoingSewerConnectionsForCompartment(compartment).FirstOrDefault();
            if (firstOutgoingConnection != null)
            {
                return new NetworkLocation(firstOutgoingConnection, 0);
            }

            return null;
        }

        private static IEnumerable<INetworkLocation> GetLocationsForCompartment(this IDiscretization discretization, ICompartment compartment)
        {
            if (compartment == null)
            {
                return Enumerable.Empty<INetworkLocation>();
            }

            var incomingLocations = GetIncomingSewerConnectionsForCompartment(compartment)
                                     .Select(c => discretization.GetLocationsForBranch(c).FirstOrDefault(IsEndLocation))
                                     .Where(l => l != null);

            var outgoingLocations = GetOutgoingSewerConnectionsForCompartment(compartment)
                                    .Select(c => discretization.GetLocationsForBranch(c).LastOrDefault(IsBeginNode))
                                    .Where(l => l != null);

            return incomingLocations.Concat(outgoingLocations);
        }

        /// <summary>
        /// Replaces the <paramref name="discretization"/> locations with the <paramref name="newLocations"/>
        /// </summary>
        /// <param name="discretization">The discretization to set</param>
        /// <param name="newLocations">New locations to set</param>
        /// <param name="fixedPointsMask">Mask for the locations containing the fixed points (value = 1 instead of 0)</param>
        private static void ResetValues(this IDiscretization discretization, ICollection<INetworkLocation> newLocations, IEnumerable<double> fixedPointsMask = null)
        {
            // reset locations
            discretization.BeginEdit("Setting values");
            discretization.Clear();

            fixedPointsMask = fixedPointsMask ?? Enumerable.Repeat(0d, newLocations.Count).ToArray();

            discretization.Locations.DoWithPropertySet(nameof(discretization.Locations.SkipUniqueValuesCheck), true, () =>
            {
                FunctionHelper.SetValuesRaw(discretization.Locations, newLocations);
                FunctionHelper.SetValuesRaw(discretization.Components[0], fixedPointsMask);
            });

            discretization.EndEdit();

            // force refresh of caching (location dictionary) -> new locations are added
            TypeUtils.SetField(discretization, "updateLocationsDictionary", true);
        }

        /// <summary>
        /// Removes duplicate computation points from the 
        /// </summary>
        /// <param name="locations">Locations to scan</param>
        /// <param name="nodes">Nodes to cleanup</param>
        private static void CleanupLocationsAtNodes(ICollection<INetworkLocation> locations, IList<INode> nodes)
        {
            var locationsByBranch = locations
                                         .GroupBy(l => l.Branch)
                                         .ToDictionary(b => b.Key, b => b.ToList());

            IList<INetworkLocation> GetLocationsForBranch(IBranch b)
            {
                return locationsByBranch.TryGetValue(b, out var branchLocations) 
                           ? branchLocations 
                           : new List<INetworkLocation>(0);
            }

            foreach (var node in nodes)
            {
                var nodeLocations = GetNetworkLocationsAtNode(GetLocationsForBranch, node);
                if (nodeLocations.Count <= 1)
                    continue;

                var locationsToRemove = new INetworkLocation[0];

                switch (node)
                {
                    case HydroNode _:
                        locationsToRemove = nodeLocations.Skip(1).ToArray();
                        break;
                    case Manhole manhole:
                        locationsToRemove = GetDuplicatePointsForManhole(manhole, nodeLocations).ToArray();
                        break;
                }

                locationsToRemove.ForEach(n =>
                {
                    locations.Remove(n);
                    locationsByBranch[n.Branch].Remove(n);
                });
            }
        }

        /// <summary>
        /// Gets unique points for <paramref name="manhole"/> from <paramref name="locationsOnManHole"/>
        /// </summary>
        /// <param name="manhole">Manhole to search for</param>
        /// <param name="locationsOnManHole">Locations on manhole</param>
        /// <returns>Filtered <paramref name="locationsOnManHole"/> (one for each compartment)</returns>
        private static IEnumerable<INetworkLocation> GetDuplicatePointsForManhole(IManhole manhole, IList<INetworkLocation> locationsOnManHole)
        {
            if (manhole.Compartments.Count == 1)
            {
                return locationsOnManHole.Skip(1);
            }

            var duplicatePoints = new List<INetworkLocation>();
            var pointsPerCompartment = locationsOnManHole.GroupBy(GetCompartmentForLocation);

            foreach (var compartmentPoints in pointsPerCompartment.Where(p => p.Key != null))
            {
                // add one point per compartment
                duplicatePoints.AddRange(compartmentPoints.Skip(1));
            }

            return duplicatePoints;
        }

        private static List<INetworkLocation> MergeLocations(IDiscretization discretization, IEnumerable<INetworkLocation> newLocations, IMultiDimensionalArray<INetworkLocation> existingLocations)
        {
            var locationsToAdd = new List<INetworkLocation>(existingLocations);

            var newLocationsByBranch = newLocations.GroupBy(l => l.Branch);

            foreach (var locations in newLocationsByBranch)
            {
                var branch = locations.Key;
                var locationsOnBranch = discretization.GetLocationsForBranch(branch);

                if (locationsOnBranch.Count == 0)
                {
                    // only new locations on branch
                    locationsToAdd.AddRange(locations);
                    continue;
                }

                locationsToAdd.AddRange(FilterExistingLocationsForBranch(locationsOnBranch, locations, branch));
            }

            return locationsToAdd;
        }

        /// <summary>
        /// Get corresponding compartment for <paramref name="location"/>
        /// </summary>
        /// <param name="location">Location to search for</param>
        /// <returns>corresponding compartment for <paramref name="location"/></returns>
        private static ICompartment GetCompartmentForLocation(INetworkLocation location)
        {
            if (!(location.Branch is ISewerConnection sewerConnection))
                return null;

            if (IsBeginNode(location))
            {
                return sewerConnection.SourceCompartment;
            }

            if (IsEndLocation(location))
            {
                return sewerConnection.TargetCompartment;
            }

            return null;
        }

        /// <summary>
        /// Get the network locations that are on the same node (but on multiple branches)
        /// </summary>
        /// <param name="getLocationsForBranch">Function to get the locations on a branch</param>
        /// <param name="node">Node to search locations for</param>
        /// <returns>Locations that are on the <paramref name="node"/></returns>
        private static List<INetworkLocation> GetNetworkLocationsAtNode(Func<IBranch, IList<INetworkLocation>> getLocationsForBranch, INode node)
        {
            if (node == null)
                return new List<INetworkLocation>();
            
            var branches = node.IncomingBranches
                               .Select(b => new { branch = b, incoming = true })
                               .Concat(node.OutgoingBranches.Select(b => new { branch = b, incoming = false }));

            return branches
                   .SelectMany(b => GetBranchNodeLocations(getLocationsForBranch, b.branch, b.incoming))
                   .ToList();
        }

        private static IEnumerable<INetworkLocation> GetBranchNodeLocations(Func<IBranch, IList<INetworkLocation>> getLocationsForBranch, IBranch branch, bool incoming)
        {
            var locations = getLocationsForBranch(branch);
            if (!locations.Any())
            {
                yield break;
            }

            // check if first or last is at begin or end node location
            var index = incoming ? locations.Count - 1 : 0;
            var atBeginOrEnd = incoming
                                   ? Math.Abs(locations[index].Chainage - branch.Length) < 1e-8
                                   : Math.Abs(locations[index].Chainage) < 1e-8;

            if (atBeginOrEnd)
            {
                yield return locations[index];
            }
        }

        /// <summary>
        /// Filters the <paramref name="existingLocations"/> from the <paramref name="newLocations"/>
        /// </summary>
        /// <param name="existingLocations">Current locations for the <paramref name="branch"/></param>
        /// <param name="newLocations">New points to add to the <paramref name="branch"/></param>
        /// <param name="branch">Branch for the points</param>
        /// <returns>New points that are not already in the cu</returns>
        private static IEnumerable<INetworkLocation> FilterExistingLocationsForBranch(IList<INetworkLocation> existingLocations, IEnumerable<INetworkLocation> newLocations, IBranch branch)
        {
            var chainages = existingLocations.Select(l => l.Chainage).OrderBy(c => c).ToArray();
            if (chainages.Length == 0)
            {
                foreach (var networkLocation in newLocations)
                {
                    yield return networkLocation;
                }

                yield break;
            }

            var locations = newLocations.OrderBy(l => l.Chainage).ToArray();
            var chainageIndex = 0;

            for (int i = 0; i < locations.Length; i++)
            {
                var location = locations[i];
                if (Math.Abs(location.Chainage - chainages[chainageIndex]) < chainageDelta || location.Chainage > branch.Length)
                {
                    continue;
                }

                if (location.Chainage < chainages[chainageIndex])
                {
                    yield return location;
                    continue;
                }

                if (location.Chainage > chainages[chainageIndex])
                {
                    if (chainageIndex != chainages.Length - 1)
                    {
                        chainageIndex += 1;
                        i -= 1; // re-evaluate this node
                    }
                    else
                    {
                        yield return location;
                    }
                }
            }
        }

        /// <summary>
        /// Incoming <see cref="ISewerConnection"/>s for compartment
        /// </summary>
        /// <param name="compartment">Compartment to look for</param>
        /// <returns>Incoming connections</returns>
        private static IEnumerable<ISewerConnection> GetIncomingSewerConnectionsForCompartment(ICompartment compartment)
        {
            return compartment.ParentManhole.IncomingBranches.OfType<ISewerConnection>()
                              .Where(c => c.TargetCompartment == compartment);
        }

        /// <summary>
        /// Outgoing <see cref="ISewerConnection"/>s for compartment
        /// </summary>
        /// <param name="compartment">Compartment to look for</param>
        /// <returns>Outgoing connections</returns>
        private static IEnumerable<ISewerConnection> GetOutgoingSewerConnectionsForCompartment(ICompartment compartment)
        {
            return compartment.ParentManhole.OutgoingBranches.OfType<ISewerConnection>()
                              .Where(c => c.SourceCompartment == compartment);
        }

        private static bool IsBeginNode(INetworkLocation location)
        {
            return Math.Abs(location.Chainage) < chainageDelta;
        }

        private static bool IsEndLocation(INetworkLocation location)
        {
            return Math.Abs(location.Chainage - location.Branch.Length) < chainageDelta;
        }

        private static double[] FixedPointsMask(IDiscretization discretization, IList<INetworkLocation> locationsToAdd)
        {
            var values = discretization.Components[0].GetValues<double>();
            if (values.Count == 0)
            {
                return null;
            }

            var fixedLocations = new List<INetworkLocation>();
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] > 0.0)
                {
                    fixedLocations.Add(discretization.Locations.Values[i]);
                }
            }

            if (fixedLocations.Count == 0)
            {
                return null;
            }

            var fixedLocationsHashset = new HashSet<INetworkLocation>(fixedLocations);
            var fixedPointsMask = new double[locationsToAdd.Count];

            for (int i = 0; i < locationsToAdd.Count; i++)
            {
                fixedPointsMask[i] = fixedLocationsHashset.Contains(locationsToAdd[i]) ? 1.0 : 0.0;
            }

            return fixedPointsMask;
        }
    }
}