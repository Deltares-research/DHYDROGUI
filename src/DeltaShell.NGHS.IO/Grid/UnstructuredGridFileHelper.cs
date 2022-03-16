using System;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Adapters;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class UnstructuredGridFileHelper
    {
        public enum BedLevelLocation
        {
            Faces = 1,
            CellEdges = 2,
            NodesMeanLev = 3,
            NodesMinLev = 4,
            NodesMaxLev = 5,
            FacesMeanLevFromNodes = 6
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(UnstructuredGridFileHelper));

        /// <summary>
        /// Load the <see cref="UnstructuredGrid"/> from the file specified
        /// at <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to the unstructured grid.</param>
        /// <param name="loadFlowLinksAndCells">
        /// if set to <c>true</c> [load flow links and cells].
        /// <paramref name="loadFlowLinksAndCells"/> defaults to false.
        /// </param>
        /// <param name="callCreateCells">
        /// if set to <c>true</c> and the grid is in UGrid convention then CreateCells will be called.
        /// <paramref name="callCreateCells"/> defaults to false.
        /// </param>
        /// <returns>
        /// The first grid stored in <paramref name="path"/>
        /// </returns>
        /// <remarks>
        /// CreateCells will recalculate the cell centers using the kernel.
        /// This will ensure the correct cell centers will be used for spatial
        /// operations. This should be called for input grids that are used for
        /// spatial operations. This SHOULD NOT be called for output grids.
        /// CreateCells will reshuffle the indices. When this is called for output
        /// grids, the data associated with cells will be incorrect, if the indices
        /// are reshuffled.
        /// </remarks>
        public static UnstructuredGrid LoadFromFile(string path,
                                                    bool loadFlowLinksAndCells = false,
                                                    bool callCreateCells = false)
        {
            if (!File.Exists(path) || Path.GetFileName(path) == null)
            {
                Log.ErrorFormat("Could not find grid at \"{0}\"", path);
                return null;
            }

            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.CONV_UGRID:
                    using (var fmUGridAdapter = new UGridToUnstructuredGridAdapter(path))
                    {
                        return fmUGridAdapter.GetUnstructuredGridFromUGridMeshId(1, callCreateCells: callCreateCells);
                    }
                case GridApiDataSet.DataSetConventions.CONV_OTHER:
                    return loadFlowLinksAndCells
                               ? NetFileImporter.ImportModelGrid(path)
                               : NetFileImporter.ImportGrid(path);
                default:
                    return null;
            }
        }

        public static double[] ReadZValues(string path, BedLevelLocation location)
        {
            var zValues = new double[0];
            GridApiDataSet.DataSetConventions fileConvention = GetConvention(path);
            if (fileConvention == GridApiDataSet.DataSetConventions.CONV_UGRID)
            {
                using (var uGrid = new UGrid(path))
                {
                    switch (location)
                    {
                        case BedLevelLocation.Faces:
                        case BedLevelLocation.FacesMeanLevFromNodes:
                            zValues = uGrid.ReadZValuesAtFacesForMeshId(1);
                            break;
                        case BedLevelLocation.CellEdges:
                            Log.WarnFormat(Resources.UnstructuredGridFileHelper_ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);
                            break;
                        case BedLevelLocation.NodesMeanLev:
                        case BedLevelLocation.NodesMinLev:
                        case BedLevelLocation.NodesMaxLev:
                            zValues = uGrid.ReadZValuesAtNodesForMeshId(1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(location), location, null);
                    }
                }
            }
            else
            {
                Log.WarnFormat(Resources.UnstructuredGridFileHelper_ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, path);
            }

            return zValues;
        }

        public static void WriteZValues(string path, BedLevelLocation location, double[] values)
        {
            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.CONV_UGRID:
                    using (var uGrid = new UGrid(path, GridApiDataSet.NetcdfOpenMode.nf90_write))
                    {
                        switch (location)
                        {
                            case BedLevelLocation.Faces:
                            case BedLevelLocation.FacesMeanLevFromNodes:
                                uGrid.WriteZValuesAtFacesForMeshId(1, values);
                                break;
                            case BedLevelLocation.CellEdges:
                                Log.WarnFormat(Resources.UnstructuredGridFileHelper_WriteZValues_Unable_to_write_z_values_at_this_location__CellEdges_are_not_currently_supported);
                                break;
                            case BedLevelLocation.NodesMeanLev:
                            case BedLevelLocation.NodesMinLev:
                            case BedLevelLocation.NodesMaxLev:
                                uGrid.WriteZValuesAtNodesForMeshId(1, values);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(location), location, null);
                        }
                    }

                    break;
                case GridApiDataSet.DataSetConventions.CONV_OTHER:
                    NetFile.WriteZValues(path, values);
                    break;
            }
        }

        public static ICoordinateSystem GetCoordinateSystem(string path)
        {
            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.CONV_UGRID:
                    using (var uGrid = new UGrid(path))
                    {
                        if (!uGrid.IsInitialized())
                        {
                            uGrid.Initialize();
                        }

                        return uGrid.CoordinateSystem;
                    }
                case GridApiDataSet.DataSetConventions.CONV_OTHER:
                    return NetFile.ReadCoordinateSystem(path);
                default:
                    return null;
            }
        }

        public static void SetCoordinateSystem(string path, ICoordinateSystem coordinateSystem)
        {
            GridApiDataSet.DataSetConventions convention = GetConvention(path);
            if (convention == GridApiDataSet.DataSetConventions.CONV_NULL)
            {
                return;
            }

            // Note: Temporary solution - UGrid v2 will likely change the way the coordinate systems are written in the NetFile
            if (convention == GridApiDataSet.DataSetConventions.CONV_UGRID)
            {
                string meshName;
                using (var uGrid = new UGrid(path))
                {
                    meshName = uGrid.GetMeshName(1);
                }

                if (string.IsNullOrEmpty(meshName))
                {
                    return;
                }

                const string gridMappingAttributeName = "grid_mapping";
                const string projectedCoordinateSystemAttributeValue = "projected_coordinate_system";
                const string nodeZVariableName = "_node_z";

                NetCdfFile netCdfFile = null;
                try
                {
                    netCdfFile = NetCdfFile.OpenExisting(path, true);
                    NetCdfVariable netCdfVariable = netCdfFile.GetVariableByName(meshName + nodeZVariableName);
                    if (netCdfVariable == null)
                    {
                        return;
                    }

                    netCdfFile.ReDefine();
                    // when updating the coordinate system in a UGrid file we must also update the grid-mapping attribute of the node_Z variable
                    netCdfFile.AddAttribute(netCdfVariable, new NetCdfAttribute(gridMappingAttributeName, projectedCoordinateSystemAttributeValue));

                    netCdfFile.EndDefine();
                    netCdfFile.Flush();
                }
                finally
                {
                    if (netCdfFile != null)
                    {
                        netCdfFile.Close();
                    }
                }
            }

            NetFile.WriteCoordinateSystem(path, coordinateSystem);
        }

        /// <summary>
        /// Create a new Unstructured file at <paramref name="path"/>
        /// containing only the correct Unstructured Grid metadata.
        /// </summary>
        /// <param name="path">The path at which the new Unstructured Grid File will be located</param>
        public static void WriteEmptyUnstructuredGridFile(string path)
        {
            var metaData = new UGridGlobalMetaData("Unknown model",
                                                   "DeltaShell",
                                                   GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION.ToString());
            using (var uGrid = new UGrid(path, metaData))
            {
                uGrid.CreateFile();
            }
        }

        /// <summary>
        /// Write <paramref name="newCoordinateSystem"/> to the _net.nc file specified at <paramref name="path"/>
        /// If <paramref name="writeNullCoordinateSystem"/> is true, and a null coordinate system is provided
        /// A projected_coordinated_system with EPSG 0 will be written to file.
        /// </summary>
        /// <param name="path">Path to the UGrid _net.nc file.</param>
        /// <param name="newCoordinateSystem">The new coordinate system to be written to file.</param>
        /// <param name="writeNullCoordinateSystem">
        /// If true, and <paramref name="newCoordinateSystem"/> == null, then write a
        /// projected_coordinate_system with EPSG 0 to file.
        /// </param>
        public static void WriteCoordinateSystemToFile(string path,
                                                       ICoordinateSystem newCoordinateSystem,
                                                       bool writeNullCoordinateSystem = false)
        {
            // Issue: D3DFMIQ-512
            // This code should be replaced with a call to the net_io_lib, once
            // it provides a method to write the coordinate system directly to
            // the _net.nc file. For the time being this should be more correct
            // than using the NetFile.WriteCoordinateSystem method.

            ICoordinateSystem currentCoordinateSystem = null;
            bool hasCoordinateSystem =
                FileContainsCoordinateSystem(path, out currentCoordinateSystem);

            // we do not want to write anything because we passed a null
            // coordinate system.
            if (newCoordinateSystem == null && !writeNullCoordinateSystem)
            {
                return;
            }

            // we want to write a null coordinate system, but a null coordinate
            // system has already been written.
            if (hasCoordinateSystem && currentCoordinateSystem == null && newCoordinateSystem == null)
            {
                return;
            }

            // we want to write a non null coordinate system, but this coordinate
            // system already exists in file.
            if (hasCoordinateSystem && currentCoordinateSystem != null && newCoordinateSystem != null &&
                currentCoordinateSystem.AuthorityCode == newCoordinateSystem.AuthorityCode)
            {
                return;
            }

            NetCdfFile netCdfFile = null;
            try
            {
                netCdfFile = NetCdfFile.OpenExisting(path, true);
                netCdfFile.ReDefine();

                // issue: D3DFMIQ-512
                // Once it is possible to either update or write UGrid files
                // within DeltaShell, this function needs to be updated.
                // Currently, it is possible for the situation where both
                // coordinate system variables exist within the _net.nc file,
                // as it is not possible to rename a variable, or rewrite the
                // whole _net.nc file. As such, it is necessary to update both
                // variables. In the future, when:
                //
                // currentCoordinateSystem.IsGeographic != newCoordinateSystem.IsGeographic,
                //
                // you would remove the variable associated with currentCoordinateSystem,
                // and only execute the relevant if statement:
                //
                // * if (currentCoordinateSystem.IsGeographic != newCoordinateSystem.IsGeographic)
                // *     RemoveVariable(netCdfFile, currentCoordinateSystem.IsGeographic ? "wgs84" : "projected_coordinate_system");
                // * var variableName = newCoordinateSystem.IsGeographic ? "wgs84" : "projected_coordinate_system";
                // * var pcs = netCdfFile.GetVariableByName(variableName) ?? 
                // *           netCdfFIle.AddVariable(variableName, NetCdfDataType.NcInteger, new NetCdfDimension[0]);

                // Update wgs84
                if ((currentCoordinateSystem?.IsGeographic ?? false) || (newCoordinateSystem?.IsGeographic ?? false))
                {
                    NetCdfVariable pcs = netCdfFile.GetVariableByName("wgs84") ??
                                         netCdfFile.AddVariable("wgs84", NetCdfDataType.NcInteger, new NetCdfDimension[0]);
                    WriteCoordinateSystemWithVariable(netCdfFile, pcs, newCoordinateSystem);
                }

                // Update projected_coordinate_system
                if (newCoordinateSystem == null ||
                    !newCoordinateSystem.IsGeographic ||
                    (!currentCoordinateSystem?.IsGeographic ?? false))
                {
                    NetCdfVariable pcs = netCdfFile.GetVariableByName("projected_coordinate_system") ??
                                         netCdfFile.AddVariable("projected_coordinate_system", NetCdfDataType.NcInteger, new NetCdfDimension[0]);
                    WriteCoordinateSystemWithVariable(netCdfFile, pcs, newCoordinateSystem);
                }

                netCdfFile.EndDefine();
                netCdfFile.Flush();
            }
            finally
            {
                netCdfFile?.Close();
            }
        }

        public static void WriteGridToFile(string path, UnstructuredGrid grid)
        {
            GridApiDataSet.DataSetConventions convention = GetConvention(path);

            if (convention == GridApiDataSet.DataSetConventions.CONV_OTHER)
            {
                if (!File.Exists(path))
                {
                    if (grid == null || grid.IsEmpty)
                    {
                        var file = NetCdfFile.CreateNew(path);
                        file.Close();
                        return;
                    }

                    NetFile.Write(path, grid);
                }
                else
                {
                    NetFile.WriteToExisting(path, grid);
                }
            }

            // throw error ??
        }

        public static void RewriteGridCoordinates(string path, UnstructuredGrid unstructuredGrid)
        {
            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.CONV_OTHER:
                    NetFile.RewriteGridCoordinates(path, unstructuredGrid);
                    break;
                case GridApiDataSet.DataSetConventions.CONV_UGRID:
                    using (var uGrid = new UGrid(path, GridApiDataSet.NetcdfOpenMode.nf90_write))
                    {
                        uGrid.RewriteGridCoordinatesForMeshId(1, unstructuredGrid.Vertices.Select(v => v.X).ToArray(),
                                                              unstructuredGrid.Vertices.Select(v => v.Y).ToArray());
                    }

                    break;
            }
        }

        public static void DoIfUgrid(string path, Action<UGridToUnstructuredGridAdapter> ugridAction)
        {
            GridApiDataSet.DataSetConventions convention = GetConvention(path);
            if (convention != GridApiDataSet.DataSetConventions.CONV_UGRID)
            {
                return;
            }

            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(path))
            {
                ugridAction(uGridAdaptor);
            }
        }

        /// <summary>
        /// Check if the _net.nc file at <paramref name="path"/> defines a
        /// coordinate system and returns this coordinate system as
        /// <paramref name="coordinateSystem"/>.
        /// If true and <paramref name="coordinateSystem"/> equals null,
        /// then a coordinate system with a non-existent EPGS code (i.e. 0)
        /// has been written to file.
        /// <returns> If the _net.nc file at <paramref name="path"/> specifies a coordinate system.</returns>
        /// </summary>
        private static bool FileContainsCoordinateSystem(string path, out ICoordinateSystem coordinateSystem)
        {
            var result = false;

            NetCdfFile netCdfFile = null;
            try
            {
                netCdfFile = NetCdfFile.OpenExisting(path);

                result = netCdfFile.GetVariableByName("wgs84") != null ||
                         netCdfFile.GetVariableByName("projected_coordinate_system") != null;
            }
            finally
            {
                netCdfFile?.Close();
            }

            coordinateSystem = GetCoordinateSystem(path);
            return result;
        }

        private static void WriteCoordinateSystemWithVariable(NetCdfFile file,
                                                              NetCdfVariable pcs,
                                                              ICoordinateSystem coordinateSystem)
        {
            file.AddAttribute(pcs, new NetCdfAttribute("name", coordinateSystem?.Name ?? "Unknown projected"));

            int epsg = coordinateSystem != null ? (int) coordinateSystem.AuthorityCode : 0;
            file.AddAttribute(pcs, new NetCdfAttribute("epsg", epsg));

            file.AddAttribute(pcs,
                              new NetCdfAttribute("grid_mapping_name", coordinateSystem != null &&
                                                                       coordinateSystem.IsGeographic
                                                                           ? "latitude_longitude"
                                                                           : "Unknown projected"));

            file.AddAttribute(pcs, new NetCdfAttribute("longitude_of_prime_meridian", 0.0));

            double semiMajorAxis = coordinateSystem?.GetSemiMajor() ?? 6378137.0;
            double semiMinorAxis = coordinateSystem?.GetSemiMinor() ?? 6356752.314245;
            double inverseFlattening = coordinateSystem?.GetInverseFlattening() ?? 298.257223563;

            file.AddAttribute(pcs, new NetCdfAttribute("semi_major_axis", semiMajorAxis));
            file.AddAttribute(pcs, new NetCdfAttribute("semi_minor_axis", semiMinorAxis));
            file.AddAttribute(pcs, new NetCdfAttribute("inverse_flattening", inverseFlattening));

            if (coordinateSystem != null)
            {
                file.AddAttribute(pcs, new NetCdfAttribute("proj4_params", coordinateSystem.PROJ4));
            }

            file.AddAttribute(pcs, new NetCdfAttribute("EPSG_code", string.Format("EPSG:{0}", epsg)));

            if (coordinateSystem != null)
            {
                file.AddAttribute(pcs, new NetCdfAttribute("projection_name", "unknown"));
                file.AddAttribute(pcs, new NetCdfAttribute("wkt", coordinateSystem.WKT));
            }
        }

        private static GridApiDataSet.DataSetConventions GetConvention(string path)
        {
            IUGridApi gridApi = GridApiFactory.CreateNew();
            if (gridApi == null)
            {
                return GridApiDataSet.DataSetConventions.CONV_NULL;
            }

            using (gridApi)
            {
                GridApiDataSet.DataSetConventions convention;
                int ierr = gridApi.GetConvention(path, out convention);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    throw new GridApiException(GridApiExceptionMessage.Format(ierr, "Couldn't get the grid convention"));
                }

                return convention;
            }
        }
    }
}