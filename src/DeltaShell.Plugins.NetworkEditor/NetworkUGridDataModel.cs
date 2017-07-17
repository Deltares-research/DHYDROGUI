using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor
{
    public class NetworkUGridDataModel
    {
        public string Name;
        public int NetworkId;

        public int NumberOfNodes;
        public int NumberOfBranches;
        public int NumberOfGeometryPoints;

        public double[] NodesX = new double[0];
        public double[] NodesY = new double[0];
        public string[] NodesNames = new string[0];
        public string[] NodesDescriptions = new string[0];
        public int[] SourceNodeIds = new int[0];
        public int[] TargedNodesIds = new int[0];
        public double[] BranchLengths = new double[0];
        public int[] NumberOfBranchGeometryPoints = new int[0];
        public int[] BranchOrderNumbers = new int[0];
        public string[] BranchNames = new string[0];
        public string[] BranchDescriptions = new string[0];
        public double[] GeopointsX = new double[0];
        public double[] GeopointsY = new double[0];
        public ICoordinateSystem CoordinateSystem;

        public NetworkUGridDataModel(IHydroNetwork network)
        {
            SetNetworkData(network);
        }

        public NetworkUGridDataModel(string networkName, ICoordinateSystem coordinateSystem, double[] nodesX, double[] nodesY, string[] nodesNames, string[] nodesDescriptions, int[] sourceNodes, int[] targetNodes, double[] branchLengths, int[] branchGeometryPoints, string[] branchNames, string[] branchDescriptions, double[] geometryPointsX, double[] geometryPointsY, int[] branchOrderNumbers)
        {
            Name = networkName;
            CoordinateSystem = coordinateSystem;

            NodesX = nodesX;
            NodesY = nodesY;
            NodesNames = nodesNames;
            NodesDescriptions = nodesDescriptions;
            SourceNodeIds = sourceNodes;
            TargedNodesIds = targetNodes;

            BranchLengths = branchLengths;
            NumberOfBranchGeometryPoints = branchGeometryPoints;
            BranchNames = branchNames;
            BranchDescriptions = branchDescriptions;

            BranchOrderNumbers = branchOrderNumbers;

            GeopointsX = geometryPointsX;
            GeopointsY = geometryPointsY;
        }

        private void SetNetworkData(IHydroNetwork network)
        {
            if (network == null) return;
            Name = network.Name ?? string.Empty;

            if (network.Nodes != null)
            {
                NumberOfNodes = network.Nodes.Count;
                NodesX = network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray();
                NodesY = network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray();
                NodesNames = network.Nodes.Select(n => n.Name).ToArray();
                NodesDescriptions = network.Nodes.Select(n => n.Description).ToArray();
            }

            if (network.Branches != null)
            {
                NumberOfBranches = network.Branches.Count;
                SourceNodeIds = network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray();
                TargedNodesIds = network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray();
                BranchLengths = network.Branches.Select(b => b.Length).ToArray();

                NumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                NumberOfBranchGeometryPoints = network.Branches.Select(b =>
                    {
                        if (b.Geometry != null && b.Geometry.Coordinates != null)
                        {
                            return b.Geometry.Coordinates.Length;
                        }
                        return 0;
                    }
                ).ToArray();

                BranchNames = network.Branches.Select(b => b.Name).ToArray();
                BranchDescriptions = network.Branches.Select(b => b.Description).ToArray();

                BranchOrderNumbers = network.Branches.Select(b => b.OrderNumber).ToArray();

                GeopointsX = network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray();
                GeopointsY = network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray();
            }
        }

        public static IHydroNetwork ReconstructHydroNetwork(NetworkUGridDataModel dataModel)
        {
            var network = new HydroNetwork
            {
                Name = dataModel.Name,
                CoordinateSystem = dataModel.CoordinateSystem
            };

            var nodes = ConstructHydroNodes(network, dataModel.NodesX, dataModel.NodesY, dataModel.NodesNames, dataModel.NodesDescriptions);

            var branches = ConstructNetworkBranches(network, nodes, dataModel.SourceNodeIds, dataModel.TargedNodesIds,
                dataModel.BranchLengths,
                dataModel.NumberOfBranchGeometryPoints, dataModel.BranchNames, dataModel.BranchDescriptions, 
                dataModel.GeopointsX, dataModel.GeopointsY, 
                dataModel.BranchOrderNumbers);

            network.Nodes.AddRange(nodes);
            network.Branches.AddRange(branches);

            return network;
        }

        private static List<IHydroNode> ConstructHydroNodes(IHydroNetwork network, double[] nodesX, double[] nodesY, string[] nodesNames, string[] nodesDescriptions)
        {
            var nodes = new List<IHydroNode>();

            int numberOfNodes = nodesX.Length;
            if (numberOfNodes <= 0)
            {
                return nodes;
            }

            if (nodesX.Length != nodesY.Length
                || nodesX.Length != nodesNames.Length
                || nodesX.Length != nodesDescriptions.Length)
            {
                throw new InvalidOperationException(string.Format("The arrays are not the same length"));
            }

            for (int i = 0; i < numberOfNodes; ++i)
            {
                var node = new HydroNode
                {
                    Network = network,
                    Name = nodesNames[i] == "" ? null : nodesNames[i],
                    Description = nodesDescriptions[i] == "" ? null : nodesDescriptions[i],
                    Geometry = new Point(nodesX[i], nodesY[i])
                };
                nodes.Add(node);
            }

            return nodes;
        }

        private static List<IChannel> ConstructNetworkBranches(INetwork parentNetwork, List<IHydroNode> nodes, int[] sourceNodes, int[] targetNodes,
            double[] branchLengths, int[] branchGeometryPoints, string[] branchNames, string[] branchDescriptions, double[] geometryPointsX, double[] geometryPointsY, int[] branchOrderNumbers)
        {
            var channels = new List<IChannel>();
            int numberOfChannels = sourceNodes.Length;

            if (numberOfChannels <= 0)
            {
                return channels;
            }

            if (numberOfChannels != targetNodes.Length
                || numberOfChannels != branchLengths.Length
                || numberOfChannels != branchNames.Length
                || numberOfChannels != branchGeometryPoints.Length
                || numberOfChannels != branchDescriptions.Length)
            {
                throw new InvalidOperationException(string.Format("The arrays are not the same length"));
            }

            int totalNumberOfGeometryPoints = branchGeometryPoints.Sum();

            if (totalNumberOfGeometryPoints != geometryPointsX.Length
                || totalNumberOfGeometryPoints != geometryPointsY.Length)
            {
                throw new InvalidOperationException(string.Format("Mismatch in the geometry point array lenghts"));
            }

            Coordinate[] geometryCoordinates = new Coordinate[totalNumberOfGeometryPoints];

            for (int j = 0; j < totalNumberOfGeometryPoints; ++j)
            {
                geometryCoordinates[j] = new Coordinate(geometryPointsX[j], geometryPointsY[j]);
            }

            int geoPointsIndex = 0;
            for (int i = 0; i < numberOfChannels; ++i)
            {
                int sourceNodeIndex = sourceNodes[i];
                int targetNodeIndex = targetNodes[i];
                int numberOfBranchGeometryPoints = branchGeometryPoints[i];

                Coordinate[] coordinates = geometryCoordinates.Skip(geoPointsIndex).Take(numberOfBranchGeometryPoints).ToArray();
                geoPointsIndex += numberOfBranchGeometryPoints;

                var channel = new Channel
                {
                    Network = parentNetwork,
                    Name = branchNames[i] == "" ? null : branchNames[i],
                    Description = branchDescriptions[i] == "" ? null : branchDescriptions[i],
                    Source = nodes[sourceNodeIndex],
                    Target = nodes[targetNodeIndex],
                    Geometry = new LineString(coordinates),
                    OrderNumber = branchOrderNumbers[i]
                };

                channels.Add(channel);
            }

            return channels;
        }
    }
}

