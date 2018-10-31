using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class UGridToNetworkAdapter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UGridToNetworkAdapter));

        public const string BranchGuiFileName = "branches.gui";

        private static int GetNetworkId(Func<int[], int> func, UGridNetwork uGridNetwork)
        {
            var networkIds = uGridNetwork.GetNetworkIds();
            var networkId = func(networkIds);
            return networkId;
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

        public static NetworkDiscretisationUGridDataModel LoadNetworkDiscretisationDataModel(string netFilePath)
        {
            try
            {
                using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath))
                {
                    var numberOfNetworkDiscretisations = uGridNetworkDiscretisation.GetNumberOfNetworkDiscretisations();
                    if (!uGridNetworkDiscretisation.HasUgridDataSetConvention() || numberOfNetworkDiscretisations < 1)
                    {
                        return new NetworkDiscretisationUGridDataModel(new Discretization());
                    }

                    var meshIds = uGridNetworkDiscretisation.GetNetworkDiscretisationIds(numberOfNetworkDiscretisations);
                    var meshId = meshIds[0];

                    int[] branchIndices;
                    double[] offset;
                    string[] ids;
                    string[] names;
                    uGridNetworkDiscretisation.ReadNetworkDiscretisationPointsForMeshId(meshId, out branchIndices, out offset, out ids, out names);

                    var networkId = uGridNetworkDiscretisation.GetNetworkIdForMeshId(meshId);
                    var meshDiscretisationName = uGridNetworkDiscretisation.GetNetworkDiscretisationNameForMeshId(meshId);
                    return new NetworkDiscretisationUGridDataModel(meshDiscretisationName, branchIndices, offset, networkId, ids, names);
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null; 
            }
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
                    var propertiesPerBranch = ReadPropertiesPerBranchFromFile(netFilePath);

                    var networkUGridDataModel = ImportNetworkFromUgridAndCreateNetworkDataModel(uGridNetwork, func);
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

        private static IList<BranchFile.BranchProperties> ReadPropertiesPerBranchFromFile(string netFilePath)
        {
            var brancheTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, BranchGuiFileName);
            var propertiesPerBranch = File.Exists(brancheTypeFilePath) ? BranchFile.Read(brancheTypeFilePath) : null;
            return propertiesPerBranch;
        }

        public static NetworkUGridDataModel ReadNetworkDataModelFromUGrid(string netFilePath)
        {
            Func<int[], int> getFirstNetworkId = networkIds => networkIds[0];

            return ReadNetworkDataModelFromUGrid(netFilePath, getFirstNetworkId);
        }

        private static NetworkUGridDataModel ReadNetworkDataModelFromUGrid(string netFilePath, Func<int[], int> func)
        {
            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath))
                {
                    return ImportNetworkFromUgridAndCreateNetworkDataModel(uGridNetwork, func);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }

        private static NetworkUGridDataModel ImportNetworkFromUgridAndCreateNetworkDataModel(UGridNetwork uGridNetwork, Func<int[], int> func)
        {
            uGridNetwork.Initialize();

            // There can be multiple networks stored in the NetCDF file
            var numberOfNetworks = uGridNetwork.GetNumberOfNetworks();
            if (!uGridNetwork.HasUgridDataSetConvention() || numberOfNetworks < 1)
            {
                return new NetworkUGridDataModel(new HydroNetwork());
            }

            var networkId = GetNetworkId(func, uGridNetwork);
            return LoadNetworkUGridDataModel(uGridNetwork, networkId);
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
    }
}
