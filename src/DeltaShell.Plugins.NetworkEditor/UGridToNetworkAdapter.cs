using System;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;

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
                using (var uGrid1D = new UGrid1D(netFilePath, metaData, GridApiDataSet.NetcdfOpenMode.nf90_write))
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
                    
                    if (uGrid1D.GetDataSetConvention() != GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
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

                    var networkName = "Network";
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
            var discretisationDataModel = new NetworkDiscretisationUGridDataModel(networkDiscretization);
            try
            {
                using (var uGrid1DMesh = new UGrid1DDiscretisation(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGrid1DMesh.Initialize();

                    /* PLEASE NOTE:
                     * The network discretisation must be coupled to a network, therefore the network ID is required.
                     * At this moment only one network will be stored in a netCDF file and hence it is save to obtain the ALL the network IDs 
                     * and pick the first in the list as the network to couple to.
                     * However, when more networks will be stored this method is no longer valid and another method to obtain the correct 
                     * network ID must be implemented.
                     */
                    var networkIds = uGrid1DMesh.GetNetworkIds();
                    var networkId = networkIds[0]; //TODO: Obtain the network ID to couple the mesh to. Maybe by name? In case there is 1 network it won't be a problem. But what if there is also a mesh?
                    
                    uGrid1DMesh.Create1DMeshInFile(discretisationDataModel.Name, discretisationDataModel.NumberOfDiscretisationPoints, discretisationDataModel.NumberOfMeshEdges, networkId);
                    uGrid1DMesh.Write1DMeshDiscretizationPoints(discretisationDataModel.BranchIdx, discretisationDataModel.Offset);
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
                    uGrid1DMesh.Initialize();
                    
                    if (uGrid1DMesh.GetDataSetConvention() != GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
                    {
                        return null;
                    }

                    var numberOfMeshDiscretisations = uGrid1DMesh.GetNumberOf1DDiscretisations();

                    if (numberOfMeshDiscretisations < 1)
                    {
                        return null;
                    }

                    var meshIds = uGrid1DMesh.Get1DMeshDiscretisationIds(numberOfMeshDiscretisations);

                    // only one 1D discretisation mesh is supported, use the first id in the array
                    var meshId = meshIds[0];

                    uGrid1DMesh.InitializeForLoading(meshId);

                    var meshDiscretisationName = uGrid1DMesh.Get1DMeshDiscretisationName(meshId);

                    int[] branchIndices;
                    double[] offset;
                    uGrid1DMesh.Read1DMeshDiscretisationPoints(meshId, out branchIndices, out offset);
                    
                    var networkDiscretisation = NetworkDiscretisationUGridDataModel.ReconstructNetworkDiscretisation(network, meshDiscretisationName, branchIndices, offset);

                    return networkDiscretisation;
                }
                
            }
            catch (Exception ex)
            {
                return null; // TODO: Do Something useful with the exceptions.?
            }
        }
    }
}
