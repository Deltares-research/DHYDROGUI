using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor
{
    public static class UGridToNetworkAdapter
    {
        private const string IO_NETCDF_NETWORK_ID = "IoNetCdfNetworkId";

        public class IntermediateObject
        {
            public string Name { get; set; }
            public IFeatureAttributeCollection Attributes { get; set; }

            public int NumberOfNodes;
            public int NumberOfBranches;
            public int NumberOfGeometryPoints;

            public double[] NodesX;
            public double[] NodesY;
            public string[] NodesNames;
            public string[] NodesDescriptions;
            public int[] SourceNodeIds;
            public int[] TargedNodesIds;
            public double[] BranchLengths;
            public int[] NumberOfBranchGeometryPoints;
            public string[] BranchNames;
            public string[] BranchDescriptions;
            public double[] GeopointsX;
            public double[] GeopointsY;
            public ICoordinateSystem CoordinateSystem;


            public IntermediateObject()
            {
                
            }

            public IntermediateObject(IHydroNetwork network)
            {
                Name = network.Name;
                NumberOfNodes = network.Nodes.Count;
                NumberOfBranches = network.Branches.Count;
                NumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);

                NodesX = network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray();
                NodesY = network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray();
                NodesNames = network.Nodes.Select(n => n.Name).ToArray();
                NodesDescriptions = network.Nodes.Select(n => n.Description).ToArray();

                SourceNodeIds = network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray();
                TargedNodesIds = network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray();
                BranchLengths = network.Branches.Select(b => b.Length).ToArray();
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

                GeopointsX = network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray();
                GeopointsY = network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray();
            }
        }

        public static void SaveNetwork(IHydroNetwork network, string netFilePath, UGridGlobalMetaData metaData)
        {
            var intermediateNetworkObject = new IntermediateObject(network);
            try
            {
                using (var uGrid1D = new UGrid1D(netFilePath, metaData))
                {
                    uGrid1D.CreateFile();

                    uGrid1D.Initialize();

                    int networkId;
                    
                    uGrid1D.Create1DNetworkInFile(intermediateNetworkObject.Name,
                        intermediateNetworkObject.NumberOfNodes,
                        intermediateNetworkObject.NumberOfBranches,
                        intermediateNetworkObject.NumberOfGeometryPoints,
                        out networkId);

                    intermediateNetworkObject.Attributes = new DictionaryFeatureAttributeCollection
                    {
                        {IO_NETCDF_NETWORK_ID, networkId}
                    };

                    uGrid1D.Write1DNetworkNodes(intermediateNetworkObject.NodesX, intermediateNetworkObject.NodesY,
                        intermediateNetworkObject.NodesNames, intermediateNetworkObject.NodesDescriptions);

                    uGrid1D.Write1DNetworkBranches(intermediateNetworkObject.SourceNodeIds,
                        intermediateNetworkObject.TargedNodesIds,
                        intermediateNetworkObject.BranchLengths,
                        intermediateNetworkObject.NumberOfBranchGeometryPoints,
                        intermediateNetworkObject.BranchNames,
                        intermediateNetworkObject.BranchDescriptions);

                    uGrid1D.Write1DNetworkGeometry(intermediateNetworkObject.GeopointsX, intermediateNetworkObject.GeopointsY);
                    
                    //var totalNumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                    //uGrid1D.Create1DNetworkInFile(
                    //    network.Name,
                    //    network.Nodes.Count,
                    //    network.Branches.Count,
                    //    totalNumberOfGeometryPoints,
                    //    out networkId);

                    //uGrid1D.Write1DNetworkNodes(
                    //    network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray(),
                    //    network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray(),
                    //    network.Nodes.Select(n => n.Name).ToArray(),
                    //    network.Nodes.Select(n => n.Description).ToArray()
                    //);

                    //uGrid1D.Write1DNetworkBranches(
                    //    network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                    //    network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                    //    network.Branches.Select(b => b.Length).ToArray(),
                    //    network.Branches.Select(b =>
                    //        {
                    //            if (b.Geometry != null && b.Geometry.Coordinates != null)
                    //            {
                    //                return b.Geometry.Coordinates.Length;
                    //            }
                    //            return 0;
                    //        }
                    //    ).ToArray(),
                    //    network.Branches.Select(b => b.Name).ToArray(),
                    //    network.Branches.Select(b => b.Description).ToArray()
                    //);
                    
                    //uGrid1D.Write1DNetworkGeometry(
                    //    network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray(),
                    //    network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray()
                    //);
                }
            }
            catch (Exception ex)
            {
                throw ex; // TODO: Do something useful with the exceptions.
            }
        }

        public static void SaveNetworkDiscretisation(IDiscretization networkDiscretization, string netFilePath)
        {
            try
            {
                using (var uGrid1DMesh = new UGrid1DMesh(netFilePath))
                {
                    var discretisationPoints = networkDiscretization.Locations.Values.ToArray();

                    var numberOfMeshEdges = discretisationPoints.Length - networkDiscretization.Network.Nodes.Count + networkDiscretization.Network.Branches.Count;

                    if (!networkDiscretization.Network.Attributes.ContainsKey(IO_NETCDF_NETWORK_ID))
                    {
                        throw new InvalidOperationException("Couldn't retrieve network Id, can't save the network discretisation");
                    };
                    int networkId = (int)networkDiscretization.Network.Attributes[IO_NETCDF_NETWORK_ID];
                    
                    int[] branchIdx = discretisationPoints.Select(l => l.Branch)
                        .ToArray()
                        .Select(b => networkDiscretization.Network.Branches.IndexOf(b))
                        .ToArray();

                    double[] offset = discretisationPoints.Select(l => l.Chainage).ToArray();

                    uGrid1DMesh.Create1DMeshInFile(networkDiscretization.Name, discretisationPoints.Length, numberOfMeshEdges, networkId);
                    uGrid1DMesh.Write1DMeshDiscretizationPoints(branchIdx, offset);

                }
            }
            catch (Exception ex)
            {
                throw ex; // TODO: Do something useful with the exceptions.
            }
        }

        public static IHydroNetwork LoadNetwork(string netFilePath)
        {
            try
            {
                using (var uGrid1D = new UGrid1D(netFilePath))
                {
                    // Open the file to load the network. There can be multiple networks stored in the NetCDF file

                    uGrid1D.Initialize();

                    int numberOfNetworks = uGrid1D.GetNumberOfNetworks();

                    if (numberOfNetworks < 1)
                    {
                        return null; // throw new Exception(string.Format("No network is stored in netCFD file located at: {0}", netFilePath)); // TODO: Should this throw?
                    }

                    var networkIds = uGrid1D.GetNetworkIds();

                    // For now only use the first network id in the array
                    int networkId = networkIds[0];

                    uGrid1D.InitializeForLoading(networkId);

                    double[] nodesX;
                    double[] nodesY;
                    string[] nodesNames;
                    string[] nodesDescriptions;
                    uGrid1D.Read1DNetworkNodes(out nodesX, out nodesY, out nodesNames, out nodesDescriptions);

                    int[] sourceNodes;
                    int[] targetNodes;
                    double[] branchLengths;
                    int[] branchGeometryPoints;
                    string[] branchNames;
                    string[] branchDescriptions;
                    uGrid1D.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeometryPoints, out branchNames, out branchDescriptions);

                    double[] geometryPointsX;
                    double[] geometryPointsY;
                    uGrid1D.Read1DNetworkGeometry(out geometryPointsX, out geometryPointsY);

                    //var networkName = uGrid1D.GetNetworkName(networkId);
                    var coordinateSystem = uGrid1D.CoordinateSystem;

                    var interObj = new IntermediateObject
                    {
                        NodesX = nodesX,
                        NodesY = nodesY,
                        NodesNames = nodesNames,
                        NodesDescriptions = nodesDescriptions,
                        SourceNodeIds = sourceNodes,
                        TargedNodesIds = targetNodes,
                        BranchLengths = branchLengths,
                        NumberOfBranchGeometryPoints = branchGeometryPoints,
                        BranchNames = branchNames,
                        BranchDescriptions = branchDescriptions,
                        GeopointsX = geometryPointsX,
                        GeopointsY = geometryPointsY,
                        Name = "DummyNetworkName",
                        CoordinateSystem = coordinateSystem 
                    };

                    var network = ReconstructHydroNetwork(interObj);
                    return network;
                }
            }
            catch (Exception ex)
            {
                return null; // TODO: Do something useful with the exceptions.
            }
        }

        public static IDiscretization LoadNetworkDiscretisation(string netFilePath, IHydroNetwork network)
        {
            try
            {
                using (var uGrid1DMesh = new UGrid1DMesh(netFilePath))
                {
                    var discretisation = new Discretization
                    {
                        Name = "DummyDiscretisationName", // TODO: Obtain the discretisation name from the netCdf file?
                        Network = network,
                    };

                    var numberOfDiscretisationPoints = uGrid1DMesh.GetNumberOf1DMeshDiscretisationPoints();

                    int[] branchIdx;
                    double[] offset;
                    uGrid1DMesh.Read1DMeshDiscretisationPoints(out branchIdx, out offset);

                    return discretisation;
                }
                
            }
            catch (Exception ex)
            {
                return null; // TODO: Do Something useful with the exceptions.?
            }

        }

        public static IHydroNetwork ReconstructHydroNetwork(IntermediateObject toNetwork)
        {
            var network = new HydroNetwork
            {
                Name = toNetwork.Name,
                CoordinateSystem = toNetwork.CoordinateSystem
            };

            var nodes = ConstructHydroNodes(network, toNetwork.NodesX, toNetwork.NodesY, toNetwork.NodesNames, toNetwork.NodesDescriptions);

            var branches = ConstructNetworkBranches(network, nodes, toNetwork.SourceNodeIds, toNetwork.TargedNodesIds,
                toNetwork.BranchLengths,
                toNetwork.NumberOfBranchGeometryPoints, toNetwork.BranchNames, toNetwork.BranchDescriptions,
                toNetwork.GeopointsX, toNetwork.GeopointsY);

            network.Nodes.AddRange(nodes);
            network.Branches.AddRange(branches);

            return network;
        }

        public static List<IHydroNode> ConstructHydroNodes(IHydroNetwork network, double[] nodesX, double[] nodesY, string[] nodesNames, string[] nodesDescriptions)
        {
            var nodes = new List<IHydroNode>();

            int numberOfNodes = nodesX.Length;
            if (numberOfNodes <= 0)
            {
                return null;
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
                    Name = nodesNames[i],
                    Description = nodesDescriptions[i],
                    Geometry = new Point(nodesX[i], nodesY[i])
                };
                nodes.Add(node);
            }

            return nodes;
        }

        public static List<IBranch> ConstructNetworkBranches(INetwork parentNetwork, List<IHydroNode> nodes, int[] sourceNodes, int[] targetNodes,
            double[] branchLengths, int[] branchGeometryPoints, string[] branchNames, string[] branchDescriptions, double[] geometryPointsX, double[] geometryPointsY)
        {
            var branches = new List<IBranch>();
            int numberOfBranches = sourceNodes.Length;

            if (numberOfBranches <= 0)
            {
                return null;
            }

            if (numberOfBranches != targetNodes.Length
                || numberOfBranches != branchLengths.Length
                || numberOfBranches != branchNames.Length
                || numberOfBranches != branchGeometryPoints.Length
                || numberOfBranches != branchDescriptions.Length)
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
            for (int i = 0; i < numberOfBranches; ++i)
            {
                int sourceNodeIndex = sourceNodes[i];
                int targetNodeIndex = targetNodes[i];
                int numberOfBranchGeometryPoints = branchGeometryPoints[i];

                Coordinate[] coordinates = geometryCoordinates.Skip(geoPointsIndex).Take(numberOfBranchGeometryPoints).ToArray();
                geoPointsIndex += numberOfBranchGeometryPoints;

                var branch = new Branch
                {
                    Network = parentNetwork,
                    Name = branchNames[i],
                    Description = branchDescriptions[i],
                    Source = nodes[sourceNodeIndex],
                    Target = nodes[targetNodeIndex],
                    Geometry = new LineString(coordinates)
                };

                // check if branchLength is equal to something? Is this useful?
                if (Math.Abs(branch.Length - branchLengths[i]) > 0.01 * branch.Length)
                {
                    throw new Exception(string.Format("Reconstruction of branch length failed. More difference than expected?"));
                }

                branches.Add(branch);
            }

            return branches;
        }
    }
}
