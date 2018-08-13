using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.IO;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor
{
    public static class NetworkDiscretisationFactory
    {
        public static IHydroNetwork CreateHydroNetwork(NetworkUGridDataModel dataModel, IList<BranchFile.BranchProperties> branchProperties = null, IList<NodeFile.CompartmentProperties> compartmentProperties = null)
        {
            var network = new HydroNetwork
            {
                Name = dataModel.Name,
                CoordinateSystem = dataModel.CoordinateSystem
            };

            var nodes = CreateNodes(network, dataModel.NodesX, dataModel.NodesY, dataModel.NodesNames, dataModel.NodesDescriptions, compartmentProperties);

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
                return null;
            }

            var discretisation = new Discretization
            {
                Name = discretisationDataModel.Name,
                Network = network,
            };

            // check if size of branchindices and offsets are equal and > 0.
            var branchIndices = discretisationDataModel.BranchIdx;
            var offset = discretisationDataModel.Offsets;
            var discretisationPointIds = discretisationDataModel.DiscretisationPointIds;
            var discretisationPointDescriptions = discretisationDataModel.DiscretisationPointDescriptions;

            if (branchIndices.Length != offset.Length
                || branchIndices.Length != discretisationPointIds.Length)
            {
                return null;// throw new Exception(string.Format("Can't reconstruct the network discretisation because the "));
            }

            // make int[] of unique branch indices
            var uniqueBranchIndices = branchIndices.Distinct();

            // get the size and check if there are that many branches.
            if (network.Branches.Count != uniqueBranchIndices.Count())
            {
                return null;
            }

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

        private static List<INode> CreateNodes(IHydroNetwork network, double[] nodesX, double[] nodesY, string[] nodesNames, string[] nodesDescriptions, IList<NodeFile.CompartmentProperties> propertiesPerCompartment)
        {
            var nodes = new List<INode>();

            var numberOfNodes = nodesX.Length;
            if (numberOfNodes <= 0)
            {
                return nodes;
            }

            if (nodesX.Length != nodesY.Length
                || nodesX.Length != nodesNames.Length
                || nodesX.Length != nodesDescriptions.Length)
            {
                throw new InvalidOperationException("The arrays are not the same length");
            }

            for (var i = 0; i < numberOfNodes; ++i)
            {
                var compartmentProperties = propertiesPerCompartment.FirstOrDefault(p => p.CompartmentId.Equals(nodesNames[i]));
                if (compartmentProperties != null)
                {
                    var existingManhole = (Manhole) network.Manholes.FirstOrDefault(m => m.Name.Equals(compartmentProperties.ManholeId));
                    var compartment = new Compartment(compartmentProperties.CompartmentId)
                    {
                        BottomLevel = compartmentProperties.BottomLevel,
                        SurfaceLevel = compartmentProperties.StreetLevel,
                        ManholeLength = Math.Sqrt(compartmentProperties.Area),
                        ManholeWidth = Math.Sqrt(compartmentProperties.Area)
                    };
                    if (existingManhole != null)
                    {
                        existingManhole.Compartments.Add(compartment);
                    }
                    else
                    {
                        var manhole = new Manhole(compartmentProperties.ManholeId)
                        {
                            Description = nodesDescriptions[i] == "" ? null : nodesDescriptions[i],
                            Compartments = new EventedList<Compartment> { compartment },
                            Geometry = new Point(nodesX[i], nodesY[i]),
                            Network = network
                        };
                        nodes.Add(manhole);
                    }
                }
                else
                {
                    var node = new HydroNode
                    {
                        Name = nodesNames[i] == "" ? null : nodesNames[i],
                        Description = nodesDescriptions[i] == "" ? null : nodesDescriptions[i],
                        Geometry = new Point(nodesX[i], nodesY[i]),
                        Network = network
                    };
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private static List<IBranch> CreateNetworkBranches(INetwork parentNetwork, List<INode> nodes, int[] sourceNodes, int[] targetNodes,
            double[] branchLengths, int[] branchGeometryPoints, string[] branchNames, string[] branchDescriptions, double[] geometryPointsX, double[] geometryPointsY, int[] branchOrderNumbers, IList<BranchFile.BranchProperties> propertiesPerBranch)
        {
            var branches = new List<IBranch>();
            int numberOfChannels = sourceNodes.Length;

            if (numberOfChannels <= 0)
            {
                return branches;
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
                branch.Source = nodes[sourceNodeIndex];
                branch.Target = nodes[targetNodeIndex];
                branch.Geometry = new LineString(coordinates);
                branch.OrderNumber = branchOrderNumbers[i];

                if (!branch.IsLengthCustom)
                {
                    branch.IsLengthCustom = !branch.Length.IsEqualTo(branchLengths[i], 0.001);
                }
                if (branch.IsLengthCustom) branch.Length = branchLengths[i];

                branches.Add(branch);
            }

            return branches;
        }

        private static IBranch CreateBranch(BranchFile.BranchProperties branchProperties)
        {
            switch (branchProperties.BranchType)
            {
                case BranchFile.BranchType.SewerConnection:
                    return new SewerConnection { WaterType = branchProperties.WaterType };
                case BranchFile.BranchType.Pipe:
                    return new Pipe { WaterType = branchProperties.WaterType }; ;
                default:
                    return new Channel();
            }
        }
    }
}
