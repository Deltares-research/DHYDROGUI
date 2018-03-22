using System;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Adaptors;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class UnstructuredGridFileHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UnstructuredGridFileHelper));

        public enum BedLevelLocation
        {
            Faces = 1,
            CellEdges = 2,
            NodesMeanLev = 3,
            NodesMinLev = 4,
            NodesMaxLev = 5,
            FacesMeanLevFromNodes = 6
        }

        public static UnstructuredGrid LoadFromFile(string path, bool loadFlowLinksAndCells = false)
        {
            if (!File.Exists(path) || Path.GetFileName(path) == null)
            {
                Log.ErrorFormat("Could not find grid at \"{0}\"", path);
                return null;
            }

            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.CONV_UGRID:
                    using (var fmUGridAdaptor = new UGridToUnstructuredGridAdaptor(path))
                    {
                        return fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
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
            var fileConvention = GetConvention(path);
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
                            throw new ArgumentOutOfRangeException("location", location, null);
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
                                throw new ArgumentOutOfRangeException("location", location, null);
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
                        if(!uGrid.IsInitialized()) uGrid.Initialize();
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
            var convention = GetConvention(path);
            if (convention == GridApiDataSet.DataSetConventions.CONV_NULL) return;

            // Note: Temporary solution - UGrid v2 will likely change the way the coordinate systems are written in the NetFile
            if (convention == GridApiDataSet.DataSetConventions.CONV_UGRID)
            {
                string meshName;
                using (var uGrid = new UGrid(path))
                {
                    meshName = uGrid.GetMeshName(1);
                }
                if (string.IsNullOrEmpty(meshName))
                    return;

                const string gridMappingAttributeName = "grid_mapping";
                const string projectedCoordinateSystemAttributeValue = "projected_coordinate_system";
                const string nodeZVariableName = "_node_z";

                NetCdfFile netCdfFile = null;
                try
                {
                    netCdfFile = NetCdfFile.OpenExisting(path, true);
                    var netCdfVariable = netCdfFile.GetVariableByName(meshName + nodeZVariableName);
                    if (netCdfVariable == null)
                        return;

                    netCdfFile.ReDefine();
                    // when updating the coordinate system in a UGrid file we must also update the grid-mapping attribute of the node_Z variable
                    netCdfFile.AddAttribute(netCdfVariable, new NetCdfAttribute(gridMappingAttributeName, projectedCoordinateSystemAttributeValue));

                    netCdfFile.EndDefine();
                    netCdfFile.Flush();
                }
                finally
                {
                    if (netCdfFile != null)
                        netCdfFile.Close();
                }
            }

            NetFile.WriteCoordinateSystem(path, coordinateSystem);
        }

        public static void WriteGridToFile(string path, UnstructuredGrid grid)
        {
            var convention = GetConvention(path);

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

        public static void DoIfUgrid(string path, Action<UGridToUnstructuredGridAdaptor> ugridAction)
        {
            var convention = GetConvention(path);
            if (convention != GridApiDataSet.DataSetConventions.CONV_UGRID)
                return;

            using (var uGridAdaptor = new UGridToUnstructuredGridAdaptor(path))
            {
                ugridAction(uGridAdaptor);
            }
        }

        private static GridApiDataSet.DataSetConventions GetConvention(string path)
        {
            var gridApi = GridApiFactory.CreateNew();
            if (gridApi == null)
            {
                return GridApiDataSet.DataSetConventions.CONV_NULL;
            }

            using (gridApi)
            {
                GridApiDataSet.DataSetConventions convention;
                var ierr = gridApi.GetConvention(path, out convention);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    throw new Exception("Couldn't get the grid convention because of error number: " + ierr);
                }
                return convention;
            }
        }
    }
}