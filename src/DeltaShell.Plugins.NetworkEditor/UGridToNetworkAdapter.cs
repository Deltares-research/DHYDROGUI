using System;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor
{
    public static class UGridToNetworkAdapter
    {
        private const string IO_NETCDF_NETWORK_ID = "IoNetCdfNetworkId";

        public static void SaveNetwork(IHydroNetwork network, string netFilePath, UGridGlobalMetaData metaData)
        {
            var networkUGridDataModel = new NetworkUGridDataModel(network);
            
            try
            {
                using (var uGrid1D = new UGrid1D(netFilePath, metaData))
                {
                    uGrid1D.CreateFile();
                    uGrid1D.Initialize();

                    int networkId;
                    
                    uGrid1D.Create1DNetworkInFile(networkUGridDataModel.Name,
                        networkUGridDataModel.NumberOfNodes,
                        networkUGridDataModel.NumberOfBranches,
                        networkUGridDataModel.NumberOfGeometryPoints,
                        out networkId);

                    networkUGridDataModel.NetworkId = networkId;

                    uGrid1D.Write1DNetworkNodes(networkUGridDataModel.NodesX, networkUGridDataModel.NodesY,
                        networkUGridDataModel.NodesNames, networkUGridDataModel.NodesDescriptions);

                    uGrid1D.Write1DNetworkBranches(networkUGridDataModel.SourceNodeIds,
                        networkUGridDataModel.TargedNodesIds,
                        networkUGridDataModel.BranchLengths,
                        networkUGridDataModel.NumberOfBranchGeometryPoints,
                        networkUGridDataModel.BranchNames,
                        networkUGridDataModel.BranchDescriptions);

                    uGrid1D.Write1DNetworkGeometry(networkUGridDataModel.GeopointsX, networkUGridDataModel.GeopointsY);
                }
            }
            catch (Exception ex)
            {
                throw ex; // TODO: Do something useful with the exceptions?
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
                    
                    if (!uGrid1D.IsUGridFormat())
                    {
                        return null;
                    }
                    
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
                    uGrid1D.Read1DNetworkNodes(networkId, out nodesX, out nodesY, out nodesNames, out nodesDescriptions);

                    int[] sourceNodes;
                    int[] targetNodes;
                    double[] branchLengths;
                    int[] branchGeometryPoints;
                    string[] branchNames;
                    string[] branchDescriptions;
                    uGrid1D.Read1DNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths, out branchGeometryPoints, out branchNames, out branchDescriptions);

                    double[] geometryPointsX;
                    double[] geometryPointsY;
                    uGrid1D.Read1DNetworkGeometry(networkId, out geometryPointsX, out geometryPointsY);

                    //var networkName = uGrid1D.GetNetworkName(networkId); // TODO: This doesn't work. Maybe because it is still based on the assumption that mesh and network are coupled?
                    // do we need a function ionc_get_network_name(ref int ioncid, ref int id, StringBuilder networkName)?

                    var networkName = "DummyNetworkName";
                    var coordinateSystem = uGrid1D.CoordinateSystem;
                    
                    var networkUGridDataModel = new NetworkUGridDataModel(networkName, coordinateSystem, nodesX, nodesY, nodesNames, nodesDescriptions, sourceNodes, targetNodes, branchLengths, branchGeometryPoints, branchNames, branchDescriptions, geometryPointsX, geometryPointsY);
                    var network = NetworkUGridDataModel.ReconstructHydroNetwork(networkUGridDataModel);
                    return network;
                }
            }
            catch (Exception ex)
            {
                return null; // TODO: Do something useful with the exceptions.
            }
        }

        public static void SaveNetworkDiscretisation(IDiscretization networkDiscretization, string netFilePath)
        {
            try
            {
                using (var uGrid1DMesh = new UGrid1DDiscretisation(netFilePath))
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

        public static IDiscretization LoadNetworkDiscretisation(string netFilePath, IHydroNetwork network)
        {
            try
            {
                using (var uGrid1DMesh = new UGrid1DDiscretisation(netFilePath))
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
    }
}
