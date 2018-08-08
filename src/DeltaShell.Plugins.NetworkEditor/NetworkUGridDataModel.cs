using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.NetworkEditor.IO;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
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
        public int[] NumberOfGeometryPointsPerBranch = new int[0];
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
            NumberOfGeometryPointsPerBranch = branchGeometryPoints;
            BranchNames = branchNames;
            BranchDescriptions = branchDescriptions;
            BranchOrderNumbers = branchOrderNumbers;

            GeopointsX = geometryPointsX;
            GeopointsY = geometryPointsY;

            NumberOfNodes = nodesNames.Length;
            NumberOfBranches = branchNames.Length;
            NumberOfGeometryPoints = branchGeometryPoints.Sum();
        }

        private void SetNetworkData(IHydroNetwork network)
        {
            if (network == null) return;
            Name = network.Name ?? string.Empty;

            var compartmentCoordinateDictionary = new Dictionary<string, Coordinate>();

            if (network.Nodes != null)
            {
                var compartments = new List<Compartment>();
                network.Manholes.ForEach(m =>
                {
                    compartments.AddRange(m.Compartments);
                });
                var compartmentCount = compartments.Count;
                
                var nonManholeNetworkNodes = network.Nodes.Where(n => !(n is IManhole)).ToList();

                // The compartment coordinates are adjusted slightly for writing to UGRID
                var compartmentsX = new List<double>();
                var compartmentsY = new List<double>();
                network.Manholes.ForEach(m =>
                {
                    var numOfCompartments = m.Compartments.Count;
                    var offset = (numOfCompartments - 1) * 0.5;
                    for (var i = 0; i < numOfCompartments; i++)
                    {
                        var compartmentX = m.Geometry.Coordinate.X - offset + i;
                        var compartmentY = m.Geometry.Coordinate.Y;
                        compartmentsX.Add(compartmentX);
                        compartmentsY.Add(compartmentY);
                        compartmentCoordinateDictionary.Add(m.Compartments[i].Name, new Coordinate(compartmentX, compartmentY));
                    }
                });

                NumberOfNodes = nonManholeNetworkNodes.Count + compartmentCount;
                NodesX = nonManholeNetworkNodes.Select(n => n.Geometry.Coordinates[0].X).Concat(compartmentsX).ToArray();
                NodesY = nonManholeNetworkNodes.Select(n => n.Geometry.Coordinates[0].Y).Concat(compartmentsY).ToArray();
                NodesNames = nonManholeNetworkNodes.Select(n => n.Name).Concat(compartments.Select(c => c.Name)).ToArray();
                NodesDescriptions = nonManholeNetworkNodes.Select(n => n.Description).Concat(compartments.Select(c => string.Empty)).ToArray();
            }

            if (network.Branches != null)
            {
                NumberOfBranches = network.Branches.Count;
                SourceNodeIds = network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray();
                TargedNodesIds = network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray();
                BranchLengths = network.Branches.Select(b => b.Length).ToArray();

                NumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                NumberOfGeometryPointsPerBranch = network.Branches.Select(b => b.Geometry?.Coordinates?.Length ?? 0).ToArray();

                BranchNames = network.Branches.Select(b => b.Name).ToArray();
                BranchDescriptions = network.Branches.Select(b => b.Description).ToArray();
                BranchOrderNumbers = network.Branches.Select(b => b.OrderNumber).ToArray();
                
                var nonSewerConnections = network.Branches.Where(b => !(b is SewerConnection)).ToArray();

                // Determine the end points of the sewer connections,
                // because the compartment coordinates are adjusted slightly
                var sourceAndTargetCompartments = new List<string>();
                network.SewerConnections.ForEach(sc =>
                {
                    sourceAndTargetCompartments.Add(sc.SourceCompartment.Name);
                    sourceAndTargetCompartments.Add(sc.TargetCompartment.Name);
                });

                var compartmentXCoordinates = sourceAndTargetCompartments.Select(name => compartmentCoordinateDictionary[name].X);
                var compartmentYCoordinates = sourceAndTargetCompartments.Select(name => compartmentCoordinateDictionary[name].Y);

                GeopointsX = nonSewerConnections.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X)).Concat(compartmentXCoordinates).ToArray();
                GeopointsY = nonSewerConnections.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y)).Concat(compartmentYCoordinates).ToArray();
            }
        }

        public static IHydroNetwork ReconstructHydroNetwork(NetworkUGridDataModel dataModel, List<DelftIniCategory> branchProperties = null)
        {
            var network = new HydroNetwork
            {
                Name = dataModel.Name,
                CoordinateSystem = dataModel.CoordinateSystem
            };

            var nodes = ConstructHydroNodes(network, dataModel.NodesX, dataModel.NodesY, dataModel.NodesNames, dataModel.NodesDescriptions);

            var branches = ConstructNetworkBranches(network, nodes, dataModel.SourceNodeIds, dataModel.TargedNodesIds,
                dataModel.BranchLengths,
                dataModel.NumberOfGeometryPointsPerBranch, dataModel.BranchNames, dataModel.BranchDescriptions, 
                dataModel.GeopointsX, dataModel.GeopointsY, 
                dataModel.BranchOrderNumbers, branchProperties);

            network.Nodes.AddRange(nodes);
            network.Branches.AddRange(branches);

            return network;
        }

        private static List<IHydroNode> ConstructHydroNodes(IHydroNetwork network, double[] nodesX, double[] nodesY, string[] nodesNames, string[] nodesDescriptions)
        {
            var nodes = new List<IHydroNode>();

            var numberOfNodes = nodesX.Length;
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

            for (var i = 0; i < numberOfNodes; ++i)
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

        private static List<IBranch> ConstructNetworkBranches(INetwork parentNetwork, List<IHydroNode> nodes, int[] sourceNodes, int[] targetNodes,
            double[] branchLengths, int[] branchGeometryPoints, string[] branchNames, string[] branchDescriptions, double[] geometryPointsX, double[] geometryPointsY, int[] branchOrderNumbers, List<DelftIniCategory> branchProperties)
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
                if(branchProperties == null) branch = new Channel();
                else
                {
                    var branchIniCategory = branchProperties.FirstOrDefault(bp => bp.GetPropertyValue(BranchFile.KnownPropertyNames.Name).Equals(branchName));
                    branch = CreateBranch(branchIniCategory);
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

        private static IBranch CreateBranch(IDelftIniCategory branchCategory)
        {
            var branchTypeNumber = int.Parse(branchCategory.GetPropertyValue(BranchFile.KnownPropertyNames.BranchType));
            var type = (BranchFile.BranchTypes) branchTypeNumber;
            switch (type)
            {
                case BranchFile.BranchTypes.SewerConnection:
                    return new SewerConnection();
                case BranchFile.BranchTypes.Pipe:
                    return new Pipe();
                default:
                    return new Channel();
            }
        }
    }
}

