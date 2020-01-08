using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class UGridToNetworkAdapter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UGridToNetworkAdapter));

        public const string BranchGuiFileName = "branches.gui";
        public const string StorageNodeFileName = "nodeFile.ini";

        /// <summary>
        /// Always returns a new <see cref="IDiscretization"/> (containing Network) filled with the
        /// information from the <paramref name="netFilePath"/>
        /// </summary>
        /// <param name="netFilePath">Path to the net file</param>
        /// <param name="discretization"></param>
        /// <param name="network"></param>
        /// <param name="nodeData">Data about the nodes (nodeFile.ini) not contained in the net file (manhole <-> compartment mapping etc.)</param>
        public static void LoadNetworkAndDiscretisation(string netFilePath, IDiscretization discretization, IHydroNetwork network, IList<NodeFile.CompartmentProperties> nodeData, IList<BranchFile.BranchProperties> branchData)
        {
            var discretisationDataModel = LoadNetworkDiscretisationDataModel(netFilePath);
            if (discretisationDataModel == null)
            {
                // no discretisation is found. Return null and try to get the network in another call
                discretization?.Clear();
                return;
            }

            FillNetworkById(netFilePath, network, discretisationDataModel.NetworkId, nodeData, branchData);
            if(network != null)
            {
                NetworkDiscretisationFactory.FillNetworkDiscretisation(discretization, network, discretisationDataModel);
            }
            else
            {
                discretization?.Clear();
            }
            
        }

        // only used in tests => todo make private
        public static NetworkUGridDataModel ReadNetworkDataModelFromUGrid(string netFilePath)
        {
            Func<int[], int> getFirstNetworkId = networkIds => networkIds.Any() ? networkIds[0] : 0;

            return ReadNetworkDataModelFromUGrid(netFilePath, getFirstNetworkId);
        }

        public static void SaveGrid(string netFilePath, GridUGridDataModel gridDataModel, UGridGlobalMetaData metaData, ICoordinateSystem coordinateSystem)
        {
            try
            {
                using (var uGridGrid = new UGridMesh2D(netFilePath, metaData, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridGrid.CreateFile();
                    uGridGrid.Initialize();
                    uGridGrid.CreateGridInFile(gridDataModel.Dimensions, gridDataModel.Data);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
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

                    uGridNetwork.DefineBranchTypeValuesNetworkId(networkId);
                }

                WriteBranchTypeValues(netFilePath, networkDataModel.BranchTypes);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        private static void WriteBranchTypeValues(string netFilePath, NetworkUGridDataModel.BranchType[] branchTypes)
        {
            var ncFile = NetCdfFile.OpenExisting(netFilePath, true);
            var var = ncFile.GetVariableByName($"{GridApiDataSet.DataSetNames.Network}_{GridApiDataSet.UGridApiConstants.BranchType}");
            if (var != null)
            {
                ncFile.ReDefine();
                //ncFile.AddAttribute(var, new NetCdfAttribute("valid_range", (object) new int[] {1, 4}) );
                //ncFile.AddAttribute(var, new NetCdfAttribute("flag_value", (object)new int[] { 1, 2, 3, 4 }));
                ncFile.AddAttribute(var, new NetCdfAttribute("flag_meaning", "dry_weather_flow storm_water_flow mixed_flow surface_water transport_water"));
                ncFile.EndDefine();
                ncFile.Write(var, branchTypes.Cast<int>().ToArray());
                ncFile.Flush();
            }
            ncFile.Close();
        }

        public static void SaveNetworkDiscretisation(string netFilePath, NetworkDiscretisationUGridDataModel discretisationDataModel)
        {
            try
            {
                using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath, GridApiDataSet.NetcdfOpenMode.nf90_write))
                {
                    uGridNetworkDiscretisation.Initialize();
                    uGridNetworkDiscretisation.CreateNetworkDiscretisationInFile(discretisationDataModel.NumberOfDiscretisationPoints, discretisationDataModel.NumberOfMeshEdges);
                    uGridNetworkDiscretisation.WriteNetworkDiscretisationPoints(discretisationDataModel.BranchIdx, discretisationDataModel.Offsets, discretisationDataModel.DiscretisationPointsX, discretisationDataModel.DiscretisationPointsY, discretisationDataModel.EdgeIdx,discretisationDataModel.EdgeChainage, discretisationDataModel.EdgePointsX, discretisationDataModel.EdgePointsY, discretisationDataModel.EdgeNodes, discretisationDataModel.DiscretisationPointIds, discretisationDataModel.DiscretisationPointDescriptions);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private static int GetNetworkId(Func<int[], int> func, UGridNetwork uGridNetwork)
        {
            var networkIds = uGridNetwork.GetNetworkIds();
            var networkId = func(networkIds);
            return networkId;
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
                    double[] discretisationPointsX;
                    double[] discretisationPointsY;
                    string[] ids;
                    string[] names;
                    uGridNetworkDiscretisation.ReadNetworkDiscretisationPointsForMeshId(meshId, out branchIndices, out offset, out discretisationPointsX, out discretisationPointsY, out ids, out names);

                    var networkId = uGridNetworkDiscretisation.GetNetworkIdForMeshId(meshId);
                    var meshDiscretisationName = uGridNetworkDiscretisation.GetNetworkDiscretisationNameForMeshId(meshId);
                    return new NetworkDiscretisationUGridDataModel(meshDiscretisationName, branchIndices, offset, discretisationPointsX, discretisationPointsY, networkId, ids, names);
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null; 
            }
        }

        private static IHydroNetwork LoadNetworkById(string netFilePath, int networkId, ICollection<NodeFile.CompartmentProperties> compartmentProperties, ICollection<BranchFile.BranchProperties> branchData)
        {
            Func<int[], int> func = networkIds =>
            {
                if (networkId > 0 && !networkIds.Contains(networkId))
                {
                    throw new Exception("The provided network ID is not present in the NetCDF file, can't load the network.");
                }
                return networkId == 0 && networkIds.Any() ? networkIds[0] : networkId;

            };

            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath))
                {
                    

                    var networkUGridDataModel = ImportNetworkFromUgridAndCreateNetworkDataModel(uGridNetwork, func);
                    return NetworkDiscretisationFactory.CreateHydroNetwork(networkUGridDataModel, branchData, compartmentProperties);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return null;
            }
        }
        private static void FillNetworkById(string netFilePath, IHydroNetwork network, int networkId, ICollection<NodeFile.CompartmentProperties> compartmentProperties, ICollection<BranchFile.BranchProperties> branchData)
        {
            Func<int[], int> func = networkIds =>
            {
                if (networkId > 0 && !networkIds.Contains(networkId))
                {
                    throw new Exception("The provided network ID is not present in the NetCDF file, can't load the network.");
                }
                return networkId == 0 && networkIds.Any() ? networkIds[0] : networkId;

            };

            try
            {
                using (var uGridNetwork = new UGridNetwork(netFilePath))
                {
                    

                    var networkUGridDataModel = ImportNetworkFromUgridAndCreateNetworkDataModel(uGridNetwork, func);
                    NetworkDiscretisationFactory.FillHydroNetwork(network, networkUGridDataModel, branchData, compartmentProperties);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public static IList<BranchFile.BranchProperties> ReadPropertiesPerBranchFromFile(string netFilePath)
        {
            var brancheTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, BranchGuiFileName);
            var propertiesPerBranch = File.Exists(brancheTypeFilePath) ? BranchFile.Read(brancheTypeFilePath, netFilePath) : null;
            return propertiesPerBranch;
        }

        public static IList<NodeFile.CompartmentProperties> ReadPropertiesPerNodeFromFile(string netFilePath)
        {
            var nodeTypeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(netFilePath, StorageNodeFileName);
            var propertiesPerNode = File.Exists(nodeTypeFilePath) ? NodeFile.Read(nodeTypeFilePath) : null;
            return propertiesPerNode;
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
            int numberOfNetworks =0;
            try
            {
                numberOfNetworks = uGridNetwork.GetNumberOfNetworks();
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                return new NetworkUGridDataModel(new HydroNetwork());
            }
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

        private static void SetCoordinateSystem(string netFilePath, ICoordinateSystem coordinateSystem)
        {
            var file = NetCdfFile.OpenExisting(netFilePath, true);
            try
            {
                file.ReDefine();
                var varname = coordinateSystem.IsGeographic ? "wgs84" : "projected_coordinate_system";
                var ncVariable = file.GetVariableByName(varname) ?? file.AddVariable(varname, NetCdfDataType.NcInteger, new NetCdfDimension[0]);

                file.AddAttribute(ncVariable, new NetCdfAttribute("name", (object) coordinateSystem.Name));
                file.AddAttribute(ncVariable, new NetCdfAttribute("epsg", (object) (int) coordinateSystem.AuthorityCode));
                file.AddAttribute(ncVariable, new NetCdfAttribute("grid_mapping_name", coordinateSystem.IsGeographic ? (object) "latitude_longitude" : (object) "Unknown projected"));
                file.AddAttribute(ncVariable, new NetCdfAttribute("longitude_of_prime_meridian", (object) 0.0));
                file.AddAttribute(ncVariable, new NetCdfAttribute("semi_major_axis", (object) coordinateSystem.GetSemiMajor()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("semi_minor_axis", (object) coordinateSystem.GetSemiMinor()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("inverse_flattening", (object) coordinateSystem.GetInverseFlattening()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("proj4_params", (object) coordinateSystem.PROJ4));
                file.AddAttribute(ncVariable, new NetCdfAttribute("EPSG_code", (object) string.Format("EPSG:{0}", (object) coordinateSystem.AuthorityCode)));
                file.AddAttribute(ncVariable, new NetCdfAttribute("projection_name", (object) "unknown"));
                file.AddAttribute(ncVariable, new NetCdfAttribute("wkt", (object) coordinateSystem.WKT));
                file.EndDefine();
                file.Flush();
            }
            finally
            {
                file.Close();
            }
        }
    }
}
