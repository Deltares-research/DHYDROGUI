using System;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor
{
    public static class UGridToNetworkAdapter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UGridToNetworkAdapter));
        private const string IO_NETCDF_NETWORK_ID = "IoNetCdfNetworkId";

        public static void SaveNetwork(IHydroNetwork network, string netFilePath, UGridGlobalMetaData metaData)
        {
            var networkUGridDataModel = new NetworkUGridDataModel(network);
            
            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath, metaData, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridNetwork.CreateFile();
                    uGridNetwork.Initialize();

                    int networkId;
                    
                    uGridNetwork.CreateNetworkInFile(networkUGridDataModel.Name,
                        networkUGridDataModel.NumberOfNodes,
                        networkUGridDataModel.NumberOfBranches,
                        networkUGridDataModel.NumberOfGeometryPoints,
                        out networkId);

                    networkUGridDataModel.NetworkId = networkId;

                    uGridNetwork.WriteNetworkNodes(networkUGridDataModel.NodesX, networkUGridDataModel.NodesY,
                        networkUGridDataModel.NodesNames, networkUGridDataModel.NodesDescriptions);

                    uGridNetwork.WriteNetworkBranches(networkUGridDataModel.SourceNodeIds,
                        networkUGridDataModel.TargedNodesIds,
                        networkUGridDataModel.BranchLengths,
                        networkUGridDataModel.NumberOfBranchGeometryPoints,
                        networkUGridDataModel.BranchNames,
                        networkUGridDataModel.BranchDescriptions);

                    uGridNetwork.WriteNetworkGeometry(networkUGridDataModel.GeopointsX, networkUGridDataModel.GeopointsY);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static IHydroNetwork LoadNetwork(string netFilePath)
        {
            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath))
                {
                    // Open the file to load the network. There can be multiple networks stored in the NetCDF file
                    uGridNetwork.Initialize();
                    
                    if (uGridNetwork.GetDataSetConvention() != GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
                    {
                        return null;
                    }
                    
                    int numberOfNetworks = uGridNetwork.GetNumberOfNetworks();

                    if (numberOfNetworks < 1)
                    {
                        return null; // throw new Exception(string.Format("No network is stored in netCFD file located at: {0}", netFilePath)); // TODO: Should this throw?
                    }

                    var networkIds = uGridNetwork.GetNetworkIds();

                    // For now only use the first network id in the array
                    int networkId = networkIds[0];

                    uGridNetwork.InitializeForLoading(networkId);

                    double[] nodesX;
                    double[] nodesY;
                    string[] nodesNames;
                    string[] nodesDescriptions;
                    uGridNetwork.ReadNetworkNodes(networkId, out nodesX, out nodesY, out nodesNames, out nodesDescriptions);

                    int[] sourceNodes;
                    int[] targetNodes;
                    double[] branchLengths;
                    int[] branchGeometryPoints;
                    string[] branchNames;
                    string[] branchDescriptions;
                    uGridNetwork.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths, out branchGeometryPoints, out branchNames, out branchDescriptions);

                    double[] geometryPointsX;
                    double[] geometryPointsY;
                    uGridNetwork.ReadNetworkGeometry(networkId, out geometryPointsX, out geometryPointsY);

                    //var networkName = uGrid1D.GetNetworkName(networkId); // TODO: This doesn't work. Maybe because it is still based on the assumption that mesh and network are coupled?
                    // do we need a function ionc_get_network_name(ref int ioncid, ref int id, StringBuilder networkName)?

                    var networkName = "Network";
                    var coordinateSystem = uGridNetwork.CoordinateSystem;
                    
                    var networkUGridDataModel = new NetworkUGridDataModel(networkName, coordinateSystem, nodesX, nodesY, nodesNames, nodesDescriptions, sourceNodes, targetNodes, branchLengths, branchGeometryPoints, branchNames, branchDescriptions, geometryPointsX, geometryPointsY);
                    var network = NetworkUGridDataModel.ReconstructHydroNetwork(networkUGridDataModel);
                    return network;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        public static void SaveNetworkDiscretisation(IDiscretization networkDiscretization, string netFilePath)
        {
            var discretisationDataModel = new NetworkDiscretisationUGridDataModel(networkDiscretization);
            try
            {
                using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridNetworkDiscretisation.Initialize();

                    /* PLEASE NOTE:
                     * The network discretisation must be coupled to a network, therefore the network ID is required.
                     * At this moment only one network will be stored in a netCDF file and hence it is save to obtain the ALL the network IDs 
                     * and pick the first in the list as the network to couple to.
                     * However, when more networks will be stored this method is no longer valid and another method to obtain the correct 
                     * network ID must be implemented.
                     */
                    var networkIds = uGridNetworkDiscretisation.GetNetworkIds();
                    var networkId = networkIds[0]; //TODO: Obtain the network ID to couple the mesh to. Maybe by name? In case there is 1 network it won't be a problem. But what if there is also a mesh?
                    
                    uGridNetworkDiscretisation.CreateNetworkInFile(discretisationDataModel.Name, discretisationDataModel.NumberOfDiscretisationPoints, discretisationDataModel.NumberOfMeshEdges, networkId);
                    uGridNetworkDiscretisation.WriteNetworkDiscretisationPoints(discretisationDataModel.BranchIdx, discretisationDataModel.Offset);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static IDiscretization LoadNetworkDiscretisation(string netFilePath, IHydroNetwork network)
        {
            try
            {
                using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath))
                {
                    uGridNetworkDiscretisation.Initialize();
                    
                    if (uGridNetworkDiscretisation.GetDataSetConvention() != GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
                    {
                        return null;
                    }

                    var numberOfNetworkDiscretisations = uGridNetworkDiscretisation.GetNumberOfNetworkDiscretisations();

                    if (numberOfNetworkDiscretisations < 1)
                    {
                        return null;
                    }

                    var meshIds = uGridNetworkDiscretisation.GetNetworkDiscretisationIds(numberOfNetworkDiscretisations);

                    // only one 1D discretisation mesh is supported, use the first id in the array
                    var meshId = meshIds[0];

                    uGridNetworkDiscretisation.InitializeForLoading(meshId);

                    var meshDiscretisationName = uGridNetworkDiscretisation.GetNetworkDiscretisationName(meshId);

                    int[] branchIndices;
                    double[] offset;
                    uGridNetworkDiscretisation.ReadNetworkDiscretisationPoints(meshId, out branchIndices, out offset);
                    
                    var networkDiscretisation = NetworkDiscretisationUGridDataModel.ReconstructNetworkDiscretisation(network, meshDiscretisationName, branchIndices, offset);

                    return networkDiscretisation;
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }
    }
}
