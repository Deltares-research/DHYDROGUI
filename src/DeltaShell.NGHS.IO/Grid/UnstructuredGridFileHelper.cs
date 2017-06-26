using System;
using System.IO;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Adaptors;
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
                return null;

            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.IONC_CONV_UGRID:
                    using (var fmUGridAdaptor = new UGridToUnstructuredGridAdaptor(path))
                    {
                        return fmUGridAdaptor.GetUnstructuredGridFromUGridMeshId(1);
                    }
                case GridApiDataSet.DataSetConventions.IONC_CONV_OTHER:
                    return loadFlowLinksAndCells
                        ? NetFileImporter.ImportModelGrid(path)
                        : NetFileImporter.ImportGrid(path);
                default:
                    return null;
            }
        }

        public static void WriteZValues(string path, BedLevelLocation location, double[] values)
        {
            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.IONC_CONV_UGRID:
                    using (var uGrid = new UGrid(path, GridApiDataSet.NetcdfOpenMode.nf90_write))
                    {
                        switch (location)
                        {
                            case BedLevelLocation.Faces:
                            case BedLevelLocation.FacesMeanLevFromNodes:
                                uGrid.WriteZValuesAtFaces(1, values);
                                break;
                            case BedLevelLocation.CellEdges:
                                Log.WarnFormat("Unable to Write z-values at this location, CellEdges are not currently supported");
                                break;
                            case BedLevelLocation.NodesMeanLev:
                            case BedLevelLocation.NodesMinLev:
                            case BedLevelLocation.NodesMaxLev:
                                uGrid.WriteZValuesAtNodes(1, values);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("location", location, null);
                        }
                    }
                    break;
                case GridApiDataSet.DataSetConventions.IONC_CONV_OTHER:
                    NetFile.WriteZValues(path, values);
                    break;
            }
        }

        public static ICoordinateSystem GetCoordinateSystem(string path)
        {
            switch (GetConvention(path))
            {
                case GridApiDataSet.DataSetConventions.IONC_CONV_UGRID:
                    using (var uGrid = new UGrid(path))
                    {
                        return uGrid.CoordinateSystem;
                    }
                case GridApiDataSet.DataSetConventions.IONC_CONV_OTHER:
                    return NetFile.ReadCoordinateSystem(path);
                default:
                    return null;
            }
        }

        public static void SetCoordinateSystem(string path, ICoordinateSystem coordinateSystem)
        {
            var convention = GetConvention(path);

            // Note: Temporary solution - UGrid v2 will likely change the way the coordinate systems are written in the NetFile
            if (convention == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
            {
                string meshName;
                using (var uGrid = new UGrid(path))
                {
                    meshName = uGrid.NameOfMesh(1);
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

            if (convention == GridApiDataSet.DataSetConventions.IONC_CONV_OTHER)
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
                case GridApiDataSet.DataSetConventions.IONC_CONV_OTHER:
                    NetFile.RewriteGridCoordinates(path, unstructuredGrid);
                    break;
                case GridApiDataSet.DataSetConventions.IONC_CONV_UGRID:
                    using (var uGrid = new UGrid(path, GridApiDataSet.NetcdfOpenMode.nf90_write))
                    {
                        uGrid.RewriteGridCoordinates(1, unstructuredGrid.Vertices.Select(v => v.X).ToArray(),
                            unstructuredGrid.Vertices.Select(v => v.Y).ToArray());
                    }
                    break;
            }
        }

        public static void DoIfUgrid(string path, Action<UGridToUnstructuredGridAdaptor> ugridAction)
        {
            var convention = GetConvention(path);
            if (convention != GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
                return;

            using (var uGridAdaptor = new UGridToUnstructuredGridAdaptor(path))
            {
                ugridAction(uGridAdaptor);
            }
        }

        private static GridApiDataSet.DataSetConventions GetConvention(string path)
        {
            GridApiDataSet.DataSetConventions convention;
            using (var gridApi = GridApiFactory.CreateNew())
            {
                var ierr = gridApi.GetConvention(path, out convention);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new Exception("Couldn't get the grid convention because of error number: " + ierr);
                }
            }
            return convention;
        }
    }
}