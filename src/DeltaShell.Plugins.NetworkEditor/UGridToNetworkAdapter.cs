using System;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Features;

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

                    if (network.Attributes == null)
                    {
                        network.Attributes = new DictionaryFeatureAttributeCollection();
                    }
                    if (network.Attributes.Keys.Contains(IO_NETCDF_NETWORK_ID))
                    {
                        network.Attributes[IO_NETCDF_NETWORK_ID] = networkId;
                    }
                    else
                    {
                        network.Attributes.Add(IO_NETCDF_NETWORK_ID, networkId);
                    }

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

        private static IHydroNetwork LoadNetwork(string netFilePath, Func<int[], int> func)
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
                        return null; 
                    }

                    var networkIds = uGridNetwork.GetNetworkIds();

                    int networkId = func(networkIds);

                    var networkUGridDataModel = LoadNetworkUGridDataModel(uGridNetwork, networkId);

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

        public static IHydroNetwork LoadNetwork(string netFilePath)
        {
            Func<int[], int> func = networkIds => networkIds[0];

            return LoadNetwork(netFilePath, func);
        }

        private static IHydroNetwork LoadNetworkById(string netFilePath, int networkId) 
        {
            Func<int[], int> func = networkIds =>
            {
                if (!networkIds.Contains(networkId))
                {
                    throw new Exception("The provided network ID is not present in the NetCDF file, can't load the network.");
                }
                return networkId;
            };

            return LoadNetwork(netFilePath, func);
        }
        
        private static NetworkUGridDataModel LoadNetworkUGridDataModel(UGridNetwork uGridNetwork, int networkId)
        {
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
            uGridNetwork.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths,
                out branchGeometryPoints, out branchNames, out branchDescriptions);

            double[] geometryPointsX;
            double[] geometryPointsY;
            uGridNetwork.ReadNetworkGeometry(networkId, out geometryPointsX, out geometryPointsY);

            var networkName = uGridNetwork.GetNetworkName(networkId);
            var coordinateSystem = uGridNetwork.CoordinateSystem;

            var networkUGridDataModel = new NetworkUGridDataModel(networkName, coordinateSystem, nodesX, nodesY, nodesNames,
                nodesDescriptions, sourceNodes, targetNodes, branchLengths, branchGeometryPoints, branchNames,
                branchDescriptions, geometryPointsX, geometryPointsY);
            return networkUGridDataModel;
        }

        public static void SaveNetworkDiscretisation(IDiscretization networkDiscretization, string netFilePath)
        {
            var discretisationDataModel = new NetworkDiscretisationUGridDataModel(networkDiscretization);
            try
            {
                using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridNetworkDiscretisation.Initialize();

                    /* PLEASE NOTE (DELFT3DFM-879):
                     * The network discretisation must be coupled to a network, therefore the network ID is required.
                     * At this moment only one network will be stored in a netCDF file and hence it is save to obtain the ALL the network IDs 
                     * and pick the first in the list as the network to couple to.
                     * However, when more networks will be stored this method is no longer valid and another method to obtain the correct 
                     * network ID must be implemented.
                     */

                    var networkIds = uGridNetworkDiscretisation.GetNetworkIds();
                    var networkId =  GetNetworkId(networkIds);
                    
                    uGridNetworkDiscretisation.CreateNetworkDiscretisationInFile(discretisationDataModel.Name, discretisationDataModel.NumberOfDiscretisationPoints, discretisationDataModel.NumberOfMeshEdges, networkId);
                    uGridNetworkDiscretisation.WriteNetworkDiscretisationPoints(discretisationDataModel.BranchIdx, discretisationDataModel.Offset);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        
        private static NetworkDiscretisationUGridDataModel LoadNetworkDiscretisationDataModel(string netFilePath)
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
                    var meshId = meshIds[0];

                    var networkId =  uGridNetworkDiscretisation.GetNetworkId(meshId);

                    uGridNetworkDiscretisation.InitializeForLoading(meshId);
                    var meshDiscretisationName = uGridNetworkDiscretisation.GetNetworkDiscretisationName(meshId);

                    int[] branchIndices;
                    double[] offset;
                    uGridNetworkDiscretisation.ReadNetworkDiscretisationPoints(meshId, out branchIndices, out offset);
                    
                    var networkDiscretisationDataModel = new NetworkDiscretisationUGridDataModel(meshDiscretisationName, branchIndices, offset, networkId);

                    return networkDiscretisationDataModel;
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null; 
            }
        }

        public static IDiscretization LoadNetworkAndDiscretisation(string netFilePath)
        {
            var discretisationDataModel = LoadNetworkDiscretisationDataModel(netFilePath);
            if (discretisationDataModel == null)
            {
                // no discretisation is found. Return null and try to get the network in another call
                return null;
            }

            var loadedNetwork = LoadNetworkById(netFilePath, discretisationDataModel.NetworkId);
            if (loadedNetwork == null)
            {
                return null;
            }
            
            var networkDiscretisation = NetworkDiscretisationUGridDataModel.ReconstructNetworkDiscretisation(loadedNetwork, discretisationDataModel.Name, discretisationDataModel.BranchIdx, discretisationDataModel.Offset);
            
            return networkDiscretisation;
        }

        private static int GetNetworkId(int[] networkIds)
        {
            if (networkIds.Length > 1)
                Log.Warn("Using more than one network in one mesh is currently not supported by DeltaShell. The first network stored in the NetCDF file will be returned.");
            return networkIds[0];
        }
    }
}
