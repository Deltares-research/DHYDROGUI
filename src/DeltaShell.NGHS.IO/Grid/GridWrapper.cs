using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class GridWrapper
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
        public static extern bool ionc_adheresto_conventions(ref int ioncid, ref int iconvtype);

        /// <summary>
        /// Inquire the NetCDF conventions used in the dataset.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="iconvtype">The NetCDF conventions type of the dataset.</param>
        /// <param name="convversion"></param>
        /// <returns>Result status, ionc_noerr if successful.</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_conventions", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_inq_conventions(ref int ioncid, ref int iconvtype, ref double convversion);

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
        public static extern int ionc_open([In] string c_path, [In, Out] ref int mode, [In, Out] ref int ioncid, [In, Out] ref int iconvtype, ref double convversion);

        /// <summary>
        /// Tries to close an open io_netcdf data set.
        /// </summary>
        /// <param name="ioncid">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncid)%ncid.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_close", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_close([In] ref int ioncid);

        #region UGRID specifics

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_def_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_def_var(ref int ioncid, ref int meshId, ref int varId, ref int type, ref int locType, string varName, string standardName, string longName, string unit, ref double fillValue);

        /// <summary>
        /// Gets the number of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="nmesh">Number of meshes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_mesh_count([In, Out] ref int ioncid, [In, Out] ref int nmesh);

        /// <summary>
        /// Gets the name of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="nmesh">Mesh id.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_name", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_mesh_name([In, Out] ref int ioncid, [In, Out] ref int meshid, [MarshalAs(UnmanagedType.LPStr)][In, Out] StringBuilder meshName);

        /// <summary>
        /// Gets the number of nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nnode">Number of nodes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_node_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_node_count(ref int ioncid, ref int meshid, ref int nnode);

        /// <summary>
        /// Gets the number of edges in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nedge">Number of edges.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_edge_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_edge_count(ref int ioncid, ref int meshid, ref int nedge);

        /// <summary>
        /// Gets the number of faces in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nface">Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_face_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_face_count(ref int ioncid, ref int meshid, ref int nface);

        /// <summary>
        /// Gets the maximum number of nodes for any face in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nmaxfacenodes">The maximum number of nodes per face in the mesh.Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_max_face_nodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_max_face_nodes(ref int ioncid, ref int meshid, ref int nmaxfacenodes);

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
        public static extern int ionc_get_node_coordinates([In, Out] ref int ioncid, [In, Out] ref int meshid, [In, Out] ref IntPtr c_xptr, [In, Out]ref IntPtr c_yptr, [In, Out] ref int nnode);

        /// <summary>
        /// Gets the edge-node connectvit table for all edges in the specified mesh.
        /// The output edge_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="c_edge_nodes_ptr">Pointer to array for the edge-node connectivity table.</param>
        /// <param name="nedge">The number of edges in the mesh.</param>
        /// <param name="startIndex"></param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_edge_nodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_edge_nodes(ref int ioncid, ref int meshid, ref IntPtr c_edge_nodes_ptr, ref int nedge, ref int startIndex);

        /// <summary>
        /// Gets the face-node connectvit table for all faces in the specified mesh.
        /// The output face_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="c_face_nodes_ptr">Pointer to array for the face-node connectivity table.</param>
        /// <param name="nface">The number of faces in the mesh.</param>
        /// <param name="nmaxfacenodes">The maximum number of nodes per face in the mesh.</param>
        /// <param name="fillvalue"></param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_face_nodes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_face_nodes(ref int ioncid, ref int meshid, ref IntPtr c_face_nodes_ptr, ref int nface, ref int nmaxfacenodes, ref int fillvalue);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_geom_ugrid", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_write_geom_ugrid(string filename);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_write_map_ugrid", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_write_map_ugrid(string filename);
        
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_coordinate_system", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_coordinate_system([In] ref int ioncid, [In, Out] ref int nmesh);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_var_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_var_count([In] ref int ioncid,[In] ref int mesh,[In] ref int location,[In,Out] ref int nCount);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varid", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_inq_varid(ref int ioncid, ref int meshId, string varName, ref int varId);
        
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varid_by_standard_name", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_inq_varid_by_standard_name(ref int ioncid, ref int meshId, ref int location, string standardName, ref int varId);
        
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varids", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_inq_varids(ref int ioncid, ref int meshId, ref int location, ref IntPtr ptr, ref int nVar);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IO_NetCDF_Message_Callback(int level, [MarshalAs(UnmanagedType.LPStr)]string message);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IO_NetCDF_Progress_Callback([MarshalAs(UnmanagedType.LPStr)]string message, ref double progress);
    
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_initialize(IO_NetCDF_Message_Callback c_message_callback, IO_NetCDF_Progress_Callback c_progress_callback);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_get_var(ref int ioncid, ref int meshId, ref int location, string varname, ref IntPtr c_zptr, ref int nNode, ref double c_fillvalue);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_put_var(ref int ioncid, ref int meshid, ref int iloctype, string c_varname, ref IntPtr c_values_ptr,ref int nVal);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_node_coordinates", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ionc_put_node_coordinates(ref int ioncid, ref int meshid, ref IntPtr c_xvalues_ptr, ref IntPtr c_yvalues_ptr, ref int nNode);

        #endregion
    }
}