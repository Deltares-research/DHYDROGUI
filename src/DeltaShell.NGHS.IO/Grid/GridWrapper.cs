using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridWrapper : IGridWrapper
    {
        /// <summary>
        /// Checks whether the specified data set adheres to a specific set of conventions.
        /// Datasets may adhere to multiple conventions at the same time, so use this method
        /// to check for individual conventions.
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="iconvtype">The NetCDF conventions type to check for.</param>
        /// <returns>Whether or not the file adheres to the specified conventions.</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_adheresto_conventions", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ionc_adheresto_conventions_dll(ref int ioncid, ref int iconvtype);

        /// <summary>
        /// Inquire the NetCDF conventions used in the dataset.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="iconvtype">The NetCDF conventions type of the dataset.</param>
        /// <param name="convversion"></param>
        /// <returns>Result status, ionc_noerr if successful.</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_conventions", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_inq_conventions_dll(ref int ioncid, ref int iconvtype, ref double convversion);

        /// <summary>
        /// Tries to open a NetCDF file and initialize based on its specified conventions.
        /// </summary>
        /// <param name="c_path">File name for netCDF dataset to be opened.</param>
        /// <param name="mode">NetCDF open mode, e.g. NF90_NOWRITE.</param>
        /// <param name="ioncid">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncid)%ncid.</param>
        /// <param name="iconvtype">The detected conventions in the file.</param>
        /// <param name="convversion"></param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_open", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_open_dll([In] string c_path, [In, Out] ref int mode, [In, Out] ref int ioncid, [In, Out] ref int iconvtype, ref double convversion);

        /// <summary>
        /// Tries to close an open io_netcdf data set.
        /// </summary>
        /// <param name="ioncid">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncid)%ncid.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_close", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_close_dll([In] ref int ioncid);

        #region UGRID specifics

        /// <summary>
        /// Gets the number of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="nmesh">Number of meshes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_mesh_count_dll([In, Out] ref int ioncid, [In, Out] ref int nmesh);

        /// <summary>
        /// Gets the name of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="nmesh">Mesh id.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_mesh_name_dll([In, Out] ref int ioncid, [In, Out] ref int meshid, [MarshalAs(UnmanagedType.LPStr)][In, Out] StringBuilder meshName);

        /// <summary>
        /// Gets the number of nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nnode">Number of nodes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_node_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_node_count_dll(ref int ioncid, ref int meshid, ref int nnode);

        /// <summary>
        /// Gets the number of edges in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nedge">Number of edges.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_edge_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_edge_count_dll(ref int ioncid, ref int meshid, ref int nedge);

        /// <summary>
        /// Gets the number of faces in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nface">Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_face_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_face_count_dll(ref int ioncid, ref int meshid, ref int nface);

        /// <summary>
        /// Gets the maximum number of nodes for any face in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nmaxfacenodes">The maximum number of nodes per face in the mesh.Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_max_face_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_max_face_nodes_dll(ref int ioncid, ref int meshid, ref int nmaxfacenodes);

        /// <summary>
        /// Gets the x,y coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="c_xptr">Pointer to array for x-coordinates</param>
        /// <param name="c_yptr">Pointer to array for y-coordinates</param>
        /// <param name="nnode">The number of nodes in the mesh.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_node_coordinates", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_node_coordinates_dll([In, Out] ref int ioncid, [In, Out] ref int meshid, [In, Out] ref IntPtr c_xptr, [In, Out]ref IntPtr c_yptr, [In, Out] ref int nnode);

        /// <summary>
        /// Gets the edge-node connectvit table for all edges in the specified mesh.
        /// The output edge_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="c_edge_nodes_ptr">Pointer to array for the edge-node connectivity table.</param>
        /// <param name="nedge">The number of edges in the mesh.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_edge_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_edge_nodes_dll(ref int ioncid, ref int meshid, ref IntPtr c_edge_nodes_ptr, ref int nedge);

        /// <summary>
        /// Gets the face-node connectvit table for all faces in the specified mesh.
        /// The output face_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="c_face_nodes_ptr">Pointer to array for the face-node connectivity table.</param>
        /// <param name="nface">The number of faces in the mesh.</param>
        /// <param name="nmaxfacenodes">The maximum number of nodes per face in the mesh.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_face_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_face_nodes_dll(ref int ioncid, ref int meshid, ref IntPtr c_face_nodes_ptr, ref int nface, ref int nmaxfacenodes);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_geom_ugrid", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_geom_ugrid_dll(string filename);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_map_ugrid", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_map_ugrid_dll(string filename);
        
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_coordinate_system", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_coordinate_system_dll([In] ref int ioncid, [In, Out] ref int nmesh);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_var_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_var_count_dll([In] ref int ioncid, [In] ref int mesh, [In] ref int location, [In, Out] ref int nCount);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varids", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_inq_varids_dll(ref int ioncid, ref int meshId, ref int location, ref IntPtr ptr, ref int nVar);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IO_NetCDF_Message_Callback(int level, [MarshalAs(UnmanagedType.LPStr)]string message);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IO_NetCDF_Progress_Callback([MarshalAs(UnmanagedType.LPStr)]string message, ref double progress);
    
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_initialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_initialize_dll(IO_NetCDF_Message_Callback c_message_callback, IO_NetCDF_Progress_Callback c_progress_callback);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_var", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_var_dll(ref int ioncid, ref int meshId, ref int location, string varname, ref IntPtr c_zptr, ref int nNode, ref double c_fillvalue);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_var", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_var_dll(ref int ioncid, ref int meshid, ref int iloctype, string c_varname, ref IntPtr c_values_ptr, ref int nVal);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_node_coordinates", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_node_coordinates_dll(ref int ioncid, ref int meshid, ref IntPtr c_xvalues_ptr, ref IntPtr c_yvalues_ptr, ref int nNode);

        #endregion
        #region UGRID 1D Specifics

        /// <summary>
        /// This is a structure to pass arrays of chars arrays from c# to fortran.
        /// </summary>
        public const int idssize = 30;
        public const int longnamessize = 80;
        [StructLayout(LayoutKind.Sequential)]
        public struct interop_charinfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = idssize)]
            public char[] ids;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = longnamessize)]
            public char[] longnames;
        }

        public const int metadatasize = 100;
        [StructLayout(LayoutKind.Sequential)]
        public struct interop_metadata
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = metadatasize)]
            public char[] institution;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = metadatasize)]
            public char[] source;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = metadatasize)]
            public char[] references;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = metadatasize)]
            public char[] version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = metadatasize)]
            public char[] modelname;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_add_global_attributes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_add_global_attributes_dll([In] ref int ioncid, interop_metadata metadata);

        /// <summary>
        /// This function creates a new netCDF file
        /// </summary>
        /// <param name="c_path">The path where the file will be created (in)</param>
        /// <param name="mode"> The netCDF opening mode (in)</param>
        /// <param name="ioncid">The netCDF file id (out)</param>
        /// <param name="iconvtype">The netCDF convention (in, e.g. 2 is UGRID)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_create", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_create_dll([In] string c_path, [In] ref int mode, [In, Out] ref int ioncid, [In] ref int iconvtype);

        /// <summary>
        /// Create a 1d network in an opened netCDF file  
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (out)</param>
        /// <param name="networkName">The network name (in) </param>
        /// <param name="nNodes">The number of network nodes (in) </param>
        /// <param name="nBranches">The number of network branches (in)</param>
        /// <param name="nGeometry">The number of geometry points (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_create_1d_network", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_create_1d_network_dll([In] ref int ioncid, [In, Out] ref int networkid, [In] string networkName, [In] ref int nNodes, [In] ref int nBranches, [In] ref int nGeometry);

        /// <summary>
        /// Write the coordinates of the network nodes
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_nodesX">The x coordinates of the network nodes (in)</param>
        /// <param name="c_nodesY">The y coordinates of the network nodes (in)</param>
        /// <param name="nodesinfo">The network infos (in)</param>
        /// <param name="nNodes">The number of network nodes (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_1d_network_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_1d_network_nodes_dll([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_nodesX, [In] ref IntPtr c_nodesY, interop_charinfo[] nodesinfo, [In] ref int nNodes);

        /// <summary>
        /// Write the coordinates of the network branches
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_sourcenodeid">The source node id (in)</param>
        /// <param name="c_targetnodeid">The target node id (in)</param>
        /// <param name="branchinfo">The branch info (in)</param>
        /// <param name="c_branchlengths">The branch lengths (in)</param>
        /// <param name="c_nbranchgeometrypoints">The number of geometry points in each branch (in)</param>
        /// <param name="nBranches">The number of branches (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_1d_network_branches", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_1d_network_branches_dll([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_sourcenodeid, [In] ref IntPtr c_targetnodeid, interop_charinfo[] branchinfo, [In] ref IntPtr c_branchlengths, [In] ref IntPtr c_nbranchgeometrypoints, [In] ref int nBranches);
 
        /// <summary>
        /// Writes the branch geometry (the geometry points)  
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_geopointsX">The x coordinates of the geometry points (in)</param>
        /// <param name="c_geopointsY">The y coordinates of the geometry points (in)</param>
        /// <param name="nGeometry">The number of geometry points (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_1d_network_branches_geometry", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_1d_network_branches_geometry_dll([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_geopointsX, [In] ref IntPtr c_geopointsY, [In] ref int nGeometry);

        /// <summary>
        /// Writes a 1d mesh. The geometrical features (e.g. the branches and geometry points) are described in the network above
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="meshname">The mesh name (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <param name="nmeshedges">The number of mesh edges (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_create_1d_mesh", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_create_1d_mesh_dll([In] ref int ioncid, [In] ref int networkid, string meshname, [In] ref int nmeshpoints, [In] ref int nmeshedges);

        /// <summary>
        /// Writes the mesh coordinates points 
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_branchidx">The branch id for each mesh point (in)</param>
        /// <param name="c_offset">The offset along the branch from the starting point (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_1d_mesh_discretisation_points", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_1d_mesh_discretisation_points_dll([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_branchidx, [In] ref IntPtr c_offset, [In] ref int nmeshpoints);

        /// <summary>
        /// Get the number of network nodes
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="nNodes">The number of nodes(out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_network_nodes_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_network_nodes_count_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref int nNodes);
        
        /// <summary>
        /// Get the number of branches
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="nBranches">The number of branches (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_network_branches_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_network_branches_count_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref int nBranches);

        /// <summary>
        /// Get the number of geometry points
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="ngeometrypoints">The number of geometry points (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_network_branches_geometry_coordinate_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_network_branches_geometry_coordinate_count_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref int ngeometrypoints);

        /// <summary>
        /// Read the node coordinates and the charinfo
        /// </summary>
        /// <param name="ioncid">The netCDF file id</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_nodesX">The x coordinates of the network nodes (out)</param>
        /// <param name="c_nodesY">The y coordinates of the network nodes (out)</param>
        /// <param name="nodesinfo">The network infos (out)</param>
        /// <param name="nNodes">The number of network nodes (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_read_1d_network_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_read_1d_network_nodes_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_nodesX, [In, Out] ref IntPtr c_nodesY, [In, Out]  interop_charinfo[] nodesinfo, [In] ref int nNodes);

        /// <summary>
        /// Read the coordinates of the network branches
        /// </summary>
        /// <param name="ioncid">The netCDF file id</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_sourcenodeid">The source node id (out)</param>
        /// <param name="c_targetnodeid">The target node id (out)</param>
        /// <param name="c_branchlengths">The branch lengths (out)</param>
        /// <param name="branchinfo">The branch info (out)</param>
        /// <param name="c_nbranchgeometrypoints">he number of geometry points in each branch (out)</param>
        /// <param name="nBranches">The number of branches (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_read_1d_network_branches", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_read_1d_network_branches_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_sourcenodeid, [In, Out] ref IntPtr c_targetnodeid, [In, Out] ref IntPtr c_branchlengths, [In, Out]  interop_charinfo[] branchinfo, [In, Out] ref IntPtr c_nbranchgeometrypoints, [In] ref int nBranches);

        /// <summary>
        /// Reads the branch geometry
        /// </summary>
        /// <param name="ioncid">The netCDF file id</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_geopointsX">The x coordinates of the geometry points (out)</param>
        /// <param name="c_geopointsY">The y coordinates of the geometry points (out)</param>
        /// <param name="nNodes">The number of nodes (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_read_1d_network_branches_geometry", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_read_1d_network_branches_geometry_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_geopointsX, [In, Out] ref IntPtr c_geopointsY, [In] ref int nNodes);

        /// <summary>
        /// Get the number of mesh discretization points 
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_mesh_discretisation_points_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_mesh_discretisation_points_count_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref int nmeshpoints);

        /// <summary>
        /// Read the coordinates of the mesh points  
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_branchidx">The branch id for each mesh point (out)</param>
        /// <param name="c_offset">The offset along the branch from the starting point (out)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_read_1d_mesh_discretisation_points", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_read_1d_mesh_discretisation_points_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_branchidx, [In, Out] ref IntPtr c_offset, [In] ref int nmeshpoints);
        
        #endregion

        #region Implementation of IGridWrapper

        public bool ionc_adheresto_conventions(ref int ioncid, ref int iconvtype)
        {
            return ionc_adheresto_conventions_dll(ref ioncid, ref iconvtype);
        }

        public int ionc_inq_conventions(ref int ioncid, ref int iconvtype, ref double convversion)
        {
            return ionc_inq_conventions_dll(ref ioncid, ref iconvtype, ref convversion);
        }

        public int ionc_open(string c_path, ref int mode, ref int ioncid, ref int iconvtype, ref double convversion)
        {
            return ionc_open_dll(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
        }

        public int ionc_close(ref int ioncid)
        {
            return ionc_close_dll(ref ioncid);
        }

        public int ionc_get_mesh_count(ref int ioncid, ref int nmesh)
        {
            return ionc_get_mesh_count_dll(ref ioncid, ref nmesh);
        }

        public int ionc_get_mesh_name(ref int ioncid, ref int meshid, StringBuilder meshName)
        {
            return ionc_get_mesh_name_dll(ref ioncid, ref meshid, meshName);
        }

        public int ionc_get_node_count(ref int ioncid, ref int meshid, ref int nnode)
        {
            return ionc_get_node_count_dll(ref ioncid, ref meshid, ref nnode);
        }

        public int ionc_get_edge_count(ref int ioncid, ref int meshid, ref int nedge)
        {
            return ionc_get_edge_count_dll(ref ioncid, ref meshid, ref nedge);
        }

        public int ionc_get_face_count(ref int ioncid, ref int meshid, ref int nface)
        {
            return ionc_get_face_count_dll(ref ioncid, ref meshid, ref nface);
        }

        public int ionc_get_max_face_nodes(ref int ioncid, ref int meshid, ref int nmaxfacenodes)
        {
            return ionc_get_max_face_nodes_dll(ref ioncid, ref meshid, ref nmaxfacenodes);
        }

        public int ionc_get_node_coordinates(ref int ioncid, ref int meshid, ref IntPtr c_xptr, ref IntPtr c_yptr, ref int nnode)
        {
            return ionc_get_node_coordinates_dll(ref ioncid, ref meshid, ref c_xptr, ref c_yptr, ref nnode);
        }

        public int ionc_get_edge_nodes(ref int ioncid, ref int meshid, ref IntPtr c_edge_nodes_ptr, ref int nedge)
        {
            return ionc_get_edge_nodes_dll(ref ioncid, ref meshid, ref c_edge_nodes_ptr, ref nedge);
        }

        public int ionc_get_face_nodes(ref int ioncid, ref int meshid, ref IntPtr c_face_nodes_ptr, ref int nface,
            ref int nmaxfacenodes)
        {
            return ionc_get_face_nodes_dll(ref ioncid, ref meshid, ref c_face_nodes_ptr, ref nface, ref nmaxfacenodes);
        }

        public int ionc_write_geom_ugrid(string filename)
        {
            return ionc_write_geom_ugrid_dll(filename);
        }

        public int ionc_write_map_ugrid(string filename)
        {
            return ionc_write_map_ugrid_dll(filename);
        }

        public int ionc_get_coordinate_system(ref int ioncid, ref int nmesh)
        {
            return ionc_get_coordinate_system_dll(ref ioncid, ref nmesh);
        }

        public int ionc_get_var_count(ref int ioncid, ref int mesh, ref int location, ref int nCount)
        {
            return ionc_get_var_count_dll(ref ioncid, ref mesh, ref location, ref nCount);
        }

        public int ionc_inq_varids(ref int ioncid, ref int meshId, ref int location, ref IntPtr ptr, ref int nVar)
        {
            return ionc_inq_varids_dll(ref ioncid, ref meshId, ref location, ref ptr, ref nVar);
        }

        public int ionc_initialize(IO_NetCDF_Message_Callback c_message_callback, IO_NetCDF_Progress_Callback c_progress_callback)
        {
            return ionc_initialize_dll(c_message_callback, c_progress_callback);
        }

        public int ionc_get_var(ref int ioncid, ref int meshId, ref int location, string varname, ref IntPtr c_zptr, ref int nNode, ref double c_fillvalue)
        {
            return ionc_get_var_dll(ref ioncid, ref meshId, ref location, varname, ref c_zptr, ref nNode, ref c_fillvalue);
        }

        public int ionc_put_var(ref int ioncid, ref int meshid, ref int iloctype, string c_varname, ref IntPtr c_values_ptr, ref int nVal)
        {
            return ionc_put_var_dll(ref ioncid, ref meshid, ref iloctype, c_varname, ref c_values_ptr, ref nVal);
        }

        public int ionc_put_node_coordinates(ref int ioncid, ref int meshid, ref IntPtr c_xvalues_ptr, ref IntPtr c_yvalues_ptr, ref int nNode)
        {
            return ionc_put_node_coordinates_dll(ref ioncid, ref meshid, ref c_xvalues_ptr, ref c_yvalues_ptr, ref nNode);
        }

        public int ionc_add_global_attributes(ref int ioncid, interop_metadata metadata)
        {
            return ionc_add_global_attributes_dll(ref ioncid, metadata);
        }

        public int ionc_create(string c_path, ref int mode, ref int ioncid, ref int iconvtype)
        {
            return ionc_create_dll(c_path, ref mode, ref ioncid, ref iconvtype);
        }

        public int ionc_create_1d_network(ref int ioncid, ref int networkid, string networkName, ref int nNodes, ref int nBranches, ref int nGeometry)
        {
            return ionc_create_1d_network_dll(ref ioncid, ref networkid, networkName, ref nNodes, ref nBranches, ref nGeometry);
        }

        public int ionc_write_1d_network_nodes(ref int ioncid, ref int networkid, ref IntPtr c_nodesX, ref IntPtr c_nodesY, interop_charinfo[] nodesinfo, ref int nNodes)
        {
            return ionc_write_1d_network_nodes_dll(ref ioncid, ref networkid, ref c_nodesX, ref c_nodesY, nodesinfo, ref nNodes);
        }

        public int ionc_write_1d_network_branches(ref int ioncid, ref int networkid, ref IntPtr c_sourcenodeid,
            ref IntPtr c_targetnodeid, interop_charinfo[] branchinfo, ref IntPtr c_branchlengths,
            ref IntPtr c_nbranchgeometrypoints, ref int nBranches)
        {
            return ionc_write_1d_network_branches_dll(ref ioncid, ref networkid, ref c_sourcenodeid, ref c_targetnodeid, branchinfo, ref c_branchlengths, ref c_nbranchgeometrypoints, ref nBranches);
        }

        public int ionc_write_1d_network_branches_geometry(ref int ioncid, ref int networkid, ref IntPtr c_geopointsX,
            ref IntPtr c_geopointsY, ref int nGeometry)
        {
            return ionc_write_1d_network_branches_geometry_dll(ref ioncid, ref networkid, ref c_geopointsX, ref c_geopointsY, ref nGeometry);
        }

        public int ionc_create_1d_mesh(ref int ioncid, ref int networkid, string meshname, ref int nmeshpoints, ref int nmeshedges)
        {
            return ionc_create_1d_mesh_dll(ref ioncid, ref networkid, meshname, ref nmeshpoints, ref nmeshedges);
        }

        public int ionc_write_1d_mesh_discretisation_points(ref int ioncid, ref int networkid, ref IntPtr c_branchidx,
            ref IntPtr c_offset, ref int nmeshpoints)
        {
            return ionc_write_1d_mesh_discretisation_points_dll(ref ioncid, ref networkid, ref c_branchidx, ref c_offset, ref nmeshpoints);
        }

        public int ionc_get_1d_network_nodes_count(ref int ioncid, ref int networkid, ref int nNodes)
        {
            return ionc_get_1d_network_nodes_count_dll(ref ioncid, ref networkid, ref nNodes);
        }

        public int ionc_get_1d_network_branches_count(ref int ioncid, ref int networkid, ref int nBranches)
        {
            return ionc_get_1d_network_branches_count_dll(ref ioncid, ref networkid, ref nBranches);
        }

        public int ionc_get_1d_network_branches_geometry_coordinate_count(ref int ioncid, ref int networkid, ref int ngeometrypoints)
        {
            return ionc_get_1d_network_branches_geometry_coordinate_count_dll(ref ioncid, ref networkid, ref ngeometrypoints);
        }

        public int ionc_read_1d_network_nodes(ref int ioncid, ref int networkid, ref IntPtr c_nodesX, ref IntPtr c_nodesY,
            interop_charinfo[] nodesinfo, ref int nNodes)
        {
            return ionc_read_1d_network_nodes_dll(ref ioncid, ref networkid, ref c_nodesX, ref c_nodesY,
                nodesinfo, ref nNodes);
        }

        public int ionc_read_1d_network_branches(ref int ioncid, ref int networkid, ref IntPtr c_sourcenodeid,
            ref IntPtr c_targetnodeid, ref IntPtr c_branchlengths, interop_charinfo[] branchinfo,
            ref IntPtr c_nbranchgeometrypoints, ref int nBranches)
        {
            return ionc_read_1d_network_branches_dll(ref ioncid, ref  networkid, ref c_sourcenodeid,
            ref c_targetnodeid, ref c_branchlengths, branchinfo,
                ref c_nbranchgeometrypoints, ref nBranches);
        }

        public int ionc_read_1d_network_branches_geometry(ref int ioncid, ref int networkid, ref IntPtr c_geopointsX,
            ref IntPtr c_geopointsY, ref int nNodes)
        {
            return ionc_read_1d_network_branches_geometry_dll(ref ioncid, ref networkid, ref c_geopointsX,
            ref c_geopointsY, ref nNodes);
        }

        public int ionc_get_1d_mesh_discretisation_points_count(ref int ioncid, ref int networkid, ref int nmeshpoints)
        {
            return ionc_get_1d_mesh_discretisation_points_count_dll(ref ioncid, ref networkid, ref nmeshpoints);
        }

        public int ionc_read_1d_mesh_discretisation_points(ref int ioncid, ref int networkid, ref IntPtr c_branchidx,
            ref IntPtr c_offset, ref int nmeshpoints)
        {
            return ionc_read_1d_mesh_discretisation_points_dll(ref ioncid, ref networkid, ref c_branchidx,
            ref c_offset, ref nmeshpoints);
        }

        #endregion
    }
}