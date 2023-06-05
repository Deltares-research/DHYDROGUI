using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// Helper for doing UGrid related read/write actions
    /// </summary>
    public static class UGridFileHelper
    {
        public const double DefaultNoDataValue = -999.0;
        private static readonly ILog Log = LogManager.GetLogger(typeof(UGridFileHelper));
        public const int IdsSize = 40;
        private static string lastCheckedUGridPath;
        private static bool lastCheckedUGridPathResult;
        private static Dictionary<UGridMeshType, int> numberOfMeshByType;
        private static int numberOfNetworks = -1;

        public enum BedLevelLocation
        {
            Faces = 1,
            CellEdges = 2,
            NodesMeanLev = 3,
            NodesMinLev = 4,
            NodesMaxLev = 5,
            FacesMeanLevFromNodes = 6
        }

        /// <summary>
        /// Reads the Z values of the first <see cref="UnstructuredGrid"/> (2d mesh) in the file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <returns>Z values of the first 2d mesh</returns>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        public static double[] ReadZValues(string path, BedLevelLocation location)
        {
            if (!IsValidPath(path))
            {
                return new double[0];
            }

            using (var api = CreateUGridApi())
            {
                api.Open(path);

                if (!api.IsUGridFile())
                {
                    Log.WarnFormat(Resources.UGridFileHelper_ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, path);
                    return new double[0];
                }

                var meshIds2d = api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                if(meshIds2d.Length == 0) 
                    return new double[0];

                var locationType = GetLocationType(location);
                if (locationType == GridLocationType.None) 
                    return new double[0];

                var variableName = GetVariableName(location);
                return api.GetVariableValues(variableName, meshIds2d[0], locationType);
            }
        }

        /// <summary>
        /// Reads the no data value used for the Z values of the first <see cref="UnstructuredGrid"/>
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <returns>No data value used for Z values of the first 2d mesh</returns>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        public static double GetZCoordinateNoDataValue(string path, BedLevelLocation location)
        {
            if (!IsValidPath(path))
            {
                return DefaultNoDataValue;
            }

            using (var api = CreateUGridApi())
            {
                api.Open(path);

                if (!api.IsUGridFile())
                    return DefaultNoDataValue;

                var meshIds2d = api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                if (meshIds2d.Length == 0)
                    return DefaultNoDataValue;

                var locationType = GetLocationType(location);
                if (locationType == GridLocationType.None)
                    return DefaultNoDataValue;

                var variableName = GetVariableName(location);
                return api.GetVariableNoDataValue(variableName, meshIds2d[0], locationType);
            }
        }

        /// <summary>
        /// Writes the Z values for the first <see cref="UnstructuredGrid"/> (2d mesh)
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <param name="values">Z values to write</param>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        public static void WriteZValues(string path, BedLevelLocation location, double[] values)
        {
            using (var api = CreateUGridApi())
            {
                api.Open(path, OpenMode.Appending);

                if (api.IsUGridFile())
                {
                    WriteZValuesWithApi(api, location, values, path);
                    return;
                }

            }
            NetFile.WriteZValues(path, values);
                    
        }

        /// <summary>
        /// Reads the coordinate system used by the UGrid file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <returns>The read coordinate system</returns>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        public static ICoordinateSystem ReadCoordinateSystem(string path)
        {
            if (!IsValidPath(path))
            {
                return null;
            }

            using (var api = CreateUGridApi())
            {
                api.Open(path);

                if (api.IsUGridFile())
                    return GetCoordinateSystemFromApi(api);
            }
            return NetFile.ReadCoordinateSystem(path);
        }

        /// <summary>
        /// Writes the <paramref name="coordinateSystem"/> to the UGrid file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="coordinateSystem">Coordinate system to write</param>.
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        public static void WriteCoordinateSystem(string path, ICoordinateSystem coordinateSystem)
        {
            if (!IsUGridFile(path))
            {
                NetFile.WriteCoordinateSystem(path, coordinateSystem);
                return;
            }

            // using old implementation because api.SetCoordinateSystem is not yet implemented

            var file = NetCdfFile.OpenExisting(path, true);
            try
            {
                file.ReDefine();
                var variableName = "projected_coordinate_system";
                var ncVariable = file.GetVariableByName(variableName) ?? file.AddVariable(variableName, NetCdfDataType.NcInteger, new NetCdfDimension[0]);

                file.AddAttribute(ncVariable, new NetCdfAttribute("name", coordinateSystem.Name));
                file.AddAttribute(ncVariable, new NetCdfAttribute("epsg", (int)coordinateSystem.AuthorityCode));
                file.AddAttribute(ncVariable, new NetCdfAttribute("grid_mapping_name", coordinateSystem.IsGeographic ? "latitude_longitude" : "Unknown projected"));
                file.AddAttribute(ncVariable, new NetCdfAttribute("longitude_of_prime_meridian", 0.0));
                file.AddAttribute(ncVariable, new NetCdfAttribute("semi_major_axis", coordinateSystem.GetSemiMajor()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("semi_minor_axis", coordinateSystem.GetSemiMinor()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("inverse_flattening", coordinateSystem.GetInverseFlattening()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("proj4_params", coordinateSystem.PROJ4));
                file.AddAttribute(ncVariable, new NetCdfAttribute("EPSG_code", string.Format("EPSG:{0}", coordinateSystem.AuthorityCode)));
                file.AddAttribute(ncVariable, new NetCdfAttribute("projection_name", "unknown"));
                file.AddAttribute(ncVariable, new NetCdfAttribute("wkt", coordinateSystem.WKT));
                file.EndDefine();
                file.Flush();

                // Note: Temporary solution - UGrid v2 will likely change the way the coordinate systems are written in the NetFile
                UpdateNodeZVariables(file);
            }
            finally
            {
                file.Close();
            }
        }

        /// <summary>
        /// Reads the first <see cref="UnstructuredGrid"/> in the UGrid file, or reads old NetFile
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="grid">Grid instance to fill.</param>
        /// <param name="loadFlowLinksAndCells">Also read flow links and cell information (Applies to NetFile only).
        ///     With a UGrid file the cell information is always read but not the flow links</param>
        /// <param name="recreateCells">Recreates the cell information by calling FindCells instead of reading it from file</param>
        /// <returns>The first <see cref="UnstructuredGrid"/> in the UGrid file</returns>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        public static void SetUnstructuredGrid(string path, UnstructuredGrid grid, bool loadFlowLinksAndCells = false, bool recreateCells = true)
        {
            var mesh2d = Read2DMesh(path);
            grid.ApplyMesh2D(path, mesh2d, loadFlowLinksAndCells, recreateCells);
        }

        private static Disposable2DMeshGeometry Read2DMesh(string path)
        {
            if (!IsValidPath(path))
            {
                Log.WarnFormat("Could not find grid file at \"{0}\", this is because you maybe just created this model. If this is not the case please check if the file with" +
                               "the grid in it exists.", path);
                return null;
            }

            using (var api = CreateUGridApi())
            {
                api.Open(path);

                if (api.IsUGridFile())
                {
                    var meshIds2d = api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                    if (meshIds2d.Length == 0)
                    {
                        return null;
                    }

                    return api.GetMesh2D(meshIds2d[0]);
                }
            }

            return null;
        }

        /// <summary>
        /// Reads the first <see cref="UnstructuredGrid"/> in the UGrid file, or reads old NetFile
        /// </summary>
        /// <param name="grid">Grid instance to fill.</param>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="mesh2d">The 2d Mesh read from the file</param>
        /// <param name="loadFlowLinksAndCells">Also read flow links and cell information (Applies to NetFile only).
        ///     With a UGrid file the cell information is always read but not the flow links</param>
        /// <param name="recreateCells">Recreates the cell information by calling FindCells instead of reading it from file</param>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        private static void ApplyMesh2D(this UnstructuredGrid grid, string path, Disposable2DMeshGeometry mesh2d, bool loadFlowLinksAndCells = false, bool recreateCells = true)
        {
            if (!IsValidPath(path))
            {
                Log.WarnFormat("Could not find grid file at \"{0}\", this is because you maybe just created this model. If this is not the case please check if the file with" +
                               "the grid in it exists.", path);
                return;
            }

            using (var api = CreateUGridApi())
            {
                api.Open(path);
                if (mesh2d != null && api.IsUGridFile())
                {
                    var unstructuredGrid = mesh2d.CreateUnstructuredGrid(recreateCells);
                    if (unstructuredGrid == null)
                    {
                        return;
                    }
                    
                    if (grid == null)
                    {
                        grid = new UnstructuredGrid();
                    }

                    grid.ResetState(unstructuredGrid.Vertices, unstructuredGrid.Edges, unstructuredGrid.Cells, unstructuredGrid.FlowLinks);
                    grid.CoordinateSystem = GetCoordinateSystemFromApi(api);
                    return;
                }
            }

            Log.WarnFormat(Resources.UGridFileHelper_ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, path);
            ApplyLegacyGrid(path, grid, loadFlowLinksAndCells);
        }

        private static void ApplyLegacyGrid(string path, UnstructuredGrid grid, bool loadFlowLinksAndCells)
        {
            UnstructuredGrid legacyUnstructuredGrid = loadFlowLinksAndCells
                                                                ? NetFileImporter.ImportModelGrid(path)
                                                                : NetFileImporter.ImportGrid(path);
            if (grid == null)
            {
                grid = new UnstructuredGrid();
            }

            if (legacyUnstructuredGrid != null)
            {
                grid.ResetState(legacyUnstructuredGrid.Vertices, legacyUnstructuredGrid.Edges, legacyUnstructuredGrid.Cells, legacyUnstructuredGrid.FlowLinks);
                grid.CoordinateSystem = legacyUnstructuredGrid.CoordinateSystem;
            }
        }

        /// <summary>
        /// Reads the first <see cref="IHydroNetwork"/> and <see cref="IDiscretization"/> in the UGrid file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="network">Instance of a <see cref="IHydroNetwork"/> to add the newly read data to</param>
        /// <param name="reportProgress">Action to report user feedback to.</param>
        /// <param name="branchProperties">Additional branch properties</param>
        /// <param name="compartmentProperties">Additional compartment properties</param>
        /// <param name="forceCustomLengths">Force all branches in the network to have custom lengths and use the lengths that are read from file</param>
        /// <exception cref="IoNetCdfNativeError">Thrown when an error code is returned from a native function.</exception>
        private static DisposableNetworkGeometry ApplyNetworkGeometry(this IHydroNetwork network, string path,
                                                                      Action<string> reportProgress, 
                                                                      IEnumerable<CompartmentProperties> compartmentProperties, 
                                                                      IEnumerable<BranchProperties> branchProperties,
                                                                      bool forceCustomLengths = false)
        {
            Ensure.NotNull(compartmentProperties, nameof(compartmentProperties));
            Ensure.NotNull(branchProperties, nameof(branchProperties));
            var errorMessage = $"Could not load network from {path}";
            if (network == null || !IsValidPath(path))
            {
                Log.Error(errorMessage);
                return null;
            }

            DisposableNetworkGeometry networkGeometry = null;
            using (var api = CreateUGridApi())
            {
                api.Open(path);

                if (!api.IsUGridFile())
                {
                    Log.Error(errorMessage);
                    return null;
                }
                reportProgress?.Invoke(Resources.UGridFileHelper_ApplyNetworkGeometry_Reading_grid_step_1_of_4);
                networkGeometry = ReadNetwork(api, network, compartmentProperties, branchProperties, forceCustomLengths);

                network.CoordinateSystem = GetCoordinateSystemFromApi(api);
                network.UpdateGeodeticDistancesOfChannels();
            }

            return networkGeometry;
        }

        /// <summary>
        /// Reads the first <see cref="IHydroNetwork"/> and <see cref="IDiscretization"/> in the UGrid file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="discretization">Instance of a <see cref="IDiscretization"/> to clear and fill with newly read data.</param>
        /// <param name="network">Instance of a <see cref="IHydroNetwork"/> to add the newly read data to.</param>
        /// <param name="reportProgress">Action to report user feedback to.</param>
        /// <exception cref="IoNetCdfNativeError">Thrown when an error code is returned from a native function.</exception>
        private static Disposable1DMeshGeometry ApplyMesh1D(this IDiscretization discretization, 
                                                            string path,
                                                            IHydroNetwork network,
                                                            Action<string> reportProgress)
        {
            var errorMessage = $"Could not load computational grid from {path}";
            if (discretization == null || !IsValidPath(path))
            {
                Log.Error(errorMessage);
                return null;
            }
            
            // check if can use x/y or get coordinate via branch/chainage
            bool canUseXyForMesh1DNodeCoordinates = CanUseXYCoordinatePossibilitiesOfGridTypeOnLocation(path, UGridMeshType.Mesh1D, GridLocationType.Node);


            // needs to be done with new api instance
            // otherwise leads to crashes
            Disposable1DMeshGeometry mesh1d = null;
            using (var api = CreateUGridApi())
            {
                api.Open(path);
                if (!api.IsUGridFile())
                {
                    Log.Error(errorMessage);
                    return null;
                }
                reportProgress?.Invoke(Resources.UGridFileHelper_ApplyMesh1D_Reading_grid_step_2_of_4);
                mesh1d = Read1DMesh(api, discretization, network, canUseXyForMesh1DNodeCoordinates);
            }

            return mesh1d;
        }

        private static bool CanUseXYCoordinatePossibilitiesOfGridTypeOnLocation(string path, UGridMeshType meshType, GridLocationType locationType)
        {
            // using old implementation because api.getvar is not yet implemented
            if (!IsUGridFile(path)) return false;

            var file = NetCdfFile.OpenExisting(path);
            try
            {
                var variablesWithCfRoleAndTopologyDimension = file.GetVariables().Where(v =>
                    file.GetAttributeValue(v, "cf_role") != null && file.GetAttributeValue(v, "topology_dimension") != null);
                switch (meshType)
                {
                    case UGridMeshType.Combined:
                        break;
                    case UGridMeshType.Mesh1D:
                        //search correct variable on long name... this should probably be improved in the kernel....
                        var mesh1d = variablesWithCfRoleAndTopologyDimension.SingleOrDefault(v =>
                        {
                            var longName = file.GetAttributeValue(v, "long_name");
                            return longName.IndexOf("1D", StringComparison.InvariantCultureIgnoreCase) >=0 
                                   && longName.IndexOf("Mesh", StringComparison.InvariantCultureIgnoreCase) >= 0;
                        });
                        if (mesh1d == null) return false;

                        switch (locationType)
                        {
                            case GridLocationType.None:
                                break;
                            case GridLocationType.Node:
                                var mesh1DNodeCoordinates = file.GetAttributeValue(mesh1d, "node_coordinates");
                                return mesh1DNodeCoordinates != null 
                                       && mesh1DNodeCoordinates.IndexOf("node_x", StringComparison.InvariantCultureIgnoreCase) >=0 
                                       && mesh1DNodeCoordinates.IndexOf("node_y", StringComparison.InvariantCultureIgnoreCase) >=0;
                            case GridLocationType.Edge:
                                break;
                            case GridLocationType.Face:
                                break;
                            case GridLocationType.Volume:
                                break;
                            case GridLocationType.All2D:
                                break;
                        }
                        break;
                    case UGridMeshType.Mesh2D:
                        break;
                    case UGridMeshType.Mesh3D:
                        break;
                }
            }
            finally
            {
                file.Close();
            }

            return false;
        }

        /// <summary>
        /// Creates a new UGrid file and adds the <paramref name="grid"/>, <paramref name="network"/>, <paramref name="networkDiscretization"/>,
        /// <paramref name="links"/> and <paramref name="zValues"/>
        /// </summary>
        /// <remarks>Deletes the previous file if it exists</remarks>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="grid"><see cref="UnstructuredGrid"/> to store in the UGrid file</param>
        /// <param name="network"><see cref="IHydroNetwork"/> to store in the UGrid file</param>
        /// <param name="networkDiscretization"><see cref="IDiscretization"/> to store in the UGrid file</param>
        /// <param name="links"><see cref="IEnumerable{ILink1D2D}"/> to store in the UGrid file</param>
        /// <param name="name">Name of the model</param>
        /// <param name="pluginName">Name of the plugin creating the file</param>
        /// <param name="pluginVersion">Version of the plugin creating the file</param>
        /// <param name="location">Location of the Z values on the 2D grid</param>
        /// <param name="zValues">Z values for the 2D grid</param>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        /// <exception cref="IOException">This error is thrown when trying to delete the file</exception>
        public static void WriteGridToFile(string path, UnstructuredGrid grid, IHydroNetwork network, 
            IDiscretization networkDiscretization, IEnumerable<ILink1D2D> links, string name, string pluginName, string pluginVersion, BedLevelLocation location, double[] zValues)
        {
            FileUtils.DeleteIfExists(path);

            using (var api = CreateUGridApi())
            {
                var metaData = new FileMetaData(name, pluginName ?? "custom", pluginVersion ?? "unknown");
                api.CreateFile(path, metaData);
                
                if (network?.Nodes?.Count > 0)
                {
                    var networkId = api.WriteNetworkGeometry(network.CreateDisposableNetworkGeometry());

                    if (networkDiscretization != null && networkDiscretization.Locations.Values.Count > 0)
                    {
                        api.WriteMesh1D(networkDiscretization.CreateDisposable1DMeshGeometry(), networkId);
                    }
                }

                if (grid == null || grid.IsEmpty)
                    return;

                api.WriteMesh2D(grid.CreateDisposable2DMeshGeometry());
            }

            if (grid.IsEmpty)
            {
                return;
            }

            // needs to be done with new api instance because on close
            // the grid is flushed to file, making it available to write 
            // depended data
            using (var api = CreateUGridApi())
            {
                api.Open(path, OpenMode.Appending);

                var link1D2Ds = links?.ToList();
                if (link1D2Ds?.Count > 0)
                {
                    api.WriteLinks(link1D2Ds.CreateDisposableLinksGeometry());
                }

                WriteZValuesWithApi(api, location, zValues, path);
            }
        }

        /// <summary>
        /// Overrides the existing x,y values of the first <see cref="UnstructuredGrid"/>s vertices
        /// </summary>
        /// <remarks>This function is mostly used for updating the vertices after a coordinate transformation</remarks>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="unstructuredGrid"><see cref="UnstructuredGrid"/> containing the new vertices values</param>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        public static void RewriteGridCoordinates(string path, UnstructuredGrid unstructuredGrid)
        {
            if (!IsValidPath(path))
            {
                return;
            }

            using (var api = CreateUGridApi())
            {
                api.Open(path, OpenMode.Appending);

                if (api.IsUGridFile())
                {
                    var xValues = unstructuredGrid.Vertices.Select(v => v.X).ToArray();
                    var yValues = unstructuredGrid.Vertices.Select(v => v.Y).ToArray();
                    
                    var meshIds = api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                    if (meshIds.Length != 0)
                    {
                        api.ResetMeshVerticesCoordinates(meshIds[0], xValues, yValues);
                    }
                    return;
                }
            }
            NetFile.RewriteGridCoordinates(path, unstructuredGrid);

        }

        /// <summary>
        /// Gets the number of 1d networks in the UGrid file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <returns>The number of 1d networks in the UGrid file</returns>
        public static int GetNumberOfNetworks(string path)
        {
            if (!IsValidPath(path))
            {
                return 0;
            }

            if (lastCheckedUGridPath == path && numberOfNetworks >= 0) return numberOfNetworks;

            using (var api = CreateUGridApi())
            {
                api.Open(path);
                numberOfNetworks = api.GetNumberOfNetworks();
                return numberOfNetworks;
            }
        }

        /// <summary>
        /// Gets the number of discretizations in the UGrid file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <returns>The number of discretizations in the UGrid file</returns>
        public static int GetNumberOfNetworkDiscretizations(string path)
        {
            if (!IsValidPath(path))
            {
                return 0;
            }
            if(numberOfMeshByType == null) numberOfMeshByType = new Dictionary<UGridMeshType, int>();
            if (lastCheckedUGridPath == path && numberOfMeshByType.TryGetValue(UGridMeshType.Mesh1D, out var nrOf1DMesh))
            {
                return nrOf1DMesh;
            }

            using (var api = CreateUGridApi())
            {
                api.Open(path);
                numberOfMeshByType[UGridMeshType.Mesh1D] = api.GetNumberOfMeshByType(UGridMeshType.Mesh1D);
                return numberOfMeshByType[UGridMeshType.Mesh1D];
            }
        }

        /// <summary>
        /// Returns if the file (<paramref name="path"/>) is a UGrid file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>if the file is a UGrid file</returns>
        public static bool IsUGridFile(string path)
        {
            if (!IsValidPath(path))
            {
                return false;
            }

            if (lastCheckedUGridPath == path)
            {
                return lastCheckedUGridPathResult;
            }

            lastCheckedUGridPath = path;

            using (var api = CreateUGridApi())
            {
                api.Open(path);
                
                lastCheckedUGridPathResult = api.IsUGridFile();

                return lastCheckedUGridPathResult;
            }
        }
        
        private static void WriteZValuesWithApi(IUGridApi api, BedLevelLocation location, double[] values, string path)
        {
            var meshIds2d = api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
            if (meshIds2d.Length == 0)
            {
                Log.Warn($"Unable to write z-values to file: \"{path}\", no 2d mesh found");
                return;
            }

            var locationType = GetLocationType(location);
            if (locationType == GridLocationType.None)
                return;

            var variableName = GetVariableName(location);
            var longName = GetVariableLongName(location);

            var unit = UGridConstants.Naming.Meter;
            var standardName = UGridConstants.Naming.Altitude;

            api.SetVariableValues(variableName, standardName, longName, unit, meshIds2d[0], locationType, values);
        }

        private static ICoordinateSystem GetCoordinateSystemFromApi(IUGridApi api)
        {
            var epsgCode = api.GetCoordinateSystemCode();
            return epsgCode > 0
                ? new OgrCoordinateSystemFactory().CreateFromEPSG(epsgCode)
                : null;
        }

        private static Disposable1DMeshGeometry Read1DMesh(IUGridApi api, IDiscretization discretization, IHydroNetwork network, bool canUseXyForMesh1DNodeCoordinates)
        {
            Disposable1DMeshGeometry mesh1d = null;
            var meshIds = api.GetMeshIdsByMeshType(UGridMeshType.Mesh1D);
            if (meshIds.Length != 0)
            {
                mesh1d = api.GetMesh1D(meshIds[0]);
                discretization.SetMesh1DGeometry(mesh1d, network, canUseXyForMesh1DNodeCoordinates);
            }

            return mesh1d;
        }

        private static DisposableNetworkGeometry ReadNetwork(IUGridApi api, IHydroNetwork network,
                                                             IEnumerable<CompartmentProperties> compartmentPropertiesList,
                                                             IEnumerable<BranchProperties> branchPropertiesList,
                                                             bool forceCustomLengths = false)
        {
            var networkIds = api.GetNetworkIds();
            if (networkIds.Length == 0)
            {
                return null;
            }

            DisposableNetworkGeometry networkGeometry = api.GetNetworkGeometry(networkIds[0]);
            network.SetNetworkGeometry(networkGeometry, branchPropertiesList, compartmentPropertiesList, forceCustomLengths);
            return networkGeometry;
        }

        private static void UpdateNodeZVariables(NetCdfFile file)
        {
            file.ReDefine();

            // When updating the coordinate system in a UGrid file
            // we must also update the grid-mapping attribute of only the node_Z variable
            // to refer to the coordinate system
            // This is because the CF commission is locked down on the node_x and node_y variables
            // and we can't set the grid_mapping attribute on these
            const string gridMappingAttributeName = "grid_mapping";
            const string projectedCoordinateSystemAttributeValue = "projected_coordinate_system";
            const string nodeZVariableName = "_node_z";

            file.GetVariables()
                .Where(v => file.GetVariableName(v).EndsWith(nodeZVariableName))
                .ForEach(v =>
                    file.AddAttribute(v,
                        new NetCdfAttribute(gridMappingAttributeName, projectedCoordinateSystemAttributeValue)));

            file.EndDefine();
            file.Flush();
        }

        private static string GetVariableName(BedLevelLocation location)
        {
            switch (location)
            {
                case BedLevelLocation.Faces:
                case BedLevelLocation.FacesMeanLevFromNodes: return UGridConstants.Naming.FaceZ;
                case BedLevelLocation.CellEdges: return "";
                case BedLevelLocation.NodesMeanLev:
                case BedLevelLocation.NodesMinLev:
                case BedLevelLocation.NodesMaxLev: return UGridConstants.Naming.NodeZ;
                default:
                    throw new ArgumentOutOfRangeException(nameof(location), location, null);
            }
        }

        private static string GetVariableLongName(BedLevelLocation location)
        {
            switch (location)
            {
                case BedLevelLocation.Faces:
                case BedLevelLocation.FacesMeanLevFromNodes: 
                    return Resources.UGrid_WriteZValuesAtFacesForMeshId_z_coordinate_of_mesh_faces;
                case BedLevelLocation.CellEdges: 
                    return "";
                case BedLevelLocation.NodesMeanLev:
                case BedLevelLocation.NodesMinLev:
                case BedLevelLocation.NodesMaxLev: 
                    return Resources.UGrid_WriteZValuesAtNodesForMeshId_z_coordinate_of_mesh_nodes;
                default:
                    throw new ArgumentOutOfRangeException(nameof(location), location, null);
            }
        }

        private static GridLocationType GetLocationType(BedLevelLocation location)
        {
            switch (location)
            {
                case BedLevelLocation.Faces:
                case BedLevelLocation.FacesMeanLevFromNodes: 
                    return GridLocationType.Face;
                case BedLevelLocation.CellEdges:
                    Log.WarnFormat(Resources.UGridFileHelper_ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);
                    return GridLocationType.None;
                case BedLevelLocation.NodesMeanLev:
                case BedLevelLocation.NodesMinLev:
                case BedLevelLocation.NodesMaxLev: 
                    return GridLocationType.Node;
                default:
                    throw new ArgumentOutOfRangeException(nameof(location), location, null);
            }
        }

        private static IUGridApi CreateUGridApi()
        {
            return new RemoteUGridApi();
            //use new UGridApi() to disable remoting
        }

        private static bool IsValidPath(string path)
        {
            if (path == null)
                return false;

            var fileInfo = new FileInfo(path);
            return fileInfo.Exists && 
                   fileInfo.Length != 0 && 
                   (!string.IsNullOrEmpty(fileInfo.Name));
        }

        /// <summary>
        /// Reads the ugrid file data into our DOM.
        /// </summary>
        /// <param name="netFilePath">Path to the UGrid file</param>
        /// <param name="convertedUgridFileObjects">Object with the Grid, Network, Discretization and Links1D2D objects used in our DOM. This will be correctly filled</param>
        /// <param name="reportProgress">Action to report user feedback to.</param>
        /// <param name="loadFlowLinksAndCells">Also read flow links and cell information (Applies to NetFile only).
        ///     With a UGrid file the cell information is always read but not the flow links</param>
        /// <param name="recreateCells">Recreates the cell information by calling FindCells instead of reading it from file</param>
        /// <param name="forceCustomLengths">Force all branches in the network to have custom lengths and use the lengths that are read from file</param>
        public static void ReadNetFileDataIntoModel(string netFilePath, IConvertedUgridFileObjects convertedUgridFileObjects,
                                                    Action<string> reportProgress = null, 
                                                    bool loadFlowLinksAndCells = false, 
                                                    bool recreateCells = true, 
                                                    bool forceCustomLengths = false)
        {
            convertedUgridFileObjects.CompartmentProperties = convertedUgridFileObjects.CompartmentProperties ?? Enumerable.Empty<CompartmentProperties>();
            convertedUgridFileObjects.BranchProperties = convertedUgridFileObjects.BranchProperties ?? Enumerable.Empty<BranchProperties>();
            DisposableNetworkGeometry networkGeometry = convertedUgridFileObjects.HydroNetwork.ApplyNetworkGeometry(netFilePath, reportProgress, convertedUgridFileObjects.CompartmentProperties, convertedUgridFileObjects.BranchProperties, forceCustomLengths);
            Disposable1DMeshGeometry mesh1d = convertedUgridFileObjects.Discretization.ApplyMesh1D(netFilePath, convertedUgridFileObjects.HydroNetwork, reportProgress);
            
            if (convertedUgridFileObjects.Grid != null)
            {
                reportProgress?.Invoke(Resources.UGridFileHelper_ReadNetFileDataIntoModel_Reading_grid_step_3_of_4);
                Disposable2DMeshGeometry mesh2d = Read2DMesh(netFilePath);
                convertedUgridFileObjects.Grid.ApplyMesh2D(netFilePath, mesh2d, loadFlowLinksAndCells, recreateCells);
                
                if (mesh2d != null
                    && convertedUgridFileObjects.Discretization != null
                    && mesh1d != null
                    && networkGeometry != null)
                {
                    reportProgress?.Invoke(Resources.UGridFileHelper_ReadNetFileDataIntoModel_Reading_grid_step_4_of_4);
                    var generatedObjectsForLinks = new GeneratedObjectsForLinks
                    {
                        Grid = convertedUgridFileObjects.Grid,
                        Mesh2d = mesh2d,
                        Discretization = convertedUgridFileObjects.Discretization,
                        Mesh1d = mesh1d,
                        NetworkGeometry = networkGeometry,
                        Links1D2D = convertedUgridFileObjects.Links1D2D
                    };
                    if (!IsValidPath(netFilePath))
                    {
                        generatedObjectsForLinks.Links1D2D.Clear();
                        return;
                    }

                    using (var api = CreateUGridApi())
                    {
                        generatedObjectsForLinks.Read1D2DLinks(netFilePath, api);
                    }
                }
            }

        }
    }
}