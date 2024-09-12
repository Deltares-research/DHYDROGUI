using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// Helper for doing (U)Grid / network / mesh related actions
    /// </summary>
    public sealed class UGridFile : IDisposable
    {
        public const double DEFAULT_NO_DATA_VALUE = -999.0;
        public const int IDS_SIZE = 40;

        // Flag to indicate if the object has already been disposed
        private bool disposed = false;
        private string lastCheckedUGridPath;
        private bool lastCheckedUGridPathResult;
        private readonly FileMetaData metaData;
        private Dictionary<UGridMeshType, int> numberOfMeshByType;
        private int numberOfNetworks = -1;

        /// <summary>
        /// Constructor for netcdf (ugrid) file operations.
        /// Instantiates objects which can be used for netcdf (ugrid) io interaction and file validation.
        /// </summary>
        /// <param name="path">Location of the file to do IO operations on</param>
        public UGridFile(string path)
        {
            UgridFileInfo = new UgridFileInfo(path);
            Api = CreateUGridApi();
            metaData = new FileMetaData(null, "custom", "unknown");
        }

        // Managed resource (.NET object that implements IDisposable)
        /// <summary>
        /// The <see cref="IUGridApi"/> which can communicate with the netcdf file. 
        /// </summary>
        /// <remarks>Should also handle non Ugrid type files.</remarks>
        public IUGridApi Api { get; set; }

        /// <summary>
        /// The Object which can validate the provided file.
        /// </summary>
        public IUgridFileInfo UgridFileInfo { get; set; }

        #region Reading miscellaneous (Z - (NoData) values, coordinate reference system)

        /// <summary>
        /// Reads the Z values of the first <see cref="UnstructuredGrid"/> (2d mesh) in the file.
        /// </summary>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <returns>Z values of the first 2d mesh or empty double array.</returns>
        public double[] ReadZValues(BedLevelLocation location, ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("reading z-values");

            if (!UgridFileInfo.IsValidPath())
            {
                logHandler.ReportWarningFormat(Resources.Could_not_read_file_0_properly_it_doesnt_exist, UgridFileInfo.Path);
                logHandler.LogReport();
                return Array.Empty<double>();
            }

            if (!OpenFileApi(logHandler))
            {
                return Array.Empty<double>();
            }

            try
            {
                if (!Api.IsUGridFile())
                {
                    logHandler.ReportWarningFormat(Resources.ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, UgridFileInfo.Path);
                    logHandler.LogReport();
                    return Array.Empty<double>();
                }

                int[] meshIds2d = Api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                if (meshIds2d.Length == 0)
                {
                    logHandler.ReportWarningFormat(Resources.ReadZValues_No_2D_mesh_found_in_file__0, UgridFileInfo.Path);
                    logHandler.LogReport();
                    return Array.Empty<double>();
                }

                GridLocationType locationType = GetLocationType(location, logHandler);
                if (locationType == GridLocationType.None)
                {
                    logHandler.ReportWarningFormat(Resources.ReadZValues_The_expected_location_type__0__is_not_found_in_file__1, locationType, UgridFileInfo.Path);
                    logHandler.LogReport();
                    return Array.Empty<double>();
                }

                string variableName = GetVariableName(location);
                double[] readZValues = Api.GetVariableValues(variableName, meshIds2d[0], locationType);
                logHandler.LogReport();
                return readZValues;
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return Array.Empty<double>();
            }
        }

        /// <summary>
        /// Reads the no data value used for the Z values of the first <see cref="UnstructuredGrid"/>
        /// </summary>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <returns>No data value used for Z values of the first 2d mesh, return <c>DEFAULT_NO_DATA_VALUE</c></returns>
        public double GetZCoordinateNoDataValue(BedLevelLocation location, ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("reading z-values");

            if (!UgridFileInfo.IsValidPath())
            {
                return DEFAULT_NO_DATA_VALUE;
            }

            if (!OpenFileApi(logHandler))
            {
                return DEFAULT_NO_DATA_VALUE;
            }

            try
            {
                if (!Api.IsUGridFile())
                {
                    return DEFAULT_NO_DATA_VALUE;
                }

                int[] meshIds2d = Api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                if (meshIds2d.Length == 0)
                {
                    return DEFAULT_NO_DATA_VALUE;
                }

                GridLocationType locationType = GetLocationType(location, logHandler);
                if (locationType == GridLocationType.None)
                {
                    return DEFAULT_NO_DATA_VALUE;
                }

                string variableName = GetVariableName(location);
                double zCoordinateNoDataValue = Api.GetVariableNoDataValue(variableName, meshIds2d[0], locationType);
                logHandler.LogReport();
                return zCoordinateNoDataValue;
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return DEFAULT_NO_DATA_VALUE;
            }
        }

        /// <summary>
        /// Reads the coordinate system used by the UGrid file
        /// </summary>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <returns>The read coordinate system, returns null when not possible.</returns>
        public ICoordinateSystem ReadCoordinateSystem(ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("reading coordinate system");
            if (!UgridFileInfo.IsValidPath())
            {
                return null;
            }

            if (!OpenFileApi(logHandler))
            {
                return null;
            }

            try
            {
                if (Api.IsUGridFile())
                {
                    return GetCoordinateSystemFromApi(logHandler);
                }
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                return null;
            }
            finally
            {
                logHandler.LogReport();
            }

            return NetFile.ReadCoordinateSystem(UgridFileInfo.Path);
        }

        /// <summary>
        /// Use the <see cref="IUGridApi"/> to retrieve the coordinate system from the netcdf file.
        /// </summary>
        /// <param name="logHandler">Logging handler to report problems while reading and handling IoNetCdfNativeError exceptions.</param>
        /// <returns>Coordinate system in the (ugrid type netcdf) file.</returns>
        private ICoordinateSystem GetCoordinateSystemFromApi(ILogHandler logHandler)
        {
            int epsgCode;
            try
            {
                epsgCode = Api.GetCoordinateSystemCode();
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                return null;
            }

            return epsgCode > 0
                       ? new OgrCoordinateSystemFactory().CreateFromEPSG(epsgCode)
                       : null;
        }

        #endregion

        #region writing miscellaneous (Z-Values, coordinate reference system)

        /// <summary>
        /// Writes the Z values for the first <see cref="UnstructuredGrid"/> (2d mesh)
        /// </summary>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <param name="values">Z values to write</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        public void WriteZValues(BedLevelLocation location, double[] values, ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("Writing z-values");

            if (!OpenFileApi(logHandler, OpenMode.Appending))
            {
                return;
            }

            try
            {
                if (Api.IsUGridFile())
                {
                    WriteZValuesWithApi(location, values, logHandler);
                    return;
                }
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarningFormat($"Api write z-values error ({e.ErrorCode}) : {e.ErrorMessage}, in method {e.NativeFunctionName}.");
                return;
            }
            finally
            {
                logHandler.LogReport();
            }

            NetFile.WriteZValues(UgridFileInfo.Path, values);
        }

        /// <summary>
        /// Writing z- values with ugrid api via <see cref="IUGridApi"/>.
        /// </summary>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <param name="values">Z values to write</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        private void WriteZValuesWithApi(BedLevelLocation location, double[] values, ILogHandler logHandler)
        {
            int[] meshIds2d = Api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
            if (meshIds2d.Length == 0)
            {
                logHandler.ReportWarning(string.Format(Resources.WriteZValuesWithApi_Unable_to_write_z_values_to_file____0____no_2d_mesh_found, UgridFileInfo.Path));
                return;
            }

            GridLocationType locationType = GetLocationType(location, logHandler);
            if (locationType == GridLocationType.None)
            {
                return;
            }

            string variableName = GetVariableName(location);
            string longName;

            try
            {
                longName = GetVariableLongName(location);
            }
            catch (ArgumentOutOfRangeException e)
            {
                logHandler.ReportError($"Could not write z-values because long name location value = {e.ActualValue} which throws: {e.Message}");
                return;
            }

            string unit = UGridConstants.Naming.Meter;
            string standardName = UGridConstants.Naming.Altitude;

            try
            {
                Api.SetVariableValues(variableName, standardName, longName, unit, meshIds2d[0], locationType, values);
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
            }
        }

        /// <summary>
        /// Writes the <paramref name="coordinateSystem"/> to the UGrid file using old implementation because
        /// api.SetCoordinateSystem is not yet implemented.
        /// </summary>
        /// <param name="coordinateSystem">Coordinate system to write</param>
        public void WriteCoordinateSystem(ICoordinateSystem coordinateSystem)
        {
            if (!IsUGridFile())
            {
                NetFile.WriteCoordinateSystem(UgridFileInfo.Path, coordinateSystem);
                return;
            }

            NetCdfFile file = NetCdfFile.OpenExisting(UgridFileInfo.Path, true);
            try
            {
                file.ReDefine();
                var variableName = "projected_coordinate_system";
                NetCdfVariable ncVariable = file.GetVariableByName(variableName) ?? file.AddVariable(variableName, NetCdfDataType.NcInteger, Array.Empty<NetCdfDimension>());

                file.AddAttribute(ncVariable, new NetCdfAttribute("name", coordinateSystem.Name));
                file.AddAttribute(ncVariable, new NetCdfAttribute("epsg", (int)coordinateSystem.AuthorityCode));
                file.AddAttribute(ncVariable, new NetCdfAttribute("grid_mapping_name", coordinateSystem.IsGeographic ? "latitude_longitude" : "Unknown projected"));
                file.AddAttribute(ncVariable, new NetCdfAttribute("longitude_of_prime_meridian", 0.0));
                file.AddAttribute(ncVariable, new NetCdfAttribute("semi_major_axis", coordinateSystem.GetSemiMajor()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("semi_minor_axis", coordinateSystem.GetSemiMinor()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("inverse_flattening", coordinateSystem.GetInverseFlattening()));
                file.AddAttribute(ncVariable, new NetCdfAttribute("proj4_params", coordinateSystem.PROJ4));
                file.AddAttribute(ncVariable, new NetCdfAttribute("EPSG_code", $"EPSG:{coordinateSystem.AuthorityCode}"));
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
        /// Updates the Node z variable using plain netcdf calls.
        /// </summary>
        /// <remarks>
        /// When updating the coordinate system in a UGrid file
        /// we must also update the grid-mapping attribute of only the node_Z variable
        /// to refer to the coordinate system.
        /// This is because the CF commission is locked down on the node_x and node_y variables
        /// and we can't set the grid_mapping attribute on these.
        /// </remarks>
        /// <param name="file">The netcdf file where we want to update.</param>
        private static void UpdateNodeZVariables(NetCdfFile file)
        {
            file.ReDefine();

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


        #endregion

        #region reading

        /// <summary>
        /// Reads the ugrid file data into our D-FLOW FM model.
        /// </summary>
        /// <param name="convertedUgridFileObjects">
        /// Object with the Grid, Network, Discretization and Links1D2D objects used in our
        /// DOM. This will be correctly filled
        /// </param>
        /// <param name="loadFlowLinksAndCells">
        /// Also read flow links and cell information (Applies to NetFile only).
        /// With a UGrid file the cell information is always read but not the flow links.
        /// The default value is <c>false</c>.
        /// </param>
        /// <param name="recreateCells">Recreates the cell information by calling FindCells instead of reading it from file.
        /// The default value is <c>true</c>.</param>
        /// <param name="forceCustomLengths">
        /// Force all branches in the network to have custom lengths and use the lengths that are
        /// read from file.
        /// The default value is <c>false</c>.
        /// </param>
        /// <param name="logHandler">Optional logger to write reading problems into. The default value is <c>null</c>, a custom one will be created if not provided.</param>
        /// <param name="reportProgress">Optional action to report user feedback to. The default value is <c>null</c>, a custom one will be created if not provided.</param>
        public void ReadNetFileDataIntoModel(IConvertedUgridFileObjects convertedUgridFileObjects,
                                             bool loadFlowLinksAndCells = false,
                                             bool recreateCells = true,
                                             bool forceCustomLengths = false,
                                             ILogHandler logHandler = null,
                                             Action<string> reportProgress =null)
        {
            logHandler = logHandler ?? new LogHandler("Read NetFile data into Model");
            reportProgress = reportProgress ?? Console.WriteLine;

            convertedUgridFileObjects.CompartmentProperties = convertedUgridFileObjects.CompartmentProperties ?? Enumerable.Empty<CompartmentProperties>();
            convertedUgridFileObjects.BranchProperties = convertedUgridFileObjects.BranchProperties ?? Enumerable.Empty<BranchProperties>();

            DisposableNetworkGeometry networkGeometry = ApplyNetworkGeometry(convertedUgridFileObjects.HydroNetwork, convertedUgridFileObjects.CompartmentProperties, convertedUgridFileObjects.BranchProperties, forceCustomLengths, logHandler, reportProgress);
            Disposable1DMeshGeometry mesh1d = ApplyMesh1D(convertedUgridFileObjects.Discretization, convertedUgridFileObjects.HydroNetwork, logHandler, reportProgress);

            Disposable2DMeshGeometry mesh2d = null;
            if (convertedUgridFileObjects.Grid != null)
            {
                mesh2d = ApplyMesh2D(convertedUgridFileObjects.Grid, logHandler, reportProgress, loadFlowLinksAndCells, recreateCells);
            }

            if (mesh2d != null
                && convertedUgridFileObjects.Discretization != null
                && mesh1d != null
                && networkGeometry != null)
            {
                Apply1D2DLinks(convertedUgridFileObjects, networkGeometry, mesh1d, mesh2d, logHandler, reportProgress);
            }

            logHandler.LogReport();
        }

        private void Apply1D2DLinks(IConvertedUgridFileObjects convertedUgridFileObjects, DisposableNetworkGeometry networkGeometry, Disposable1DMeshGeometry mesh1d, Disposable2DMeshGeometry mesh2d, ILogHandler logHandler, Action<string> reportProgress)
        {
            ReportProgressWithException(Resources.Apply1D2DLinks_Reading_grid_step_3_of_4, logHandler, reportProgress);

            var generatedObjectsForLinks = new GeneratedObjectsForLinks
            {
                Grid = convertedUgridFileObjects.Grid,
                Mesh2d = mesh2d,
                Discretization = convertedUgridFileObjects.Discretization,
                Mesh1d = mesh1d,
                NetworkGeometry = networkGeometry,
                Links1D2D = convertedUgridFileObjects.Links1D2D
            };

            if (!UgridFileInfo.IsValidPath())
            {
                generatedObjectsForLinks.Links1D2D.Clear();
                return;
            }

            try
            {
                Read1D2DLinks(generatedObjectsForLinks, logHandler);
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
            }
            catch (ArgumentNullException e)
            {
                logHandler.ReportError($"Null reference exception (in {e.ParamName}), 1D2D data read from api failed because it is not available, error message : {e.Message}.");
            }
            finally
            {
                logHandler.LogReport();
            }
        }

        #endregion

        #region helpers
        /// <summary>
        /// Returns if this netcdf file which is provided is a UGrid typed file.
        /// </summary>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <returns>If is a UGrid type file return true, if not false.</returns>
        public bool IsUGridFile(ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("Checking if netcdf file is of ugrid type");
            if (!UgridFileInfo.IsValidPath())
            {
                return false;
            }

            if (lastCheckedUGridPath == UgridFileInfo.Path)
            {
                return lastCheckedUGridPathResult;
            }

            lastCheckedUGridPath = UgridFileInfo.Path;

            if (!OpenFileApi(logHandler))
            {
                return false;
            }

            try
            {
                lastCheckedUGridPathResult = Api.IsUGridFile();
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return false;
            }

            return lastCheckedUGridPathResult;
        }

        /// <summary>
        /// Checks if X & Y coordinates are available of the provided type location <see cref="GridLocationType"/> on the ugrid
        /// entity of type <see cref="UGridMeshType"/>.
        /// Using plain netcdf implementation because api.getvar is not yet implemented in ugrid api.
        /// </summary>
        /// <param name="meshType">The ugrid mesh type (for example : mesh1d, mesh2d) (<see cref="UGridMeshType"/>)</param>
        /// <param name="locationType">Type of location on the mesh (node, edge etc.) (<see cref="GridLocationType"/>)</param>
        /// <param name="logHandler">Logging handler to report problems while reading and handling IoNetCdfNativeError exceptions.</param>
        /// <returns>
        /// True if X & Y coordinates are available of the provided type location <see cref="GridLocationType"/> on the
        /// ugrid entity of type <see cref="UGridMeshType"/>
        /// </returns>
        private bool CanUseXYCoordinatePossibilitiesOfGridTypeOnLocation(UGridMeshType meshType, GridLocationType locationType, ILogHandler logHandler)
        {
            if (!IsUGridFile(logHandler))
            {
                return false;
            }

            NetCdfFile file = NetCdfFile.OpenExisting(UgridFileInfo.Path);
            try
            {
                IEnumerable<NetCdfVariable> variablesWithCfRoleAndTopologyDimension = file.GetVariables().Where(v =>
                                                                                                                    file.GetAttributeValue(v, "cf_role") != null && file.GetAttributeValue(v, "topology_dimension") != null);
                switch (meshType)
                {
                    case UGridMeshType.Combined:
                        break;
                    case UGridMeshType.Mesh1D:
                        //search correct variable on long name... this should probably be improved in the kernel....
                        NetCdfVariable mesh1d = variablesWithCfRoleAndTopologyDimension.SingleOrDefault(v =>
                        {
                            string longName = file.GetAttributeValue(v, "long_name");
                            return longName.IndexOf("1D", StringComparison.InvariantCultureIgnoreCase) >= 0
                                   && longName.IndexOf("Mesh", StringComparison.InvariantCultureIgnoreCase) >= 0;
                        });
                        if (mesh1d == null)
                        {
                            return false;
                        }

                        switch (locationType)
                        {
                            case GridLocationType.None:
                                break;
                            case GridLocationType.Node:
                                string mesh1DNodeCoordinates = file.GetAttributeValue(mesh1d, "node_coordinates");
                                return mesh1DNodeCoordinates != null
                                       && mesh1DNodeCoordinates.IndexOf("node_x", StringComparison.InvariantCultureIgnoreCase) >= 0
                                       && mesh1DNodeCoordinates.IndexOf("node_y", StringComparison.InvariantCultureIgnoreCase) >= 0;
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
        /// Returns the grid location type of the z value transformed from <see cref="BedLevelLocation"/>.
        /// </summary>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <param name="logHandler">Optional logging handler to report problems while reading.</param>
        /// <returns><see cref="GridLocationType"/></returns>
        private static GridLocationType GetLocationType(BedLevelLocation location, ILogHandler logHandler)
        {
            switch (location)
            {
                case BedLevelLocation.Faces:
                case BedLevelLocation.FacesMeanLevFromNodes:
                    return GridLocationType.Face;
                case BedLevelLocation.CellEdges:
                    logHandler.ReportWarning(Resources.ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);
                    return GridLocationType.None;
                case BedLevelLocation.NodesMeanLev:
                case BedLevelLocation.NodesMinLev:
                case BedLevelLocation.NodesMaxLev:
                    return GridLocationType.Node;
                default:
                    logHandler.ReportWarning(string.Format(Resources.UGridFile_GetLocationType_Unsupported_bed_level_location___0_, location));
                    return GridLocationType.None;
            }
        }

        /// <summary>
        /// Returns default variable name attribute value for where to find or store bed level locations
        /// </summary>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <returns>
        /// The string value of the variable name in the netcdf file which matches the bed level (
        /// <see cref="BedLevelLocation"/>) location
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">This is a switch case based on <see cref="BedLevelLocation"/> but still you can provide an invalid enum value which will throw this exception.</exception>
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
                    throw new ArgumentOutOfRangeException(nameof(location), location, string.Format(Resources.UGridFile_GetVariableName_This_location_is_not_available_in_enum____0_, nameof(BedLevelLocation)));
            }
        }

        /// <summary>
        /// Returns default variable long name attribute value for where to find or store bed level locations
        /// </summary>
        /// <param name="location">Location of the Z values on the grid</param>
        /// <returns>
        /// The string value of the variable name in the netcdf file which matches the bed level (
        /// <see cref="BedLevelLocation"/>) location
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">This is a switch case based on <see cref="BedLevelLocation"/> but still you can provide an invalid enum value which will throw this exception.</exception>
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
                    throw new ArgumentOutOfRangeException(nameof(location), location, string.Format(Resources.UGridFile_GetVariableName_This_location_is_not_available_in_enum____0_, nameof(BedLevelLocation)));
            }
        }

        /// <summary>
        /// When instantiating <see cref="UGridFile"/> this will be called to set an implementation for <see cref="IUGridApi"/>
        /// interfacing.
        /// </summary>
        /// <returns>An implementation for <see cref="IUGridApi"/> interfacing</returns>
        private static IUGridApi CreateUGridApi()
        {
            return new RemoteUGridApi();
            //use new UGridApi() to disable remoting
        }

        /// <summary>
        /// Handles the file opening and catches the exceptions which will be logged in the <see cref="ILogHandler"/>.
        /// </summary>
        /// <param name="logHandler">Logging handler to report problems while opening and handling IoNetCdfNativeError exceptions.</param>
        /// <param name="openMode">The way you want to open the file with the <see cref="IUGridApi"/>, see <see cref="OpenMode"/>.</param>
        /// <returns>True if opening file was successful.</returns>
        private bool OpenFileApi(ILogHandler logHandler, OpenMode openMode = OpenMode.Reading)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            try
            {
                Api.Open(UgridFileInfo.Path, openMode);
            }
            catch (FileNotFoundException ex)
            {
                logHandler.ReportWarningFormat(Resources.Could_not_read_file_0_properly_it_doesnt_exist, UgridFileInfo.Path + ex.Message);
                logHandler.LogReport();
                return false;
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return false;
            }

            return true;
        }
        #endregion

        #region reading network geometry

        /// <summary>
        /// Reads <see cref="DisposableNetworkGeometry"/> in the UGrid file and applies it to the provided <see cref="IHydroNetwork"/>.
        /// When applies, read <see cref="ICoordinateSystem"/> from file and apply + update where needed in <see cref="IHydroNetwork"/>.
        /// </summary>
        /// <param name="network">Instance of a <see cref="IHydroNetwork"/> to add the newly read data to</param>
        /// <param name="compartmentProperties">Additional compartment properties</param>
        /// <param name="branchProperties">Additional branch properties</param>
        /// <param name="forceCustomLengths">
        /// Force all branches in the network to have custom lengths and use the lengths that are read from file.
        /// Used in output file visualization.
        /// You want to use the length defined in 'network_edge_length' variable which is used by the kernel to calculate with
        /// instead of our calculated branch lengths.
        /// </param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <param name="reportProgress">Action to report user feedback to.</param>
        /// <returns><see cref="DisposableNetworkGeometry"/> which is applies to the <see cref="IHydroNetwork"/> or null of none found in the provided file.</returns>
        public DisposableNetworkGeometry ApplyNetworkGeometry(IHydroNetwork network,
                                                              IEnumerable<CompartmentProperties> compartmentProperties,
                                                              IEnumerable<BranchProperties> branchProperties,
                                                              bool forceCustomLengths = false,
                                                              ILogHandler logHandler = null,
                                                              Action<string> reportProgress = null)
        {
            Ensure.NotNull(compartmentProperties, nameof(compartmentProperties));
            Ensure.NotNull(branchProperties, nameof(branchProperties));

            logHandler = logHandler ?? new LogHandler("reading network geometry");
            reportProgress = reportProgress ?? Console.WriteLine;

            string errorMessage = string.Format(Resources.ApplyNetworkGeometry_Could_not_load_network_from__0_, UgridFileInfo.Path);
            if (network == null || !UgridFileInfo.IsValidPath())
            {
                logHandler.ReportError(errorMessage);
                logHandler.LogReport();
                return null;
            }

            DisposableNetworkGeometry networkGeometry = null;

            if (!OpenFileApi(logHandler))
            {
                return null;
            }

            if (!Api.IsUGridFile())
            {
                logHandler.ReportError(errorMessage);
                logHandler.LogReport();
                return null;
            }

            ReportProgressWithException(Resources.ApplyNetworkGeometry_Reading_grid_step_1_of_4, logHandler, reportProgress);
            
            networkGeometry = ReadNetwork(network, compartmentProperties, branchProperties, logHandler, forceCustomLengths);

            network.CoordinateSystem = GetCoordinateSystemFromApi(logHandler);
            network.UpdateGeodeticDistancesOfChannels();
            UpdateBranchLengthIfNotMatchesFileBranchLength(network, networkGeometry);
            logHandler.LogReport();
            return networkGeometry;
        }

        /// <summary>
        /// If after reading in the network geometry and setting the coordinate system provided in the file
        /// the branch length in the HydroNetwork is not equal to the branch length stored in 'network'_edge_length
        /// set the branch to the length of the file and make the branch of a custom length.
        /// </summary>
        /// <param name="network">
        /// Instance of a <see cref="IHydroNetwork"/> in which branch lengths
        /// are calculated from geometry, geodetic length via the provided valid coordinate reference system or a user defined
        /// custom length
        /// </param>
        /// <param name="networkGeometry">The network geometry from the file containing the branch length.</param>
        private static void UpdateBranchLengthIfNotMatchesFileBranchLength(IHydroNetwork network, DisposableNetworkGeometry networkGeometry)
        {
            network.Branches.ForEach((branch, branchIndex) =>
            {
                if (!branch.IsLengthCustom
                    && Math.Abs(branch.Length - networkGeometry.BranchLengths[branchIndex]) > 1.0e-4)
                {
                    branch.Length = 0;
                    branch.Length = networkGeometry.BranchLengths[branchIndex];
                    branch.IsLengthCustom = true;
                }
            });
        }

        /// <summary>
        /// Reading network geometry <see cref="DisposableNetworkGeometry"/> in the UGrid file and applies it to the provided <see cref="IHydroNetwork"/>.
        /// </summary>
        /// <param name="network">Instance of a <see cref="IHydroNetwork"/> to add the newly read data to</param>
        /// <param name="compartmentPropertiesList">Additional compartment properties</param>
        /// <param name="branchPropertiesList">Additional branch properties</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <param name="forceCustomLengths">
        /// Force all branches in the network to have custom lengths and use the lengths that are read from file.
        /// Used in output file visualization.
        /// You want to use the length defined in 'network_edge_length' variable which is used by the kernel to calculate with
        /// instead of our calculated branch lengths.
        /// </param>
        /// <returns>A <see cref="DisposableNetworkGeometry"/> or null if fails.</returns>
        private DisposableNetworkGeometry ReadNetwork(IHydroNetwork network,
                                                      IEnumerable<CompartmentProperties> compartmentPropertiesList,
                                                      IEnumerable<BranchProperties> branchPropertiesList,
                                                      ILogHandler logHandler,
                                                      bool forceCustomLengths = false)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            try
            {
                int[] networkIds = Api.GetNetworkIds();
                if (networkIds.Length == 0)
                {
                    logHandler.ReportError(Resources.ReadNetwork_No_network_geometries_in_file_detected);
                    return null;
                }

                try
                {
                    DisposableNetworkGeometry networkGeometry = Api.GetNetworkGeometry(networkIds[0]);
                    if (networkGeometry == null)
                    {
                        return null;
                    }
                    var uGridFileHelperNetworkGeometry = new UGridFileHelperNetworkGeometry();
                    uGridFileHelperNetworkGeometry.SetNetworkGeometry(network, networkGeometry, branchPropertiesList, compartmentPropertiesList, forceCustomLengths);
                    return networkGeometry;
                }
                catch (ArgumentNullException e)
                {
                    logHandler.ReportError(string.Format(Resources.ReadNetwork_No_network_geometry_retrieved_from_file__arg_null_exception, e.ParamName, e.Message));
                    return null;
                }
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                return null;
            }
        }

        /// <summary>
        /// Gets the number of 1d network geometries (<see cref="DisposableNetworkGeometry"/>) in the UGrid file
        /// </summary>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <returns>The number of 1d networks in the UGrid file</returns>
        public int GetNumberOfNetworks(ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("reading nr of networks");

            if (!UgridFileInfo.IsValidPath())
            {
                return 0;
            }

            if (lastCheckedUGridPath == UgridFileInfo.Path && numberOfNetworks >= 0)
            {
                return numberOfNetworks;
            }

            if (!OpenFileApi(logHandler))
            {
                return 0;
            }

            try
            {
                numberOfNetworks = Api.GetNumberOfNetworks();
                return numberOfNetworks;
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return 0;
            }
        }
        #endregion

        #region reading mesh1d

        /// <summary>
        /// Reads the <see cref="IDiscretization"/> in the UGrid file
        /// </summary>
        /// <param name="discretization">Instance of a <see cref="IDiscretization"/> to clear and fill with newly read data.</param>
        /// <param name="network">Instance of a <see cref="IHydroNetwork"/> to add the newly read data to.</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <param name="reportProgress">Action to report user feedback to.</param>
        /// <returns>The <see cref="Disposable1DMeshGeometry"/> discretization of the 1D network or null.</returns>
        public Disposable1DMeshGeometry ApplyMesh1D(IDiscretization discretization,
                                                    IHydroNetwork network,
                                                    ILogHandler logHandler,
                                                    Action<string> reportProgress)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            reportProgress = reportProgress ?? Console.WriteLine;

            string errorMessage = string.Format(Resources.UGridFile_ApplyMesh1D_Could_not_load_computational_grid_from__0_, UgridFileInfo.Path);
            if (discretization == null || !UgridFileInfo.IsValidPath())
            {
                logHandler.ReportError(errorMessage);
                logHandler.LogReport();
                return null;
            }

            // check if can use x/y or get coordinate via branch/chainage
            bool canUseXyForMesh1DNodeCoordinates = CanUseXYCoordinatePossibilitiesOfGridTypeOnLocation(UGridMeshType.Mesh1D, GridLocationType.Node, logHandler);

            if (!OpenFileApi(logHandler))
            {
                return null;
            }

            try
            {
                if (!Api.IsUGridFile())
                {
                    logHandler.ReportError(errorMessage);
                    return null;
                }

                ReportProgressWithException(Resources.ApplyMesh1D_Reading_grid_step_2_of_4, logHandler, reportProgress);

                Disposable1DMeshGeometry mesh1d = Read1DMesh(discretization, network, canUseXyForMesh1DNodeCoordinates, logHandler);

                return mesh1d;
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                return null;
            }
            finally
            {
                logHandler.LogReport();
            }
        }

        private static void ReportProgressWithException(string progressMessage, ILogHandler logHandler, Action<string> reportProgress)
        {
            try
            {
                reportProgress?.Invoke(progressMessage);
            }
            catch (Exception ex)
            {
                logHandler?.ReportWarning(string.Format(Resources.ReportProgressWithException_Could_not_report_progress_because, ex.Message));
            }
        }

        /// <summary>
        /// Used the <see cref="IUGridApi"/> to retrieve <see cref="Disposable1DMeshGeometry"/>
        /// and apply the values in the provided <see cref="IDiscretization"/> using <see cref="IHydroNetwork"/> to set for branch
        /// id, chainage.
        /// </summary>
        /// <param name="discretization">Instance of a <see cref="IDiscretization"/> to clear and fill with newly read data.</param>
        /// <param name="network">Instance of a <see cref="IHydroNetwork"/> to add the newly read data to.</param>
        /// <param name="canUseXyForMesh1DNodeCoordinates">
        /// Boolean which states if in the file also the X & Y values are available
        /// for the the network locations.
        /// </param>
        /// <param name="logHandler">Logging handler to report problems while reading and handling IoNetCdfNativeError exceptions.</param>
        /// <returns>
        /// The <see cref="Disposable1DMeshGeometry"/> discretization of the 1D network or null if <see cref="IUGridApi"/>
        /// cannot read from file.
        /// </returns>
        private Disposable1DMeshGeometry Read1DMesh(IDiscretization discretization, IHydroNetwork network, bool canUseXyForMesh1DNodeCoordinates, ILogHandler logHandler)
        {
            try
            {
                int[] meshIds = Api.GetMeshIdsByMeshType(UGridMeshType.Mesh1D);
                if (meshIds.Length == 0)
                {
                    return null;
                }

                Disposable1DMeshGeometry mesh1d = Api.GetMesh1D(meshIds[0]);
                if (mesh1d == null)
                {
                    return null;
                }

                UGridFileHelperMesh1D.SetMesh1DGeometry(discretization, mesh1d, network, logHandler, canUseXyForMesh1DNodeCoordinates);
                return mesh1d;
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return null;
            }
        }

        /// <summary>
        /// Gets the number of discretizations in the UGrid file
        /// </summary>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        /// <returns>The number of discretizations in the UGrid file</returns>
        public int GetNumberOfNetworkDiscretizations(ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("reading number of mesh 1d");
            if (!UgridFileInfo.IsValidPath())
            {
                return 0;
            }

            if (numberOfMeshByType == null)
            {
                numberOfMeshByType = new Dictionary<UGridMeshType, int>();
            }

            if (lastCheckedUGridPath == UgridFileInfo.Path && numberOfMeshByType.TryGetValue(UGridMeshType.Mesh1D, out int nrOf1DMesh))
            {
                return nrOf1DMesh;
            }

            if (!OpenFileApi(logHandler))
            {
                return 0;
            }

            try
            {
                numberOfMeshByType[UGridMeshType.Mesh1D] = Api.GetNumberOfMeshByType(UGridMeshType.Mesh1D);
                return numberOfMeshByType[UGridMeshType.Mesh1D];
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return 0;
            }
            catch (ArgumentNullException e)
            {
                logHandler.ReportError(string.Format(Resources.Null_reference_exception_log, e.ParamName, e.Message));
                logHandler.LogReport();
                return 0;
            }
            catch (KeyNotFoundException e)
            {
                logHandler.ReportError(string.Format(Resources.GridFile_GetNumberOfNetworkDiscretizations_Key__0__not_found_in_dictionary_numberOfMeshByType, UGridMeshType.Mesh1D, e.Message));
                logHandler.LogReport();
                return 0;
            }
        }
        #endregion

        #region reading mesh2d
        /// <summary>
        /// Reads the first <see cref="Disposable2DMeshGeometry"/> (or reads plain 2d grid from NetFile) and applies the data to the provided <see cref="UnstructuredGrid"/> in the UGrid file.
        /// </summary>
        /// <param name="grid">Grid instance to fill.</param>
        /// <param name="loadFlowLinksAndCells">
        /// Also read flow links and cell information (Applies to NetFile only).
        /// With a UGrid file the cell information is always read but not the flow links
        /// </param>
        /// <param name="recreateCells">Recreates the cell information by calling FindCells instead of reading it from file</param>
        public void SetUnstructuredGrid(UnstructuredGrid grid, bool loadFlowLinksAndCells = false, bool recreateCells = true)
        {
            ILogHandler logHandler = new LogHandler("Reading 2D Mesh and apply on provided Grid.");
            ApplyMesh2D(grid, logHandler, loadFlowLinksAndCells: loadFlowLinksAndCells, recreateCells: recreateCells);
            logHandler.LogReport();
        }

        /// <summary>
        /// Applies read <see cref="Disposable1DMeshGeometry"/> in <see cref="UnstructuredGrid"/> of the UGrid file, or reads old /
        /// non ugrid NetFile type and place into provided <see cref="UnstructuredGrid"/>.
        /// </summary>
        /// <param name="grid">Grid instance to fill.</param>
        /// <param name="logHandler">Logging handler to report problems and handling IoNetCdfNativeError exceptions.</param>
        /// <param name="reportProgress">Action to report user feedback to.</param>
        /// <param name="loadFlowLinksAndCells">
        /// Also read flow links and cell information (Applies to NetFile only).
        /// With a UGrid file the cell information is always read but not the flow links
        /// </param>
        /// <param name="recreateCells">Recreates the cell information by calling FindCells instead of reading it from file</param>
        /// <returns><see cref="Disposable2DMeshGeometry"/> or null if none could be read.</returns>
        public Disposable2DMeshGeometry ApplyMesh2D(UnstructuredGrid grid,
                                                    ILogHandler logHandler,
                                                    Action<string> reportProgress = null,
                                                    bool loadFlowLinksAndCells = false,
                                                    bool recreateCells = true)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));

            if (grid == null || !UgridFileInfo.IsValidPath())
            {
                logHandler.ReportWarning(string.Format(Resources.ApplyMesh2D_Could_not_find_grid_file_at___0____this_is_because_you_maybe_just_created_this_model__If_this_is_not_the_case_please_check_if_the_file_with_the_grid_in_it_exists_, UgridFileInfo.Path));
                return null;
            }

            if (!OpenFileApi(logHandler))
            {
                return null;
            }

            try
            {
                if (!Api.IsUGridFile())
                {
                    logHandler.ReportWarning(string.Format(Resources.UGridFile_ApplyMesh2D_Could_not_create_mesh2d_from_file__0__because_it_is_not_a_ugrid_type_file__Trying_plain_netcdf_2d_grid_file_reading, UgridFileInfo.Path));
                    ApplyLegacyGrid(UgridFileInfo.Path, grid, loadFlowLinksAndCells);
                    Disposable2DMeshGeometry plainMesh2d = grid.CreateDisposable2DMeshGeometry();
                    return plainMesh2d;
                }

                ReportProgressWithException(Resources.Apply1D2DLinks_Reading_grid_step_3_of_4, logHandler, reportProgress);

                Disposable2DMeshGeometry mesh2d = Read2DMesh(logHandler);
                if (mesh2d == null)
                {
                    return null;
                }

                UGridFileHelperMesh2D.SetMesh2DGeometry(grid, mesh2d, recreateCells);
                ICoordinateSystem coordinateSystem = GetCoordinateSystemFromApi(logHandler);
                if (coordinateSystem != grid.CoordinateSystem)
                {
                    grid.CoordinateSystem = coordinateSystem;
                }

                return mesh2d;
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                return null;
            }
            finally
            {
                logHandler.LogReport();
            }
        }

        /// <summary>
        /// Reads the 2d mesh as <see cref="Disposable2DMeshGeometry"/> from the (ugrid netcdf) file.
        /// </summary>
        /// <param name="logHandler">Logging handler to report problems while reading and handling IoNetCdfNativeError exceptions.</param>
        /// <returns><see cref="Disposable2DMeshGeometry"/> or null if api <see cref="IUGridApi"/> could not read.</returns>
        private Disposable2DMeshGeometry Read2DMesh(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            if (!UgridFileInfo.IsValidPath())
            {
                logHandler.ReportWarning(string.Format(Resources.ApplyMesh2D_Could_not_find_grid_file_at___0____this_is_because_you_maybe_just_created_this_model__If_this_is_not_the_case_please_check_if_the_file_with_the_grid_in_it_exists_, UgridFileInfo.Path));
                return null;
            }

            if (!OpenFileApi(logHandler))
            {
                return null;
            }

            try
            {
                if (Api.IsUGridFile())
                {
                    int[] meshIds2d = Api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                    if (meshIds2d.Length == 0)
                    {
                        return null;
                    }

                    return Api.GetMesh2D(meshIds2d[0]);
                }
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                return null;
            }

            return null;
        }

        /// <summary>
        /// The file in this object is not a ugrid netcdf file type, we still want to read mesh2d using plain netcdf file reading
        /// tooling in to provided <see cref="UnstructuredGrid"/>.
        /// </summary>
        /// <param name="path">File in which non ugrid 2D mesh resides.</param>
        /// <param name="grid">The <see cref="UnstructuredGrid"/> where we want to apply the 2D mesh data into.</param>
        /// <param name="loadFlowLinksAndCells">
        /// Also read flow links and cell information (Applies to NetFile only).
        /// With a UGrid file the cell information is always read but not the flow links
        /// </param>
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
        #endregion

        #region reading 1d2d links
        /// <summary>
        /// Reads the set of 1d/2d links from the UGrid file
        /// </summary>
        /// <param name="generatedObjectsForLinks">all objects needed to generate our 1D2D links from a (ugrid) file</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while reading and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        public void Read1D2DLinks(IGeneratedObjectsForLinks generatedObjectsForLinks, ILogHandler logHandler)
        {
            Ensure.NotNull(generatedObjectsForLinks, nameof(generatedObjectsForLinks));
            Ensure.NotNull(logHandler, nameof(logHandler));

            if (!OpenFileApi(logHandler))
            {
                return;
            }

            try
            {
                if (!Api.IsUGridFile())
                {
                    logHandler.ReportError(string.Format(Resources.GeneratedObjectsForLinks_Read1D2DLinks_Could_not_load_links_from__0___This_is_not_a_UGrid_file_, UgridFileInfo.Path));
                    generatedObjectsForLinks.Links1D2D?.Clear();
                    return;
                }

                if (NeedToReadMesh2D(generatedObjectsForLinks))
                {
                    generatedObjectsForLinks.Mesh2d = Read2DMesh(logHandler);
                    if (generatedObjectsForLinks.Mesh2d != null)
                    {
                        generatedObjectsForLinks.FillValueMesh2DFaceNodes = (int)GetZCoordinateNoDataValue(BedLevelLocation.Faces, logHandler);
                    }
                }

                if (NeedToReadMesh1D(generatedObjectsForLinks))
                {
                    generatedObjectsForLinks.Mesh1d = Read1DMesh(new Discretization(), new HydroNetwork(), false, logHandler);
                }

                if (NeedToReadNetworkGeometry(generatedObjectsForLinks))
                {
                    generatedObjectsForLinks.NetworkGeometry = ReadNetwork(new HydroNetwork(), Enumerable.Empty<CompartmentProperties>(), Enumerable.Empty<BranchProperties>(), logHandler);
                }

                int linksId = Api.GetLinksId();
                if (linksId != -1) // when api return -1 no link administration could be found in the file
                {
                    generatedObjectsForLinks.LinksGeometry = Api.GetLinks(linksId);
                    if (generatedObjectsForLinks.LinksGeometry == null)
                    {
                        generatedObjectsForLinks.Links1D2D?.Clear();
                        return;
                    }
                    var helper1D2DLinks = new UGridFileHelper1D2DLinks();
                    helper1D2DLinks.SetLinks(generatedObjectsForLinks, logHandler);
                }
                else
                {
                    generatedObjectsForLinks.Links1D2D?.Clear();
                }
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
            }
            catch (NotSupportedException ex)
            {
                logHandler.ReportError(string.Format(Resources.UGridFile_Read1D2DLinks_Cannot_clear__0___list_has_no_Clear_support__error___1_, nameof(generatedObjectsForLinks.Links1D2D), ex.Message));
            }
            finally
            {
                logHandler.LogReport();
            }
        }

        private static bool NeedToReadNetworkGeometry(IGeneratedObjectsForLinks generatedObjectsForLinks)
        {
            return generatedObjectsForLinks.Discretization?.Network != null && generatedObjectsForLinks.NetworkGeometry == null;
        }

        private static bool NeedToReadMesh1D(IGeneratedObjectsForLinks generatedObjectsForLinks)
        {
            return generatedObjectsForLinks.Discretization != null && generatedObjectsForLinks.Mesh1d == null;
        }

        private static bool NeedToReadMesh2D(IGeneratedObjectsForLinks generatedObjectsForLinks)
        {
            return generatedObjectsForLinks.Grid != null && generatedObjectsForLinks.Mesh2d == null;
        }
        #endregion

        #region write mesh2d

        public void InitializeMetaData(string modelName, string pluginName, string pluginVersion)
        {
            metaData.ModelName = modelName;
            metaData.Source = pluginName ?? metaData.Source;
            metaData.Version = pluginName ?? metaData.Version;
        }

        /// <summary>
        /// Creates a new UGrid file and adds the <paramref name="grid"/>, <paramref name="network"/>,
        /// <paramref name="networkDiscretization"/>,
        /// <paramref name="links"/> and <paramref name="zValues"/>
        /// </summary>
        /// <remarks>Deletes the previous file if it exists</remarks>
        /// <param name="grid"><see cref="UnstructuredGrid"/> to store in the UGrid file</param>
        /// <param name="network"><see cref="IHydroNetwork"/> to store in the UGrid file</param>
        /// <param name="networkDiscretization"><see cref="IDiscretization"/> to store in the UGrid file</param>
        /// <param name="links"><see cref="IEnumerable{ILink1D2D}"/> to store in the UGrid file</param>
        /// <param name="location">Location of the Z values on the 2D grid</param>
        /// <param name="zValues">Z values for the 2D grid</param>
        /// <param name="logHandler">Optional logging handler to report problems and handling IoNetCdfNativeError exceptions.</param>
        public void WriteGridToFile(UnstructuredGrid grid,
                                    IHydroNetwork network,
                                    IDiscretization networkDiscretization,
                                    IEnumerable<ILink1D2D> links,
                                    BedLevelLocation location,
                                    double[] zValues,
                                    ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("Writing 1D2D model entities");

            FileUtils.DeleteIfExists(UgridFileInfo.Path);
            DisposableNetworkGeometry networkGeometry = null;
            Disposable1DMeshGeometry mesh1d = null;

            if (metaData.ModelName == null)
            {
                logHandler.ReportError("Can not create ugrid netcdf file is meta data not is properly initialized.");
                return;
            }

            try
            {
                Api.CreateFile(UgridFileInfo.Path, metaData);

                if (network?.Nodes?.Count > 0)
                {
                    networkGeometry = network.CreateDisposableNetworkGeometry();
                    int networkId = Api.WriteNetworkGeometry(networkGeometry);

                    if (networkDiscretization != null && networkDiscretization.Locations.Values.Count > 0)
                    {
                        mesh1d = networkDiscretization.CreateDisposable1DMeshGeometry(logHandler);
                        Api.WriteMesh1D(mesh1d, networkId);
                    }
                }

                if (grid == null || grid.IsEmpty)
                {
                    return;
                }

                using (Disposable2DMeshGeometry disposable2DMeshGeometry = grid.CreateDisposable2DMeshGeometry())
                {
                    Api.WriteMesh2D(disposable2DMeshGeometry);
                }

                WriteZValuesWithApi(location, zValues, logHandler);

                List<ILink1D2D> link1D2Ds = links?.ToList();
                if (link1D2Ds?.Count > 0 && mesh1d != null)
                {
                    mesh1d.ValidateMesh1DSourceLocationsOnlyExistOnce(link1D2Ds).ForEach(logHandler.ReportError);
                    using (DisposableLinksGeometry disposableLinksGeometry = link1D2Ds.CreateDisposableLinksGeometry())
                    {
                        Api.WriteLinks(disposableLinksGeometry);
                    }
                }

                mesh1d?.Dispose();
                networkGeometry?.Dispose();
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
            }
            catch (ArgumentNullException e)
            {
                logHandler.ReportError($"Null reference exception (in {e.ParamName}), error message : {e.Message}.");
                logHandler.LogReport();
            }
            finally
            {
                Api.Close();
            }

            logHandler.LogReport();
        }

        /// <summary>
        /// Overrides the existing x,y values of the first <see cref="UnstructuredGrid"/>s vertices
        /// </summary>
        /// <remarks>This function is mostly used for updating the vertices after a coordinate transformation</remarks>
        /// <param name="unstructuredGrid"><see cref="UnstructuredGrid"/> containing the new vertices values</param>
        /// <param name="logHandler">
        /// Optional logging handler to report problems while writing and handling IoNetCdfNativeError
        /// exceptions.
        /// </param>
        public void RewriteGridCoordinates(UnstructuredGrid unstructuredGrid, ILogHandler logHandler = null)
        {
            logHandler = logHandler ?? new LogHandler("Overwriting existing mesh 2D x,y values");

            if (!UgridFileInfo.IsValidPath())
            {
                return;
            }

            if (!OpenFileApi(logHandler, OpenMode.Appending))
            {
                return;
            }

            try
            {
                if (Api.IsUGridFile())
                {
                    double[] xValues = unstructuredGrid.Vertices.Select(v => v.X).ToArray();
                    double[] yValues = unstructuredGrid.Vertices.Select(v => v.Y).ToArray();

                    int[] meshIds = Api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                    if (meshIds.Length != 0)
                    {
                        Api.ResetMeshVerticesCoordinates(meshIds[0], xValues, yValues);
                    }

                    return;
                }
            }
            catch (IoNetCdfNativeError e)
            {
                logHandler.ReportWarning(string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, e.ErrorCode, e.ErrorMessage, e.NativeFunctionName));
                logHandler.LogReport();
                return;
            }
            catch (ArgumentNullException e)
            {
                logHandler.ReportError(string.Format(Resources.Null_reference_exception_log, e.ParamName, e.Message));
                logHandler.LogReport();
                return;
            }
            catch (ArgumentException e)
            {
                logHandler.ReportError($"Could not reset mesh vertices coordinates, error message : {e.Message}.");
                logHandler.LogReport();
                return;
            }
            NetFile.RewriteGridCoordinates(UgridFileInfo.Path, unstructuredGrid);
        }
        #endregion

        #region IDisposable pattern
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            
            // Suppress finalization to prevent the garbage collector from calling the finalizer.
            GC.SuppressFinalize(this);
        }

        // Protected virtual Dispose method to centralize cleanup logic.
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && Api != null)
                {
                    // Dispose managed resources here.
                    Api.Dispose();
                    Api = null;
                }

                disposed = true;
            }
        }

        // Destructor/Finalizer to handle the case where Dispose was not called.
        ~UGridFile()
        {
            // Finalizer calls Dispose with disposing set to false.
            Dispose(false);
        }
        #endregion
    }
}