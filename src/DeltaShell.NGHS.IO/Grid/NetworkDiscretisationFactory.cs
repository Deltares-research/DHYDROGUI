using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileWriters.Network;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class NetworkDiscretisationFactory
    {
        public static IHydroNetwork CreateHydroNetwork(NetworkUGridDataModel dataModel, IEnumerable<BranchFile.BranchProperties> branchProperties = null, ICollection<NodeFile.CompartmentProperties> compartmentProperties = null, IHydroNetwork network = null)
        {
            if (network == null)
            {
                network = new HydroNetwork();
            }

            network.Name = dataModel.Name;
            network.CoordinateSystem = dataModel.CoordinateSystem;

            var nodes = CreateNodes(network, dataModel.NodesX, dataModel.NodesY, dataModel.NodesNames, dataModel.NodesDescriptions, compartmentProperties).ToList();

            var branches = CreateNetworkBranches(network, nodes, dataModel.SourceNodeIds, dataModel.TargedNodesIds,
                dataModel.BranchLengths,
                dataModel.NumberOfGeometryPointsPerBranch, dataModel.BranchNames, dataModel.BranchDescriptions,
                dataModel.GeopointsX, dataModel.GeopointsY,
                dataModel.BranchOrderNumbers, branchProperties);

            network.Nodes.AddRange(nodes);
            network.Branches.AddRange(branches);

            return network;
        }

        public static IDiscretization CreateNetworkDiscretisation(IHydroNetwork network, NetworkDiscretisationUGridDataModel discretisationDataModel)
        {
            if (network?.Branches == null)
            {
                return new Discretization();
            }

            var discretisation = new Discretization
            {
                Name = discretisationDataModel.Name,
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered
            };

            // check if size of branchindices and offsets are equal and > 0.
            var branchIndices = discretisationDataModel.BranchIdx;
            var offset = discretisationDataModel.Offsets;
            var discretisationPointIds = discretisationDataModel.DiscretisationPointIds;
            var discretisationPointDescriptions = discretisationDataModel.DiscretisationPointDescriptions;

            if (branchIndices.Length != offset.Length
                || branchIndices.Length != discretisationPointIds.Length)
            {
                return discretisation;// throw new Exception(string.Format("Can't reconstruct the network discretisation because the "));
            }

            // make int[] of unique branch indices
            var uniqueBranchIndices = branchIndices.Distinct();

            // get the size and check if there are that many branches.
            /*if (network.Branches.Count != uniqueBranchIndices.Count())
            {
                return null;
            }*/

            for (var i = 0; i < branchIndices.Length; i++)
            {
                var branchIndex = branchIndices[i];
                var branch = network.Branches[branchIndex];
                var discretisationPointId = discretisationPointIds[i];
                var discretisationPointDescription = discretisationPointDescriptions[i];

                var networkLocation =
                    new NetworkLocation(branch, offset[i])
                    {
                        Name = discretisationPointId,
                        LongName = discretisationPointDescription
                    };

                discretisation.Locations.Values.Add(networkLocation);
            }

            return discretisation;
        }

        private static IEnumerable<INode> CreateNodes(IHydroNetwork network, double[] nodesX, double[] nodesY, string[] nodesNames, string[] nodesDescriptions, ICollection<NodeFile.CompartmentProperties> propertiesPerCompartment)
        {
            var numberOfNodes = nodesX.Length;
            if (numberOfNodes <= 0)
            {
                yield break;
            }

            if (nodesX.Length != nodesY.Length
                || nodesX.Length != nodesNames.Length
                || nodesX.Length != nodesDescriptions.Length)
            {
                throw new InvalidOperationException("The arrays are not the same length");
            }

            var propertiesLookup = propertiesPerCompartment?.ToDictionary(p => p.CompartmentId);
            var manHoleLookup = network.Manholes.ToDictionary(m => m.Name);

            for (var i = 0; i < numberOfNodes; ++i)
            {
                INode node = null;

                var nodeName = nodesNames[i];
                if (propertiesLookup != null && propertiesLookup.TryGetValue(nodeName, out var compartmentProperties))
                {
                    var manholeWidth = Math.Sqrt(compartmentProperties.Area);
                    var compartment = new Compartment(compartmentProperties.CompartmentId)
                    {
                        BottomLevel = compartmentProperties.BedLevel,
                        SurfaceLevel = compartmentProperties.StreetLevel,
                        FloodableArea = compartmentProperties.StreetStorageArea,
                        ManholeLength = manholeWidth,
                        ManholeWidth = manholeWidth
                    };

                    if (manHoleLookup.TryGetValue(compartmentProperties.ManholeId, out var existingManhole))
                    {
                        existingManhole.Compartments.Add(compartment);
                        continue;
                    }

                    node = new Manhole(compartmentProperties.ManholeId)
                    {
                        Compartments = new EventedList<ICompartment> {compartment}
                    };
                    manHoleLookup[node.Name] = (IManhole) node;
                }
                else
                {
                    node = new HydroNode
                    {
                        Name = nodeName == "" ? null : nodeName,
                        LongName = nodesDescriptions[i] == "" ? null : nodesDescriptions[i]
                    };
                }

                node.Description = nodesDescriptions[i] == "" ? null : nodesDescriptions[i];
                node.Geometry = new Point(nodesX[i], nodesY[i]);
                node.Network = network;

                yield return node;
            }
        }

        private static IEnumerable<IBranch> CreateNetworkBranches(INetwork parentNetwork, IEnumerable<INode> nodes, int[] sourceNodes, int[] targetNodes,
            double[] branchLengths, int[] branchGeometryPoints, string[] branchNames, string[] branchDescriptions, double[] geometryPointsX, double[] geometryPointsY, int[] branchOrderNumbers, IEnumerable<BranchFile.BranchProperties> propertiesPerBranch)
        {
            int numberOfChannels = sourceNodes.Length;

            if (numberOfChannels <= 0)
            {
                yield break;
            }

            if (numberOfChannels != targetNodes.Length
                || numberOfChannels != branchLengths.Length
                || numberOfChannels != branchNames.Length
                || numberOfChannels != branchGeometryPoints.Length
                || numberOfChannels != branchDescriptions.Length)
            {
                throw new InvalidOperationException("The arrays are not the same length");
            }

            var totalNumberOfGeometryPoints = branchGeometryPoints.Sum();

            if (totalNumberOfGeometryPoints != geometryPointsX.Length
                || totalNumberOfGeometryPoints != geometryPointsY.Length)
            {
                throw new InvalidOperationException("Mismatch in the geometry point array lenghts");
            }

            var geometryCoordinates = new Coordinate[totalNumberOfGeometryPoints];

            for (var j = 0; j < totalNumberOfGeometryPoints; ++j)
            {
                geometryCoordinates[j] = new Coordinate(geometryPointsX[j], geometryPointsY[j]);
            }

            var geoPointsIndex = 0;
            var nodesArray = nodes.ToArray();
            for (var i = 0; i < numberOfChannels; ++i)
            {
                var sourceNodeIndex = sourceNodes[i];
                var targetNodeIndex = targetNodes[i];
                var numberOfBranchGeometryPoints = branchGeometryPoints[i];

                var coordinates = geometryCoordinates.Skip(geoPointsIndex).Take(numberOfBranchGeometryPoints).ToArray();
                geoPointsIndex += numberOfBranchGeometryPoints;

                IBranch branch;
                var branchName = branchNames[i];
                if (propertiesPerBranch == null) branch = new Channel();
                else
                {
                    var branchProperties = propertiesPerBranch.FirstOrDefault(bp => bp.Name.Equals(branchName));
                    branch = CreateBranch(branchProperties);
                }

                branch.Network = parentNetwork;
                branch.Name = branchNames[i] == "" ? null : branchNames[i];
                branch.Description = branchDescriptions[i] == "" ? null : branchDescriptions[i];
                
                branch.Source = nodesArray[sourceNodeIndex];
                branch.Target = nodesArray[targetNodeIndex];
                branch.Geometry = new LineString(coordinates);
                branch.OrderNumber = branchOrderNumbers[i];

                if (!branch.IsLengthCustom)
                {
                    branch.IsLengthCustom = !branch.Length.IsEqualTo(branchLengths[i], 0.001);
                }
                if (branch.IsLengthCustom) branch.Length = branchLengths[i];

                yield return branch;
            }
        }

        private static IBranch CreateBranch(BranchFile.BranchProperties branchProperties)
        {
            switch (branchProperties.BranchType)
            {
                case BranchFile.BranchType.SewerConnection:
                    return new SewerConnection { WaterType = branchProperties.WaterType };
                case BranchFile.BranchType.Pipe:
                    return new Pipe { WaterType = branchProperties.WaterType, Material = branchProperties.Material}; ;
                default:
                    return new Channel();
            }
        }
    }
}
