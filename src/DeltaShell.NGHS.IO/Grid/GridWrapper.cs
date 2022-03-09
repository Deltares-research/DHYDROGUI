using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridWrapper
    {
        private const int StartIndex = 0;

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

        public virtual int DefineVariable(int ioncId, int meshId, int varId, int type, GridApiDataSet.LocationType locationType, string varName,
                                          string standardName, string longName, string unit, double fillValue)
        {
            // Dummy value: will ensure the networkId gets ignored in io_netcdf.dll. 
            //              The dll requires this value, however we do not
            //              support networks at the time of writing.
            var networkId = 0;

            // Dummy value: required by the io_netcdf.dll.
            int fillValueInt = -999;

            var locType = (int) locationType;
            return ionc_def_var_dll(ref ioncId, ref meshId, ref networkId, ref varId,
                                    ref type, ref locType,
                                    varName, standardName, longName, unit,
                                    ref fillValueInt, ref fillValue);
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
            int startIndex = StartIndex; // always update the const StartIndex!
            return ionc_get_edge_nodes_dll(ref ioncId, ref meshId, ref edgeNodesPtr, ref numberOfEdges, ref startIndex);
        }

        public virtual int GetFaceNodes(int ioncId, int meshId, ref IntPtr faceNodesPtr, int numberOfFaces, int numberOfMaxFaceNodes, ref int fillvalue)
        {
            // Note: startIndex should always be the same for both GetEdgeNodes and GetFaceNodes
            int startIndex = StartIndex; // always update the const StartIndex!
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

        public virtual int AddGlobalAttributes(int ioncId, InteropMetadata metadata)
        {
            return ionc_add_global_attributes_dll(ref ioncId, ref metadata);
        }

        public virtual int create(string path, int mode, ref int ioncId)
        {
            return ionc_create_dll(path, ref mode, ref ioncId);
        }

        public virtual int def_mesh_contact(ref int ioncId, ref int contactsmesh, string contactsmeshname, ref int ncontacts, ref int mesh1, ref int mesh2, ref int locationType1Id, ref int locationType2Id)
        {
            return ionc_def_mesh_contact_dll(ref ioncId, ref contactsmesh, contactsmeshname, ref ncontacts, ref mesh1, ref mesh2, ref locationType1Id, ref locationType2Id);
        }

        public virtual int put_mesh_contact(ref int ioncId, ref int contactsmesh, ref IntPtr c_mesh1indexes, ref IntPtr c_mesh2indexes, InteropCharInfo[] contactsinfo, ref int ncontacts)
        {
            var startindex = 0;
            return ionc_put_mesh_contact_dll(ref ioncId, ref contactsmesh, ref c_mesh1indexes, ref c_mesh2indexes, contactsinfo, ref ncontacts, ref startindex);
        }

        public virtual int get_contacts_count(ref int ioncId, ref int contactsmesh, ref int ncontacts)
        {
            return ionc_get_contacts_count_dll(ref ioncId, ref contactsmesh, ref ncontacts);
        }

        public virtual int get_mesh_contact(ref int ioncId, ref int contactsmesh, ref IntPtr c_mesh1indexes, ref IntPtr c_mesh2indexes, InteropCharInfo[] contactsinfo, ref int ncontacts)
        {
            var startIndex = 0;
            return ionc_get_mesh_contact_dll(ref ioncId, ref contactsmesh, ref c_mesh1indexes, ref c_mesh2indexes, contactsinfo, ref ncontacts, ref startIndex);
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

        public virtual int GetMeshIds(int ioncId, UGridMeshType meshType, ref IntPtr pointerToMeshIds, int numberOfMeshes)
        {
            var mType = (int) meshType;
            return ionc_ug_get_mesh_ids_dll(ref ioncId, ref mType, ref pointerToMeshIds, ref numberOfMeshes);
        }
        
        public virtual int get_meshgeom(ref int ioncid, ref int meshId, ref MeshGeom meshGeom, bool includeArrays)
        {
            return ionc_get_meshgeom_dll(ref ioncid, ref meshId, ref meshGeom, ref includeArrays);
        }

        public virtual int get_meshgeom_dim(ref int ioncid, ref int meshId, ref MeshGeomDim meshGeomDim)
        {
            return ionc_get_meshgeom_dim_dll(ref ioncid, ref meshId, ref meshGeomDim);
        }

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
        private static extern int ionc_open_dll([In] string c_path, [In] [Out] ref int mode, [In] [Out] ref int ioncid, [In] [Out] ref int iconvtype, ref double convversion);

        /// <summary>
        /// Tries to close an open io_netcdf data set.
        /// </summary>
        /// <param name="ioncid">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncid)%ncid.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_close", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_close_dll([In] ref int ioncid);

        #region UGRID specifics

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_def_var", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_def_var_dll(ref int ioncid, ref int meshId, ref int networkId, ref int varId, ref int type, ref int locType, string varName, string standardName, string longName, string unit, ref int fillValueInt, ref double fillValue);

        /// <summary>
        /// Gets the number of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="nmesh">Number of meshes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_mesh_count_dll([In] [Out] ref int ioncid, [In] [Out] ref int nmesh);

        /// <summary>
        /// Gets the name of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshId">Mesh id.</param>
        /// <param name="meshName">The mesh name.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_mesh_name_dll([In] [Out] ref int ioncid, [In] [Out] ref int meshId, [MarshalAs(UnmanagedType.LPStr)] [In] [Out]
                                                         StringBuilder meshName);

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
        private static extern int ionc_get_node_coordinates_dll([In] [Out] ref int ioncid, [In] [Out] ref int meshId, [In] [Out] ref IntPtr c_xptr, [In] [Out] ref IntPtr c_yptr, [In] [Out] ref int nnode);

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
        private static extern int ionc_get_coordinate_system_dll([In] ref int ioncid, [In] [Out] ref int nmesh);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_var_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_var_count_dll([In] ref int ioncid, [In] ref int mesh, [In] ref int location, [In] [Out] ref int nCount);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varid", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_inq_varid_dll(ref int ioncid, ref int meshId, string varName, ref int varId);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varid_by_standard_name", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_inq_varid_by_standard_name_dll(ref int ioncid, ref int meshId, ref int location, string standardName, ref int varId);

        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_inq_varids", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_inq_varids_dll(ref int ioncid, ref int meshId, ref int location, ref IntPtr ptr, ref int nVar);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IO_NetCDF_Message_Callback(int level, [MarshalAs(UnmanagedType.LPStr)] string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void IO_NetCDF_Progress_Callback([MarshalAs(UnmanagedType.LPStr)] string message, ref double progress);

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
        /// <param name="meshGeom"></param>
        /// <param name="includeArrays"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_meshgeom",
                   CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_meshgeom_dll(ref int ioncid, ref int meshid, [In] [Out] ref MeshGeom meshGeom, ref bool includeArrays);

        /// <summary>
        /// Gets the dimension of the 1d2d grid.
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshId"></param>
        /// <param name="meshGeomDim"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_meshgeom_dim", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_meshgeom_dim_dll([In] ref int ioncid, [In] ref int meshid, [In] [Out] ref MeshGeomDim meshGeomDim);

        [StructLayout(LayoutKind.Sequential)]
        public struct MeshGeom
        {
            public IntPtr edge_nodes;
            public IntPtr face_nodes;
            public IntPtr edge_faces;
            public IntPtr face_edges;
            public IntPtr face_links;

            public IntPtr branchids;
            public IntPtr nbranchgeometrynodes;
            public IntPtr nedge_nodes; /* Needs io_netcdf library update */

            public IntPtr nodex;
            public IntPtr nodey;
            public IntPtr nodez;
            public IntPtr edgex;
            public IntPtr edgey;
            public IntPtr edgez;
            public IntPtr facex;
            public IntPtr facey;
            public IntPtr facez;

            public IntPtr branchoffsets;
            public IntPtr geopointsX;
            public IntPtr geopointsY;
            public IntPtr branchlengths;

            public IntPtr layer_zs;
            public IntPtr interface_zs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MeshGeomDim
        {
            public char[] meshname;
            public int dim;
            public int numnode;
            public int numedge;
            public int numface;
            public int maxnumfacenodes;
            public int numlayer;
            public int layertype;
            public int nt_nbranches;
            public int nt_ngeometry;
        }

        #endregion

        #region UGRID 1D Specifics

        /// <summary>
        /// This is a structure to pass arrays of chars arrays from c# to fortran.
        /// </summary>
        public const int idssize = 40;

        public const int longnamessize = 80;

        [StructLayout(LayoutKind.Sequential)]
        public struct InteropCharInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = idssize)]
            public char[] ids;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = longnamessize)]
            public char[] longnames;
        }

        public const int metadatasize = 100;

        [StructLayout(LayoutKind.Sequential)]
        public struct InteropMetadata
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
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_add_global_attributes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_add_global_attributes_dll([In] ref int ioncid, ref InteropMetadata metadata);

        /// <summary>
        /// This function creates a new netCDF file
        /// </summary>
        /// <param name="c_path">The path where the file will be created (in)</param>
        /// <param name="mode"> The netCDF opening mode (in)</param>
        /// <param name="ioncid">The netCDF file id (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_create", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_create_dll([In] string c_path, [In] ref int mode, [In] [Out] ref int ioncid);

        /// <summary>
        /// Defines the contacts structure.
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="contactsmesh">The id of the contactsmesh (out)</param>
        /// <param name="contactsmeshname">The name of the contacts structure (in)</param>
        /// <param name="ncontacts">The number of contactss (in)</param>
        /// <param name="mesh1">The id of the first connecting mesh (in)</param>
        /// <param name="mesh2">The id of the second connecting mesh (in)</param>
        /// <param name="locationType1Id">The location type for the first mesh: 0, 1, 2 for node, edge, face respectively (in)</param>
        /// <param name="locationType2Id">The location type for the second mesh (in)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_def_mesh_contact", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_def_mesh_contact_dll([In] ref int ioncid, [In] [Out] ref int contactsmesh, string contactsmeshname, [In] ref int ncontacts, [In] ref int mesh1, [In] ref int mesh2, [In] ref int locationType1Id, [In] ref int locationType2Id);

        /// <summary>
        /// Puts the contacts structure.
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="contactsmesh">The id of the contactsmesh (in)</param>
        /// <param name="c_mesh1indexes">The mesh1 indexes (in)</param>
        /// <param name="c_mesh2indexes">The mesh2 indexes (in)</param>
        /// <param name="contactsinfo">The contacts info containing the ids and longnames (in)</param>
        /// <param name="ncontacts">The number of contactss (in)</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_put_mesh_contact", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_put_mesh_contact_dll([In] ref int ioncid, [In] ref int contactsmesh, [In] ref IntPtr c_mesh1indexes, [In] ref IntPtr c_mesh2indexes, [In] [Out] InteropCharInfo[] contactsinfo, [In] ref int ncontacts, [In] ref int startIndex);

        /// <summary>
        /// Get the number of contacts from a specific contactsmesh
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="contactsmesh">The id of the contactsmesh (in)</param>
        /// <param name="ncontacts">The number of contactss (out)</param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_contacts_count", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_contacts_count_dll([In] ref int ioncid, [In] ref int contactsmesh, [In] [Out] ref int ncontacts);

        /// <summary>
        /// Get the the mesh contacts ids from a specific contactsmesh
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="contactsmesh">The id of the contactsmesh (in)</param>
        /// <param name="c_mesh1indexes">The mesh1 indexes (out)</param>
        /// <param name="c_mesh2indexes">The mesh2 indexes (out)</param>
        /// <param name="contactsinfo">The contacts info containing the ids and longnames (out)</param>
        /// <param name="ncontacts">The number of contactss (in)</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_mesh_contact", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_mesh_contact_dll([In] ref int ioncid, [In] ref int contactsmesh, [In] [Out] ref IntPtr c_mesh1indexes, [In] [Out] ref IntPtr c_mesh2indexes, [In] [Out] InteropCharInfo[] contactsinfo, [In] ref int ncontacts, [In] ref int startIndex);

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
        private static extern int ionc_clone_mesh_definition_dll([In] ref int ncidin, [In] ref int ncidout, [In] ref int meshidin, [In] [Out] ref int meshidout);

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
        private static extern int ionc_get_number_of_networks_dll([In] ref int ioncid, [In] [Out] ref int nnumNetworks);

        /// <summary>
        /// Gets the number of meshes
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshType"> Mesh type: 0 = any type, 1 = 1D mesh, 2 = 2D mesh, 3 = 3D mesh </param>
        /// <param name="numMeshes"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_number_of_meshes", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_number_of_meshes_dll([In] ref int ioncid, [In] ref int meshType, [In] [Out] ref int numMeshes);

        /// <summary>
        /// Get the network ids
        /// </summary>
        /// <param name="ncidin"></param>
        /// <param name="c_networkids"></param>
        /// <param name="nnumNetworks"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_network_ids", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_network_ids_dll([In] ref int ncidin, [In] [Out] ref IntPtr c_networkids, [In] ref int nnumNetworks);

        /// <summary>
        /// Gets the mesh ids
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="meshType"></param>
        /// <param name="pointerToMeshIds"></param>
        /// <param name="nnumNetworks"></param>
        /// <returns></returns>
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_ug_get_mesh_ids", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_ug_get_mesh_ids_dll([In] ref int ioncid, [In] ref int meshType, [In] [Out] ref IntPtr pointerToMeshIds, [In] ref int nnumNetworks);
        
        //-Get the network id for a specified mesh
        [DllImport(GridApiDataSet.GRIDDLL_NAME, EntryPoint = "ionc_get_network_id_from_mesh_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ionc_get_network_id_from_mesh_id_dll([In] ref int ioncid, [In] ref int meshId, [In] [Out] ref int networkid);
        #endregion
    }
}