using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor.IO;
using GeoAPI.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor
{
    public static class UGridToNetworkAdapter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UGridToNetworkAdapter));

        public const string BranchGuiFileName = "branches.gui";

        public static NetworkUGridDataModel ReadNetworkDataModelFromUGrid(string netFilePath)
        {
            Func<int[], int> func = networkIds => networkIds[0];

            return ReadNetworkDataModelFromUGrid(netFilePath, func);
        }

        private static NetworkUGridDataModel ReadNetworkDataModelFromUGrid(string netFilePath, Func<int[], int> func)
        {
            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath))
                {
                    // Open the file to load the network. There can be multiple networks stored in the NetCDF file
                    uGridNetwork.Initialize();

                    if (uGridNetwork.GetDataSetConvention() != GridApiDataSet.DataSetConventions.CONV_UGRID)
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

                    return LoadNetworkUGridDataModel(uGridNetwork, networkId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        private static NetworkUGridDataModel LoadNetworkUGridDataModel(UGridNetwork uGridNetwork, int networkId)
        {
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
            int[] branchOrderNumbers;

            uGridNetwork.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths,
                out branchGeometryPoints, out branchNames, out branchDescriptions, out branchOrderNumbers);

            double[] geometryPointsX;
            double[] geometryPointsY;
            uGridNetwork.ReadNetworkGeometry(networkId, out geometryPointsX, out geometryPointsY);

            var networkName = uGridNetwork.GetNetworkName(networkId);
            var coordinateSystem = uGridNetwork.CoordinateSystem;

            var networkUGridDataModel = new NetworkUGridDataModel(networkName, coordinateSystem, nodesX, nodesY, nodesNames,
                nodesDescriptions, sourceNodes, targetNodes, branchLengths, branchGeometryPoints, branchNames,
                branchDescriptions, geometryPointsX, geometryPointsY, branchOrderNumbers);
            return networkUGridDataModel;
        }

        public static NetworkDiscretisationUGridDataModel LoadNetworkDiscretisationDataModel(string netFilePath)
        {
            try
            {
                using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath))
                {
                    if (uGridNetworkDiscretisation.GetDataSetConvention() != GridApiDataSet.DataSetConventions.CONV_UGRID)
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
                    var networkId = uGridNetworkDiscretisation.GetNetworkIdForMeshId(meshId);
                    
                    var meshDiscretisationName = uGridNetworkDiscretisation.GetNetworkDiscretisationNameForMeshId(meshId);

                    int[] branchIndices;
                    double[] offset;
                    string[] ids;
                    string[] names;
                    uGridNetworkDiscretisation.ReadNetworkDiscretisationPointsForMeshId(meshId, out branchIndices, out offset, out ids, out names);
                    
                    var networkDiscretisationDataModel = new NetworkDiscretisationUGridDataModel(meshDiscretisationName, branchIndices, offset, networkId, ids, names);

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
            
            var networkDiscretisation = NetworkDiscretisationFactory.CreateNetworkDiscretisation(loadedNetwork, discretisationDataModel);
            
            return networkDiscretisation;
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

        private static IHydroNetwork LoadNetwork(string netFilePath, Func<int[], int> func)
        {
            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath))
                {
                    var brancheTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, BranchGuiFileName);
                    var propertiesPerBranch = File.Exists(brancheTypeFilePath) ? BranchFile.Read(brancheTypeFilePath) : null;

                    // Open the file to load the network. There can be multiple networks stored in the NetCDF file
                    uGridNetwork.Initialize();

                    if (uGridNetwork.GetDataSetConvention() != GridApiDataSet.DataSetConventions.CONV_UGRID)
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

                    var network = NetworkDiscretisationFactory.CreateHydroNetwork(networkUGridDataModel, propertiesPerBranch);
                    return network;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        public static void SaveNetwork(string netFilePath, NetworkUGridDataModel networkDataModel, UGridGlobalMetaData metaData)
        {
            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath, metaData, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridNetwork.CreateFile();
                    uGridNetwork.Initialize();

                    int networkId;
                    uGridNetwork.CreateNetworkInFile(
                        networkDataModel.NumberOfNodes,
                        networkDataModel.NumberOfBranches,
                        networkDataModel.NumberOfGeometryPoints,
                        out networkId);

                    networkDataModel.NetworkId = networkId;

                    uGridNetwork.WriteNetworkNodes(networkDataModel.NodesX, networkDataModel.NodesY,
                        networkDataModel.NodesNames, networkDataModel.NodesDescriptions);

                    uGridNetwork.WriteNetworkBranches(networkDataModel.SourceNodeIds,
                        networkDataModel.TargedNodesIds,
                        networkDataModel.BranchLengths,
                        networkDataModel.NumberOfGeometryPointsPerBranch,
                        networkDataModel.BranchNames,
                        networkDataModel.BranchDescriptions,
                        networkDataModel.BranchOrderNumbers);

                    uGridNetwork.WriteNetworkGeometry(networkDataModel.GeopointsX, networkDataModel.GeopointsY);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static void SaveNetworkDiscretisation(string netFilePath, NetworkDiscretisationUGridDataModel discretisationDataModel)
        {
            try
            {
                using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridNetworkDiscretisation.Initialize();
                    uGridNetworkDiscretisation.CreateNetworkDiscretisationInFile(discretisationDataModel.NumberOfDiscretisationPoints);
                    uGridNetworkDiscretisation.WriteNetworkDiscretisationPoints(discretisationDataModel.BranchIdx, discretisationDataModel.Offsets, discretisationDataModel.DiscretisationPointIds, discretisationDataModel.DiscretisationPointDescriptions);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static void CloseFile(string netFilePath)
        {
            try
            {
                using (var uGridNetworkDiscretisation =
                    new UGridNetworkDiscretisation(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridNetworkDiscretisation.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

    }
}
