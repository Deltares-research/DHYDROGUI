using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.NetworkEditor
{
    public static class UGridToNetworkAdapter
    {
        private const string IO_NETCDF_NETWORK_ID = "IoNetCdfNetworkId";
        
        public static void SaveNetwork(IHydroNetwork network, string netFilePath, UGridGlobalMetaData metaData)
        {
            try
            {
                using (var uGrid1D = new UGrid1D(netFilePath, metaData))
                {
                    uGrid1D.CreateFile();

                    var totalNumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                    int networkId;
                    uGrid1D.Create1DGridInFile(
                        network.Name,
                        network.Nodes.Count,
                        network.Branches.Count,
                        totalNumberOfGeometryPoints,
                        out networkId);

                    network.Attributes = new DictionaryFeatureAttributeCollection
                    {
                        {IO_NETCDF_NETWORK_ID, networkId}
                    };

                    uGrid1D.Write1DNetworkNodes(
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray(),
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray(),
                        network.Nodes.Select(n => n.Name).ToArray(),
                        network.Nodes.Select(n => n.Description).ToArray()
                    );

                    uGrid1D.Write1DNetworkBranches(
                        network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Length).ToArray(),
                        network.Branches.Select(b =>
                            {
                                if (b.Geometry != null && b.Geometry.Coordinates != null)
                                {
                                    return b.Geometry.Coordinates.Length;
                                }
                                return 0;
                            }
                        ).ToArray(),
                        network.Branches.Select(b => b.Name).ToArray(),
                        network.Branches.Select(b => b.Description).ToArray()
                    );
                    uGrid1D.Write1DNetworkGeometry(
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray(),
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray()
                    );

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
                    var numberOfNodes = uGrid1D.GetNumberOfNetworkNodes();

                    var numberOfBranches = uGrid1D.GetNumberOfNetworkBranches();

                    var numberOfGeometryPoints = uGrid1D.GetNumberOfNetworkGeometryPoints();

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

                    //uGrid1D.CoordinateSystem.AuthorityCode
                    int epsgCode = uGrid1D.GridApi.GetCoordinateSystemCode(); // TODO: Does the GridApi have to be exposed here?
                    var coordinateSystem = epsgCode > 0 ? new OgrCoordinateSystemFactory().CreateFromEPSG(epsgCode) : null;

                    var network = new HydroNetwork()
                    {
                        Name = "DummyNetworkName", // TODO: Network name should be extracted from the netcdf file
                        CoordinateSystem = coordinateSystem,
                    };

                    // Reconstruct list of hydronodes (x, y coordinates; name; description)
                    var nodes = ConstructHydroNodes(network, nodesX, nodesY, nodesNames, nodesDescriptions);

                    // Reconstruct list of branches from the given data.
                    var branches = ConstructNetworkBranches(network, nodes, sourceNodes, targetNodes, branchLengths, branchGeometryPoints, branchNames, branchDescriptions, geometryPointsX, geometryPointsY);

                    // validate number of constructed nodes and branches
                    if (numberOfNodes != nodes.Count)
                    {
                        throw new InvalidOperationException(string.Format("Loading the network nodes from the netcfd file failed. Expected {0} nodes, actually {1} nodes", numberOfNodes, nodes.Count));
                    }
                    if (numberOfBranches != branches.Count)
                    {
                        throw new InvalidOperationException(string.Format("Loading the network branches from the netcfd file failed. Expected {0} branches, actually {1} branches", numberOfBranches, branches.Count));
                    }

                    var totalConstructedNumberOfGeometryPoints = branches.Sum(b => b.Geometry.Coordinates.Length);
                    if (numberOfGeometryPoints!= totalConstructedNumberOfGeometryPoints)
                    {
                        throw new InvalidOperationException(string.Format("Loading the network branch geometry points from the netcfd file failed. Expected {0} geometry points, actually {1} geometry points", numberOfGeometryPoints, totalConstructedNumberOfGeometryPoints));
                    }

                    network.Nodes.AddRange(nodes);
                    network.Branches.AddRange(branches);

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
