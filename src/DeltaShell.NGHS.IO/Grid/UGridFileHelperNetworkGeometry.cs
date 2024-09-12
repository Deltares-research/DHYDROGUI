using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridFileHelperNetworkGeometry
    {

        /// <summary>
        /// Sets the nodes and branches of the <paramref name="network"/> with the values of the <paramref name="networkGeometry"/>
        /// </summary>
        /// <param name="network">Network to set</param>
        /// <param name="networkGeometry"><see cref="DisposableNetworkGeometry"/> containing the network data</param>
        /// <param name="branchProperties">Additional branch properties</param>
        /// <param name="compartmentProperties">Additional compartment properties</param>
        /// <param name="forceCustomLengths">Force all branches in the network to have custom lengths and use the lengths that are read from file</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the node or compartment names are not unique.</exception>
        public void SetNetworkGeometry(IHydroNetwork network,
                                              DisposableNetworkGeometry networkGeometry,
                                              IEnumerable<BranchProperties> branchProperties,
                                              IEnumerable<CompartmentProperties> compartmentProperties,
                                              bool forceCustomLengths = false)
        {
            Ensure.NotNull(network, nameof(network));
            Ensure.NotNull(networkGeometry, nameof(networkGeometry));
            Ensure.NotNull(branchProperties, nameof(branchProperties));
            Ensure.NotNull(compartmentProperties, nameof(compartmentProperties));

            INode[] nodes = CreateNetworkNodes(network, networkGeometry, compartmentProperties);

            IReadOnlyDictionary<string, INode> nodeLookup = CreateNodeLookup(nodes);
            IReadOnlyDictionary<string, ICompartment> compartmentLookup = CreateCompartmentLookup(nodes);

            IEnumerable<IBranch> branches = CreateBranches(networkGeometry, branchProperties, forceCustomLengths, nodeLookup, compartmentLookup);

            network.Nodes.AddRange(nodes.Distinct());
            network.Branches.AddRange(branches);
        }

        private static IReadOnlyDictionary<string, ICompartment> CreateCompartmentLookup(IEnumerable<INode> nodes)
        {
            return nodes
                   .OfType<IManhole>()
                   .SelectMany(m => m.Compartments)
                   .ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_NetworkCompartmentNamesContext, c => c.Name);
        }

        private IReadOnlyDictionary<string, INode> CreateNodeLookup(IEnumerable<INode> nodes)
        {
            return nodes
                   .SelectMany(n => GetNodeLookupNames(n)
                                   .Select(name => new
                                   {
                                       key = name,
                                       node = n
                                   })).ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_NetworkNodesContext,
                                                                    n => n.key,
                                                                    n => n.node);
        }

        private static IEnumerable<IBranch> CreateBranches(DisposableNetworkGeometry networkGeometry,
                                                           IEnumerable<BranchProperties> branchProperties,
                                                           bool forceCustomLengths,
                                                           IReadOnlyDictionary<string, INode> nodeLookup,
                                                           IReadOnlyDictionary<string, ICompartment> compartmentLookup)
        {
            var propertiesLookup = branchProperties?.ToDictionary(p => p.Name) ?? new Dictionary<string, BranchProperties>();

            var branches = new List<IBranch>();

            var geometryOffset = 0;

            for (int i = 0; i < networkGeometry.BranchIds.Length; i++)
            {
                propertiesLookup.TryGetValue(networkGeometry.BranchIds[i], out var properties);
                branches.Add(CreateBranchByIndex(networkGeometry, i, properties, nodeLookup, compartmentLookup, ref geometryOffset, forceCustomLengths));
            }

            return branches.ToArray();
        }

        private static IBranch CreateBranchByIndex(DisposableNetworkGeometry networkGeometry, int branchIndex, BranchProperties branchProperties, IReadOnlyDictionary<string, INode> nodeLookup, IReadOnlyDictionary<string, ICompartment> compartments, ref int geometryOffset, bool forceCustomLengths)
        {
            var toNodeId = networkGeometry.NodeIds[networkGeometry.NodesTo[branchIndex]];
            var fromNodeId = networkGeometry.NodeIds[networkGeometry.NodesFrom[branchIndex]];
            var branchId = networkGeometry.BranchIds[branchIndex];

            INode source = nodeLookup[fromNodeId];
            INode target = nodeLookup[toNodeId];
            var branch = GetBranch(branchProperties, source, target);

            branch.Name = branchId;
            branch.OrderNumber = networkGeometry.BranchOrder[branchIndex];

            var nodeCount = networkGeometry.BranchGeometryNodesCount[branchIndex];
            branch.Geometry = GetLineStringForBranch(networkGeometry, nodeCount, geometryOffset);
            geometryOffset += nodeCount;

            ((IHydroNetworkFeature)branch).LongName = networkGeometry.BranchLongNames[branchIndex];

            if (forceCustomLengths || (branchProperties?.IsCustomLength ?? false))
            {
                branch.Length = 0;
                branch.Length = networkGeometry.BranchLengths[branchIndex];
                branch.IsLengthCustom = true;
            }

            if (branch is IPipe pipe)
            {
                pipe.WaterType = ToPipeWaterType((BranchType)networkGeometry.BranchTypes[branchIndex]);
            }

            if (branch is ISewerConnection sewerConnection)
            {
                // compartment can be null when coupled to rural network
                sewerConnection.SourceCompartment = compartments.ContainsKey(fromNodeId) ? compartments[fromNodeId] : null;
                sewerConnection.TargetCompartment = compartments.ContainsKey(toNodeId) ? compartments[toNodeId] : null;
            }

            return branch;
        }

        private static LineString GetLineStringForBranch(DisposableNetworkGeometry networkGeometry, int nodeCount, int geometryOffset)
        {
            var coordinates = new Coordinate[nodeCount];

            for (int j = 0; j < nodeCount; j++)
            {
                var geometryIndex = j + geometryOffset;
                coordinates[j] = new Coordinate(networkGeometry.BranchGeometryX[geometryIndex], networkGeometry.BranchGeometryY[geometryIndex]);
            }

            return new LineString(coordinates);
        }

        private static SewerConnectionWaterType ToPipeWaterType(BranchType networkGeometryBranchType)
        {
            switch (networkGeometryBranchType)
            {
                case BranchType.DryWeatherFlow:
                    return SewerConnectionWaterType.DryWater;
                case BranchType.StormWaterFlow:
                    return SewerConnectionWaterType.StormWater;
                case BranchType.MixedFlow:
                    return SewerConnectionWaterType.Combined;
            }
            return SewerConnectionWaterType.None;

        }

        private static IEnumerable<string> GetNodeLookupNames(INode node)
        {
            if (node is IManhole manhole)
            {
                foreach (var compartment in manhole.Compartments)
                {
                    yield return compartment.Name;
                }
                yield break;
            }

            yield return node.Name;
        }

        private static IBranch GetBranch(BranchProperties branchProperties, INode source, INode target)
        {
            if (branchProperties == null)
            {
                if (source is Manhole || target is Manhole)
                {
                    return new Pipe { Source = source, Target = target };
                }

                return new Channel(source, target);
            }

            var branchType = branchProperties.BranchType;

            IBranch branch;
            switch (branchType)
            {
                case BranchFile.BranchType.SewerConnection:
                    branch = new SewerConnection { Source = source, Target = target, WaterType = branchProperties.WaterType };
                    break;
                case BranchFile.BranchType.Pipe:
                    branch = new Pipe { Source = source, Target = target, WaterType = branchProperties.WaterType, Material = branchProperties.Material };
                    break;
                default:
                    branch = new Channel(source, target);
                    break;
            }

            return branch;
        }

        private static INode[] CreateNetworkNodes(IHydroNetwork network,
                                                  DisposableNetworkGeometry networkGeometry,
                                                  IEnumerable<CompartmentProperties> compartmentProperties = null)
        {
            Dictionary<string, CompartmentProperties> compartmentPropertiesLookup = CreateCompartmentPropertiesLookup(compartmentProperties);
            Dictionary<string, IManhole> manHoleLookup = CreateManholeLookup(network);

            var nodes = new List<INode>();
            var manholesToFix = new List<IManhole>();

            int nodeCount = networkGeometry.NodesX.Length;
            for (var i = 0; i < nodeCount; i++)
            {
                INode node;

                string nodeName = networkGeometry.NodeIds[i];
                bool isCompartment = compartmentPropertiesLookup.ContainsKey(nodeName);

                if (isCompartment)
                {
                    CompartmentProperties properties = compartmentPropertiesLookup[nodeName];
                    Compartment compartment = CreateCompartment(nodeName, properties);

                    if (CompartmentIsInExistingManhole(manHoleLookup, properties.ManholeId))
                    {
                        IManhole existingManhole = manHoleLookup[properties.ManholeId];

                        existingManhole.Compartments.Add(compartment);
                        manholesToFix.Add(existingManhole);

                        continue;
                    }

                    node = CreateManhole(networkGeometry, i, properties, compartment);
                    manHoleLookup[node.Name] = (IManhole)node;
                }
                else
                {
                    node = CreateHydroNode(networkGeometry, i, nodeName);
                }

                ((IHydroNetworkFeature)node).LongName = GetNodeLongName(networkGeometry, i);

                node.Network = network;

                nodes.Add(node);
            }

            foreach (IManhole manhole in manholesToFix)
            {
                FixManholeGeometry(manhole);
            }

            return nodes.ToArray();
        }

        private static Dictionary<string, CompartmentProperties> CreateCompartmentPropertiesLookup(IEnumerable<CompartmentProperties> compartmentProperties)
        {
            Dictionary<string, CompartmentProperties> lookup = compartmentProperties?
                .ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_CompartmentIdContext, p => p.CompartmentId);

            return lookup ?? new Dictionary<string, CompartmentProperties>();
        }

        private static Dictionary<string, IManhole> CreateManholeLookup(IHydroNetwork network)
        {
            return network.Manholes.ToDictionaryWithErrorDetails(Resources.HydroUGridExtensions_ManholeNamesContext, m => m.Name);
        }

        private static Compartment CreateCompartment(string nodeName, CompartmentProperties properties)
        {
            double manholeWidth = Math.Sqrt(properties.Area);

            var compartment = new Compartment(nodeName)
            {
                BottomLevel = properties.BedLevel,
                SurfaceLevel = properties.StreetLevel,
                FloodableArea = properties.StreetStorageArea,
                ManholeLength = manholeWidth,
                ManholeWidth = manholeWidth,
                Shape = properties.CompartmentShape,
                CompartmentStorageType = properties.CompartmentStorageType
            };

            if (properties.UseTable)
            {
                AddTablePropertiesToCompartment(compartment, properties);
            }

            return compartment;
        }

        private static void AddTablePropertiesToCompartment(ICompartment compartment, CompartmentProperties properties)
        {
            compartment.UseTable = true;
            compartment.Storage.Arguments[0].InterpolationType = properties.Interpolation;
            compartment.Storage.Arguments[0].SetValues(properties.Levels);
            compartment.Storage.Components[0].SetValues(properties.StorageAreas);
        }

        private static bool CompartmentIsInExistingManhole(
            IReadOnlyDictionary<string, IManhole> manHoleLookup,
            string manholeId)
        {
            return manHoleLookup.ContainsKey(manholeId);
        }

        private static Manhole CreateManhole(DisposableNetworkGeometry networkGeometry,
                                             int nodeIndex,
                                             CompartmentProperties properties,
                                             ICompartment compartment)
        {
            return new Manhole(properties.ManholeId)
            {
                Compartments = new EventedList<ICompartment> { compartment },
                Geometry = new Point(networkGeometry.NodesX[nodeIndex], networkGeometry.NodesY[nodeIndex])
            };
        }

        private static HydroNode CreateHydroNode(DisposableNetworkGeometry networkGeometry, int nodeIndex, string nodeName)
        {
            return new HydroNode
            {
                Name = nodeName == "" ? null : nodeName,
                Geometry = new Point(networkGeometry.NodesX[nodeIndex], networkGeometry.NodesY[nodeIndex])
            };
        }

        private static string GetNodeLongName(DisposableNetworkGeometry networkGeometry, int nodeIndex)
        {
            return networkGeometry.NodeLongNames[nodeIndex] != ""
                       ? networkGeometry.NodeLongNames[nodeIndex]
                       : null;
        }

        private static void FixManholeGeometry(IManhole manhole)
        {
            Coordinate firstCompartmentLocation = manhole.Geometry.Coordinate;
            double offset = (manhole.Compartments.Count - 1) / 2.0;
            manhole.Geometry = new Point(firstCompartmentLocation.X + offset, firstCompartmentLocation.Y);
        }
    }
}