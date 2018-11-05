using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridWrapper
    {
        private const int StartIndex = 0;
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

        //-Get the network names------///
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_network_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_network_name_dll([In] ref int ncidin, [In] ref int networkId, [MarshalAs(UnmanagedType.LPStr)][In, Out] StringBuilder networkName);

        #region UGRID specifics

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_def_var", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_def_var_dll(ref int ioncid, ref int meshId, ref int varId, ref int type, ref int locType, string varName, string standardName, string longName, string unit, ref double fillValue);

        /// <summary>
        /// Get the id of the geometry network.
        /// </summary>
        /// <param name="ioncid">The IONC data set id (in)</param>
        /// <param name="networkid">The geometry mesh (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_network_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_network_id_dll([In] ref int ioncid, [In, Out] ref int networkid);

        /// <summary>
        /// Get the id of the 1d computational mesh
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The 1d computational mesh id (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_mesh_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_mesh_id_dll([In] ref int ioncid, [In, Out] ref int meshId);

        /// <summary>
        /// Get the id of the 2d computational mesh
		/// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The 2d computational mesh id (out)</param>
		/// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_2d_mesh_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_2d_mesh_id_dll([In] ref int ioncid, [In, Out] ref int meshId);

        /// <summary>
        /// Get the id of the 3d computational mesh
		/// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The 3d computational mesh id (out)</param>
		/// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_3d_mesh_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_3d_mesh_id_dll([In] ref int ioncid, [In, Out] ref int meshId);

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
        /// <param name="meshId">Mesh id.</param>
        /// <param name="meshName">The mesh name.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_mesh_name_dll([In, Out] ref int ioncid, [In, Out] ref int meshId, [MarshalAs(UnmanagedType.LPStr)][In, Out] StringBuilder meshName);

        /// <summary>
        /// Gets the number of nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="nnode">Number of nodes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_node_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_node_count_dll(ref int ioncid, ref int meshId, ref int nnode);

        /// <summary>
        /// Gets the number of edges in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="nedge">Number of edges.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_edge_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_edge_count_dll(ref int ioncid, ref int meshId, ref int nedge);

        /// <summary>
        /// Gets the number of faces in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="nface">Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_face_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_face_count_dll(ref int ioncid, ref int meshId, ref int nface);

        /// <summary>
        /// Gets the maximum number of nodes for any face in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="nmaxfacenodes">The maximum number of nodes per face in the mesh.Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_max_face_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_max_face_nodes_dll(ref int ioncid, ref int meshId, ref int nmaxfacenodes);

        /// <summary>
        /// Gets the x,y coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="c_xptr">Pointer to array for x-coordinates</param>
        /// <param name="c_yptr">Pointer to array for y-coordinates</param>
        /// <param name="nnode">The number of nodes in the mesh.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_node_coordinates", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_node_coordinates_dll([In, Out] ref int ioncid, [In, Out] ref int meshId, [In, Out] ref IntPtr c_xptr, [In, Out]ref IntPtr c_yptr, [In, Out] ref int nnode);

        /// <summary>
        /// Gets the edge-node connectvit table for all edges in the specified mesh.
        /// The output edge_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="c_edge_nodes_ptr">Pointer to array for the edge-node connectivity table.</param>
        /// <param name="nedge">The number of edges in the mesh.</param>
        /// <param name="startIndex"></param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_edge_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_edge_nodes_dll(ref int ioncid, ref int meshId, ref IntPtr c_edge_nodes_ptr, ref int nedge, ref int startIndex);

        /// <summary>
        /// Gets the face-node connectvit table for all faces in the specified mesh.
        /// The output face_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="c_face_nodes_ptr">Pointer to array for the face-node connectivity table.</param>
        /// <param name="nface">The number of faces in the mesh.</param>
        /// <param name="nmaxfacenodes">The maximum number of nodes per face in the mesh.</param>
        /// <param name="fillvalue"></param>
        /// <param name="startIndex"></param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_face_nodes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_face_nodes_dll(ref int ioncid, ref int meshId, ref IntPtr c_face_nodes_ptr, ref int nface, ref int nmaxfacenodes, ref int fillvalue, ref int startIndex);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_geom_ugrid", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_geom_ugrid_dll(string filename);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_map_ugrid", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_write_map_ugrid_dll(string filename);
        
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_coordinate_system", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_coordinate_system_dll([In] ref int ioncid, [In, Out] ref int nmesh);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_var_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_var_count_dll([In] ref int ioncid, [In] ref int mesh, [In] ref int location, [In, Out] ref int nCount);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varid", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_inq_varid_dll(ref int ioncid, ref int meshId, string varName, ref int varId);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varid_by_standard_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_inq_varid_by_standard_name_dll(ref int ioncid, ref int meshId, ref int location, string standardName, ref int varId);

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
        private static extern int ionc_put_var_dll(ref int ioncid, ref int meshId, ref int iloctype, string c_varname, ref IntPtr c_values_ptr, ref int nVal);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_node_coordinates", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_node_coordinates_dll(ref int ioncid, ref int meshId, ref IntPtr c_xvalues_ptr, ref IntPtr c_yvalues_ptr, ref int nNode);

        #endregion
        #region 1d2d Links

        /// <summary>
        /// Gets the 1d2d grid.
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshId"></param>
        /// <param name="meshgeom"></param>
        /// <param name="startIndex"></param>
        /// <param name="includeArrays"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_meshgeom",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_meshgeom_dll(ref int ioncid, ref int meshid, ref int networkId, [In, Out] ref meshgeom meshgeom, ref int startIndex, ref bool includeArrays);

        /// <summary>
        /// Gets the dimension of the 1d2d grid.
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshId"></param>
        /// <param name="meshgeomdim"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_meshgeom_dim", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_meshgeom_dim_dll([In] ref int ioncid, [In] ref int meshid, [In] ref int networkId, [In, Out] ref meshgeomdim meshgeomdim);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_meshgeom", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_put_meshgeom_dll([In] ref int ioncid, [In, Out] ref int meshid, [In, Out] ref int networkid, [In] ref meshgeom meshgeom, [In] ref meshgeomdim meshgeomdim, [In] string c_meshname, [In] string c_networkName, [In] ref int start_index);


        #region meshgeom

        [StructLayout(LayoutKind.Sequential)]
        public struct meshgeom
        {
            public IntPtr edge_nodes;
            public IntPtr face_nodes;
            public IntPtr edge_faces;
            public IntPtr face_edges;
            public IntPtr face_links;

            public IntPtr nnodex;
            public IntPtr nnodey;
            public IntPtr nedge_nodes;
            public IntPtr nbranchlengths;
            public IntPtr nbranchgeometrynodes;

            public IntPtr ngeopointx;
            public IntPtr ngeopointy;
            public IntPtr nbranchorder;
            public IntPtr branchidx;
            public IntPtr branchoffsets;

            public IntPtr nodex;
            public IntPtr nodey;
            public IntPtr nodez;
            public IntPtr edgex;
            public IntPtr edgey;
            public IntPtr edgez;
            public IntPtr facex;
            public IntPtr facey;
            public IntPtr facez;

            public IntPtr layer_zs;
            public IntPtr interface_zs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct meshgeomdim
        {
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public char[] meshname;
            public int dim;
            public int numnode;
            public int numedge;
            public int numface;
            public int maxnumfacenodes;
            public int numlayer;
            public int layertype;
            public int nnodes;
            public int nbranches;
            public int ngeometry;
            public int epgs;
        }

        #endregion meshgeom

        #endregion
        #region UGRID 1D Specifics

        /// <summary>
        /// This is a structure to pass arrays of chars arrays from c# to fortran.
        /// </summary>
        public const int idssize = 40;
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
        private static extern int ionc_add_global_attributes_dll([In] ref int ioncid, ref interop_metadata metadata);

        /// <summary>
        /// This function creates a new netCDF file
        /// </summary>
        /// <param name="c_path">The path where the file will be created (in)</param>
        /// <param name="mode"> The netCDF opening mode (in)</param>
        /// <param name="ioncid">The netCDF file id (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_create", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_create_dll([In] string c_path, [In] ref int mode, [In, Out] ref int ioncid);

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
        /// <param name="startIndex"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_1d_network_branches", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_1d_network_branches_dll([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_sourcenodeid, [In] ref IntPtr c_targetnodeid, interop_charinfo[] branchinfo, [In] ref IntPtr c_branchlengths, [In] ref IntPtr c_nbranchgeometrypoints, [In] ref int nBranches, [In] ref int startIndex);

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
        /// <param name="networkname">The network name</param>
        /// <param name="meshId">The mesh id (out)</param>
        /// <param name="meshname">The mesh name (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_create_1d_mesh", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_create_1d_mesh_dll([In] ref int ioncid, [In] string networkname, [In, Out] ref int meshId, [In] string meshname, [In] ref int nmeshpoints);

        /// <summary>
        /// Writes the mesh coordinates points 
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="meshid">dataset mesh 1d id</param>
        /// <param name="c_branchidx">The branch id for each mesh point (in)</param>
        /// <param name="c_offset">The offset along the branch from the starting point (in)</param>
        /// <param name="nodeinfo">The node info (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <param name="startIndex">array start index</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_1d_mesh_discretisation_points", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_1d_mesh_discretisation_points_dll([In] ref int ioncid, [In] ref int meshid, [In] ref IntPtr c_branchidx, [In] ref IntPtr c_offset, interop_charinfo[] nodeinfo, [In] ref int nmeshpoints, [In] ref int startIndex);

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
        /// Get the number of geometry points for all branches
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="ngeometrypoints">The number of geometry points for all branches (out)</param>
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
        /// <param name="startIndex"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_network_branches", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_network_branches_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_sourcenodeid, [In, Out] ref IntPtr c_targetnodeid, [In, Out] ref IntPtr c_branchlengths, [In, Out]  interop_charinfo[] branchinfo, [In, Out] ref IntPtr c_nbranchgeometrypoints, [In] ref int nBranches, [In] ref int startIndex);

        /// <summary>
        /// Reads the branch geometry
        /// </summary>
        /// <param name="ioncid">The netCDF file id</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_geopointsX">The x coordinates of the geometry points (out)</param>
        /// <param name="c_geopointsY">The y coordinates of the geometry points (out)</param>
        /// <param name="nGeometrypoints">The number of nodes (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_read_1d_network_branches_geometry", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_read_1d_network_branches_geometry_dll([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_geopointsX, [In, Out] ref IntPtr c_geopointsY, [In] ref int nGeometrypoints);

        /// <summary>
        /// Get the number of mesh discretization points 
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="meshId">The mesh id (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_mesh_discretisation_points_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_mesh_discretisation_points_count_dll([In] ref int ioncid, [In] ref int meshId, [In, Out] ref int nmeshpoints);

        /// <summary>
        /// Read the coordinates of the mesh points  
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="meshId">The mesh id (in)</param>
        /// <param name="c_branchidx">The branch id for each mesh point (out)</param>
        /// <param name="c_offset">The offset along the branch from the starting point (out)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_mesh_discretisation_points", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_mesh_discretisation_points_dll([In] ref int ioncid, [In] ref int meshId, [In, Out] ref IntPtr c_branchidx, [In, Out] ref IntPtr c_offset, [In, Out] interop_charinfo[] value, [In] ref int nmeshpoints, [In] ref int startIndex);

        /// <summary>
        /// Defines the contacts structure.
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="linkmesh">The id of the linksmesh (out)</param>
        /// <param name="linkmeshname">The name of the link (in)</param>
        /// <param name="ncontacts">The number of contactss (in)</param>
        /// <param name="mesh1">The id of the first connecting mesh (in)</param>
        /// <param name="mesh2">The id of the second connecting mesh (in)</param>
        /// <param name="locationType1Id">The location type for the first mesh: 0, 1, 2 for node, edge, face respectively (in)</param>
        /// <param name="locationType2Id">The location type for the second mesh (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_def_mesh_contact", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_def_mesh_contact_dll([In] ref int ioncid, [In, Out] ref int linkmesh, string linkmeshname, [In] ref int ncontacts, [In] ref int mesh1, [In] ref int mesh2, [In] ref int locationType1Id, [In] ref int locationType2Id);

        /// <summary>
        /// Puts the contacts structure.
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="linkmesh">The id of the linkmesh (in)</param>
        /// <param name="c_mesh1indexes">The mesh1 indexes (in)</param>
        /// <param name="c_mesh2indexes">The mesh2 indexes (in)</param>
        /// <param name="c_contacttype">type of link</param>
        /// <param name="contactsinfo">The contacts info containing the ids and longnames (in)</param>
        /// <param name="ncontacts">The number of contactss (in)</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_mesh_contact", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_mesh_contact_dll([In] ref int ioncid, [In] ref int linkmesh, [In] ref IntPtr c_mesh1indexes, [In] ref IntPtr c_mesh2indexes, [In] ref IntPtr c_contacttype, [In, Out]  interop_charinfo[] contactsinfo, [In] ref int ncontacts, [In] ref int startIndex);

        /// <summary>
        /// Get the number of contacts from a specific linkmesh
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="linkmesh">The id of the linkmesh (in)</param>
        /// <param name="nlinks">The number of contactss (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_contacts_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_contacts_count_dll([In] ref int ioncid, [In] ref int linkmesh, [In, Out] ref int nlinks);

        /// <summary>
        /// Get the the mesh contacts ids from a specific linkmesh 
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="linkmesh">The id of the linkmesh (in)</param>
        /// <param name="c_mesh1indexes">The mesh1 indexes (out)</param>
        /// <param name="c_mesh2indexes">The mesh2 indexes (out)</param>
        /// <param name="c_contacttype">link type</param>
        /// <param name="contactsinfo">The contacts info containing the ids and longnames (out)</param>
        /// <param name="nlinks">The number of contactss (in)</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_contact", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_mesh_contact_dll([In] ref int ioncid, [In] ref int linkmesh, [In, Out] ref IntPtr c_mesh1indexes, [In, Out] ref IntPtr c_mesh2indexes, [In, Out] ref IntPtr c_contacttype, [In, Out]  interop_charinfo[] contactsinfo, [In] ref int nlinks, [In] ref int startIndex);

        /// <summary>
        /// Clone the definitions specific mesh from one netCDF file to another netCDF. 
        /// Clones all related attributes of the mesh, but it can not clone mesh contacts yet!
        /// </summary>
        /// <param name="ncidin">The input netCDF file id containing the mesh to clone (in)</param>
        /// <param name="ncidout">The output netCDF file id, can be empty/not empty (in)</param>
        /// <param name="meshidin">The mesh id to copy (in)</param>
        /// <param name="meshidout">The id of the cloned mesh in the output file (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_clone_mesh_definition", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_clone_mesh_definition_dll([In] ref int ncidin, [In] ref int ncidout, [In] ref int meshidin, [In, Out] ref int meshidout);

        /// <summary>
        /// Clone the data of a specific mesh from one netCDF file to another netCDF
        /// </summary>
        /// <param name="ncidin">The input netCDF file id containing the mesh to clone (in)</param>
        /// <param name="ncidout">The output netCDF file id, can be empty/not empty (in)</param>
        /// <param name="meshidin">The mesh id to copy (in)</param>
        /// <param name="meshidout">The id of the cloned mesh in the output file (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_clone_mesh_data", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_clone_mesh_data_dll([In] ref int ncidin, [In] ref int ncidout, [In] ref int meshidin, [In] ref int meshidout);

        /// <summary>
        /// Gets the number of networks
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="nnumNetworks"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_number_of_networks", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_number_of_networks_dll([In] ref int ioncid, [In, Out] ref int nnumNetworks);

        /// <summary>
        /// Gets the number of meshes
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshType"> Mesh type: 0 = any type, 1 = 1D mesh, 2 = 2D mesh, 3 = 3D mesh </param>
        /// <param name="numMeshes"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_number_of_meshes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_number_of_meshes_dll([In] ref int ioncid, [In] ref int meshType, [In, Out] ref int numMeshes);

        /// <summary>
        /// Get the network ids
        /// </summary>
        /// <param name="ncidin"></param>
        /// <param name="c_networkids"></param>
        /// <param name="nnumNetworks"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_network_ids", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_network_ids_dll([In] ref int ncidin, [In, Out] ref IntPtr c_networkids, [In] ref int nnumNetworks);

        /// <summary>
        /// Gets the mesh ids
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshType"></param>
        /// <param name="pointerToMeshIds"></param>
        /// <param name="nnumNetworks"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_ug_get_mesh_ids", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_ug_get_mesh_ids_dll([In] ref int ioncid, [In] ref int meshType, [In, Out] ref IntPtr pointerToMeshIds, [In] ref int nnumNetworks);

        //-Branch order functions -------------------//

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_1d_network_branchorder", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_1d_network_branchorder_dll([In] ref int ioncId, [In] ref int networkId, [In] ref IntPtr pointerToBranchOrder, [In] ref int numberOfBranches);


        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_1d_network_branchorder", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_1d_network_branchorder_dll([In] ref int ioncId, [In] ref int networkId, [In, Out] ref IntPtr pointerToBranchOrder, [In] ref int numberOfBranches);


        //-Get the network id for a specified mesh
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_network_id_from_mesh_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_network_id_from_mesh_id_dll([In] ref int ioncid, [In] ref int meshId, [In, Out] ref int networkid);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_contact_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_contact_id_dll([In] ref int ioncid, [In] ref int contactId);

        // Read/Write discretisation point ids
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_def_mesh_ids", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_def_mesh_ids_dll([In] ref int ioncid, [In] ref int meshid, [In] ref int iloctype);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_var_chars", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_var_chars_dll([In] ref int ioncid, [In] ref int meshid, [MarshalAs(UnmanagedType.LPStr)][In, Out] StringBuilder varname, [In] interop_charinfo[] values, [In] ref int nvalues);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_var_chars", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_var_chars_dll([In] ref int ioncid, [In] ref int meshid, [MarshalAs(UnmanagedType.LPStr)][In, Out] StringBuilder varname, [In, Out] interop_charinfo[] values, [In] ref int nvalues);


        #endregion

        public virtual bool AdherestoConventions(int ioncId, int convtype)
        {
            return ionc_adheresto_conventions_dll(ref ioncId, ref convtype);
        }

        public virtual int InqueryConventions(int ioncId, ref int convtype, ref double convversion)
        {
            return ionc_inq_conventions_dll(ref ioncId, ref convtype, ref convversion);
        }

        public virtual int Open(string path, int mode, ref int ioncId, ref int convtype, ref double convversion)
        {
            return ionc_open_dll(path, ref mode, ref ioncId, ref convtype, ref convversion);
        }

        public virtual int Close(int ioncId)
        {
            return ionc_close_dll(ref ioncId);
        }

        public virtual int GetNetworkName(int ioncId, int networkId, StringBuilder networkName)
        {
            return ionc_get_network_name_dll(ref ioncId, ref networkId, networkName);
        }

        public virtual int DefineVariable(int ioncId, int meshId, int varId, int type, GridApiDataSet.LocationType locationType, string varName,
            string standardName, string longName, string unit, double fillValue)
        {
            var locType = (int) locationType;
            return ionc_def_var_dll(ref ioncId, ref meshId, ref varId, ref type, ref locType, varName, standardName,
                longName, unit, ref fillValue);
        }

        public virtual int Get1DNetworkId(int ioncId, ref int networkId)
        {
            return ionc_get_1d_network_id_dll(ref ioncId, ref networkId);
        }

        public virtual int Get1DMeshId(int ioncId, ref int meshId)
        {
            return ionc_get_1d_mesh_id_dll(ref ioncId, ref meshId);
        }

        public virtual int Get2DMeshId(ref int ioncId, ref int meshId)
        {
            return ionc_get_2d_mesh_id_dll(ref ioncId, ref meshId);
        }

        public virtual int Get3DMeshId(int ioncId, ref int meshId)
        {
            return ionc_get_3d_mesh_id_dll(ref ioncId, ref meshId);
        }

        public virtual int GetMeshCount(int ioncId, ref int numberOfMesh)
        {
            return ionc_get_mesh_count_dll(ref ioncId, ref numberOfMesh);
        }

        public virtual int GetMeshName(int ioncId, int meshId, StringBuilder meshName)
        {
            return ionc_get_mesh_name_dll(ref ioncId, ref meshId, meshName);
        }

        public virtual int GetNodeCount(int ioncId, int meshId, ref int numberOfNodes)
        {
            return ionc_get_node_count_dll(ref ioncId, ref meshId, ref numberOfNodes);
        }

        public virtual int GetEdgeCount(int ioncId, int meshId, ref int numberOfEdges)
        {
            return ionc_get_edge_count_dll(ref ioncId, ref meshId, ref numberOfEdges);
        }

        public virtual int GetFaceCount(int ioncId, int meshId, ref int numberOfFaces)
        {
            return ionc_get_face_count_dll(ref ioncId, ref meshId, ref numberOfFaces);
        }

        public virtual int GetMaxFaceNodes(int ioncId, int meshId, ref int numberOfMaxFaceNodes)
        {
            return ionc_get_max_face_nodes_dll(ref ioncId, ref meshId, ref numberOfMaxFaceNodes);
        }

        public virtual int GetNodeCoordinates(int ioncId, int meshId, ref IntPtr xptr, ref IntPtr yptr, int numberOfNodes)
        {
            return ionc_get_node_coordinates_dll(ref ioncId, ref meshId, ref xptr, ref yptr, ref numberOfNodes);
        }

        public virtual int GetEdgeNodes(int ioncId, int meshId, ref IntPtr edgeNodesPtr, int numberOfEdges)
        {
            // Note: startIndex should always be the same for both GetEdgeNodes and GetFaceNodes
            var startIndex = StartIndex; // always update the const StartIndex!
            return ionc_get_edge_nodes_dll(ref ioncId, ref meshId, ref edgeNodesPtr, ref numberOfEdges, ref startIndex);
        }

        public virtual int GetFaceNodes(int ioncId, int meshId, ref IntPtr faceNodesPtr, int numberOfFaces, int numberOfMaxFaceNodes, ref int fillvalue)
        {
            // Note: startIndex should always be the same for both GetEdgeNodes and GetFaceNodes
            var startIndex = StartIndex; // always update the const StartIndex!
            return ionc_get_face_nodes_dll(ref ioncId, ref meshId, ref faceNodesPtr, ref numberOfFaces, ref numberOfMaxFaceNodes, ref fillvalue, ref startIndex);
        }

        public virtual int WriteGeomUgrid(string filename)
        {
            return ionc_write_geom_ugrid_dll(filename);
        }

        public virtual int WriteMapUgrid(string filename)
        {
            return ionc_write_map_ugrid_dll(filename);
        }

        public virtual int GetCoordinateSystem(int ioncId, ref int nmesh)
        {
            return ionc_get_coordinate_system_dll(ref ioncId, ref nmesh);
        }

        public virtual int GetVariablesCount(int ioncId, int mesh, GridApiDataSet.LocationType locationType, ref int numberOfVarCount)
        {
            var locType = (int) locationType;
            return ionc_get_var_count_dll(ref ioncId, ref mesh, ref locType, ref numberOfVarCount);
        }

        public virtual int InqueryVariableId(int ioncId, int meshId, string varName, ref int varId)
        {
            return ionc_inq_varid_dll(ref ioncId, ref meshId, varName, ref varId);
        }

        public virtual int InqueryVariableIdByStandardName(int ioncId, int meshId, GridApiDataSet.LocationType locationId, string standardName,
            ref int varId)
        {
            var locId = (int) locationId;
            return ionc_inq_varid_by_standard_name_dll(ref ioncId, ref meshId, ref locId, standardName, ref varId);
        }

        public virtual int InqueryVariableIds(int ioncId, int meshId, GridApiDataSet.LocationType locationType, ref IntPtr ptr, int numberOfVar)
        {
            var locType = (int) locationType;
            return ionc_inq_varids_dll(ref ioncId, ref meshId, ref locType, ref ptr, ref numberOfVar);
        }

        public virtual int initialize(IO_NetCDF_Message_Callback messageCallback, IO_NetCDF_Progress_Callback progressCallback)
        {
            return ionc_initialize_dll(messageCallback, progressCallback);
        }

        public virtual int GetVariable(int ioncId, int meshId, int location, string varname, ref IntPtr valuesPtr, int numberOfValues, ref double fillvalue)
        {
            return ionc_get_var_dll(ref ioncId, ref meshId, ref location, varname, ref valuesPtr, ref numberOfValues, ref fillvalue);
        }

        public virtual int PutVariable(int ioncId, int meshId, GridApiDataSet.LocationType locationType, string varname, IntPtr valuesPtr, int numberOfValues)
        {
            var locType = (int) locationType;
            return ionc_put_var_dll(ref ioncId, ref meshId, ref locType, varname, ref valuesPtr, ref numberOfValues);
        }

        public virtual int PutNodeCoordinates(int ioncId, int meshId, IntPtr xvaluesPtr, IntPtr yvaluesPtr, int numberOfNodes)
        {
            return ionc_put_node_coordinates_dll(ref ioncId, ref meshId, ref xvaluesPtr, ref yvaluesPtr, ref numberOfNodes);
        }

        public virtual int AddGlobalAttributes(int ioncId, interop_metadata metadata)
        {
            return ionc_add_global_attributes_dll(ref ioncId, ref metadata);
        }

        public virtual int create(string path, int mode, ref int ioncId)
        {
            return ionc_create_dll(path, ref mode, ref ioncId);
        }

        public virtual int Create1DNetwork(int ioncId, ref int networkId, string networkName, int numberOfNodes, int numberOfBranches, int numberOfGeometry)
        {
            return ionc_create_1d_network_dll(ref ioncId, ref networkId, networkName, ref numberOfNodes, ref numberOfBranches, ref numberOfGeometry);
        }

        public virtual int Write1DNetworkNodes(int ioncId, int networkId, IntPtr xNodesPtr, IntPtr yNodesPtr, interop_charinfo[] nodesinfo, int numberOfNodes)
        {
            return ionc_write_1d_network_nodes_dll(ref ioncId, ref networkId, ref xNodesPtr, ref yNodesPtr, nodesinfo, ref numberOfNodes);
        }

        public virtual int Write1DNetworkBranches(int ioncId, int networkId, IntPtr sourceNodeIdsPtr,
            IntPtr targetNodeIdsPtr, interop_charinfo[] branchinfo, IntPtr branchLengthsPtr,
            IntPtr numberOfBranchGeometryPointsPtr, int numberOfBranches, int startIndex = 0)
        {
            return ionc_put_1d_network_branches_dll(ref ioncId, ref networkId, ref sourceNodeIdsPtr, ref targetNodeIdsPtr, branchinfo, ref branchLengthsPtr, ref numberOfBranchGeometryPointsPtr, ref numberOfBranches, ref startIndex);
        }

        public virtual int Write1DNetworkBranchesGeometry(int ioncId, int networkId, IntPtr xGeometryPointsPtr, IntPtr yGeometryPointsPtr, int numberOfGeometryPoints)
        {
            return ionc_write_1d_network_branches_geometry_dll(ref ioncId, ref networkId, ref xGeometryPointsPtr, ref yGeometryPointsPtr, ref numberOfGeometryPoints);
        }

        public virtual int Create1DMesh(int ioncId, string networkName, ref int meshId, string meshName, int numberOfMeshPoints)
        {
            return ionc_create_1d_mesh_dll(ref ioncId, networkName, ref meshId, meshName, ref numberOfMeshPoints);
        }

        public virtual int Write1DMeshDiscretisationPoints(int ioncid, int mesh1did, IntPtr c_branchidx, IntPtr c_offset, interop_charinfo[] nodeinfo, int nmeshpoints, int startIndex)
        {
            return ionc_put_1d_mesh_discretisation_points_dll(ref ioncid, ref mesh1did, ref c_branchidx, ref c_offset, nodeinfo, ref nmeshpoints, ref startIndex);
        }

        public virtual int Get1DNetworkNodesCount(int ioncId, int networkId, ref int numberOfNodes)
        {
            return ionc_get_1d_network_nodes_count_dll(ref ioncId, ref networkId, ref numberOfNodes);
        }

        public virtual int Get1DNetworkBranchesCount(int ioncId, int networkId, ref int numberOfBranches)
        {
            return ionc_get_1d_network_branches_count_dll(ref ioncId, ref networkId, ref numberOfBranches);
        }

        public virtual int Get1DNetworkBranchesGeometryCoordinateCount(int ioncId, int networkId, ref int numberOfGeometryPoints)
        {
            return ionc_get_1d_network_branches_geometry_coordinate_count_dll(ref ioncId, ref networkId, ref numberOfGeometryPoints);
        }

        public virtual int Read1DNetworkNodes(int ioncId, int networkId, ref IntPtr xNodesPtr, ref IntPtr yNodesPtr, interop_charinfo[] nodesinfo, int numberOfNodes)
        {
            return ionc_read_1d_network_nodes_dll(ref ioncId, ref networkId, ref xNodesPtr, ref yNodesPtr,
                nodesinfo, ref numberOfNodes);
        }

        public virtual int Read1DNetworkBranches(int ioncId, int networkId, ref IntPtr sourceNodeIdsPtr, ref IntPtr targetNodeIdsPtr, ref IntPtr branchLengthsPtr, interop_charinfo[] branchinfo, ref IntPtr numberOfBranchGeometryPointsPtr, int numberOfBranches)
        {
            int startIndex = 0;
            return ionc_get_1d_network_branches_dll(ref ioncId, ref  networkId, ref sourceNodeIdsPtr,
            ref targetNodeIdsPtr, ref branchLengthsPtr, branchinfo,
                ref numberOfBranchGeometryPointsPtr, ref numberOfBranches, ref startIndex);
        }

        public virtual int Read1DNetworkBranchesGeometry(int ioncId, int networkId, ref IntPtr xGeometryPointsPtr, ref IntPtr yGeometryPointsPtr, int numberOfNodes)
        {
            return ionc_read_1d_network_branches_geometry_dll(ref ioncId, ref networkId, ref xGeometryPointsPtr,
            ref yGeometryPointsPtr, ref numberOfNodes);
        }

        public virtual int Get1DMeshDiscretisationPointsCount(int ioncId, int meshId, ref int numberOfMeshPoints)
        {
            return ionc_get_1d_mesh_discretisation_points_count_dll(ref ioncId, ref meshId, ref numberOfMeshPoints);
        }

        public virtual int Read1DMeshDiscretisationPoints(int ioncId, int meshId, ref IntPtr xBranchIndicesPtr, ref IntPtr offsetPtr, interop_charinfo[] discretisationPointInfo, int numberOfDiscretisationPoints)
        {
            int startIndex = 0;
            return ionc_get_1d_mesh_discretisation_points_dll(ref ioncId, ref meshId, ref xBranchIndicesPtr, ref offsetPtr, discretisationPointInfo, ref numberOfDiscretisationPoints, ref startIndex);
        }

        public virtual int Create1D2DLinks(int ioncId, ref int contactsmesh, string contactsmeshname, int ncontacts, int mesh1, int mesh2, int locationType1Id, int locationType2Id)
        {
            return ionc_def_mesh_contact_dll(ref ioncId, ref contactsmesh, contactsmeshname, ref ncontacts, ref mesh1, ref mesh2, ref locationType1Id, ref locationType2Id);
        }

        public virtual int Write1D2DLinks(int ioncId, int contactsmesh, IntPtr c_mesh1DIndexes, IntPtr c_mesh2DIndexes, IntPtr c_contacttype, interop_charinfo[] contactsinfo, int ncontacts)
        {
            var startindex = 0;
            return ionc_put_mesh_contact_dll(ref ioncId, ref contactsmesh, ref c_mesh1DIndexes, ref c_mesh2DIndexes, ref c_contacttype, contactsinfo, ref ncontacts, ref startindex);
        }

        public virtual int GetNumberOf1D2DLinks(ref int ioncId, ref int contactsmesh, ref int ncontacts)
        {
            return ionc_get_contacts_count_dll(ref ioncId, ref contactsmesh, ref ncontacts);
        }

        public virtual int Read1D2DLinks(int ioncId, int contactsmesh, ref IntPtr c_mesh1indexes, ref IntPtr c_mesh2indexes, ref IntPtr c_contacttype, ref interop_charinfo[] contactsinfo, ref int ncontacts)
        {
            int startIndex = 0;
            return ionc_get_mesh_contact_dll(ref ioncId, ref contactsmesh, ref c_mesh1indexes, ref c_mesh2indexes, ref c_contacttype, contactsinfo, ref ncontacts, ref startIndex);
        }

        public virtual int clone_mesh_definition(ref int ncidin, ref int ncidout, ref int meshidin, ref int meshidout)
        {
            return ionc_clone_mesh_definition_dll(ref ncidin, ref ncidout, ref meshidin, ref meshidout);
        }

        public virtual int clone_mesh_data(ref int ncidin, ref int ncidout, ref int meshidin, ref int meshidout)
        {
            return ionc_clone_mesh_data_dll(ref ncidin, ref ncidout, ref meshidin, ref meshidout);
        }

        public virtual int GetNumberOfNetworks(int ioncId, ref int nnumNetworks)
        {
            return ionc_get_number_of_networks_dll(ref ioncId, ref nnumNetworks);
        }

        public virtual int GetNumberOfMeshes(int ioncId, int meshType, ref int numMeshes)
        {
            return ionc_get_number_of_meshes_dll(ref ioncId, ref meshType, ref numMeshes);
        }

        public virtual int GetNetworkIdFromMeshId(int ioncId, int meshId, ref int networkId)
        {
            return ionc_get_network_id_from_mesh_id_dll(ref ioncId, ref meshId, ref networkId);
        }
        
        public virtual int GetNetworkIds(int ioncId, ref IntPtr pointerToNetworkIds, int numberOfNetworks)
        {
            return ionc_get_network_ids_dll(ref ioncId, ref pointerToNetworkIds, ref numberOfNetworks);
        }


        public virtual int Get1D2DLinksMeshId(int ioncId, ref int contactId)
        {
            return ionc_get_contact_id_dll(ref ioncId, ref contactId);
        }

        public virtual int GetMeshIds(int ioncId, UGridMeshType meshType, ref IntPtr pointerToMeshIds, int numberOfMeshes)
        {
            var mType = (int) meshType;
            return ionc_ug_get_mesh_ids_dll(ref ioncId, ref mType, ref pointerToMeshIds, ref numberOfMeshes);
        }

        public virtual int Put1DNetworkBranchorder(int ioncId, int networkId, IntPtr pointerToBranchOrder, int numberOfBranches)
        {
            return ionc_put_1d_network_branchorder_dll(ref ioncId, ref networkId, ref pointerToBranchOrder, ref numberOfBranches);
        }

        public virtual int Get1DNetworkBranchorder(int ioncId, int networkId, ref IntPtr pointerToBranchOrder, int numberOfBranches)
        {
            return ionc_get_1d_network_branchorder_dll(ref ioncId, ref networkId, ref pointerToBranchOrder, ref numberOfBranches);
        }

        public virtual int get_meshgeom(ref int ioncid, ref int meshId, ref meshgeom meshgeom, ref int startIndex, bool includeArrays)
        {
            int networkId = -1;
            return ionc_get_meshgeom_dll(ref ioncid, ref meshId, ref networkId, ref meshgeom, ref startIndex, ref includeArrays);
        }

        public virtual int get_meshgeom_dim(ref int ioncid, ref int meshId, ref meshgeomdim meshgeomdim)
        {
            int networkId = -1;
            return ionc_get_meshgeom_dim_dll(ref ioncid, ref meshId, ref networkId, ref meshgeomdim);
        }
        public virtual int Create2DMesh(int ioncId, ref int meshId, ref int networkId, ref meshgeom meshgeom, ref meshgeomdim meshgeomdim, string meshName, string networkName, ref int start_index)
        {
            return ionc_put_meshgeom_dll(ref ioncId, ref meshId, ref networkId, ref meshgeom, ref meshgeomdim, meshName, networkName, ref start_index);
        }

    }
}