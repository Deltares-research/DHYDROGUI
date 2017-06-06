using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGridWrapper
    {
        /// <summary>
        /// Checks whether the specified data set adheres to a specific set of conventions.
        /// Datasets may adhere to multiple conventions at the same time, so use this method
        /// to check for individual conventions.
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="iconvtype">The NetCDF conventions type to check for.</param>
        /// <returns>Whether or not the file adheres to the specified conventions.</returns>
        bool ionc_adheresto_conventions(ref int ioncid, ref int iconvtype);

        /// <summary>
        /// Inquire the NetCDF conventions used in the dataset.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="iconvtype">The NetCDF conventions type of the dataset.</param>
        /// <param name="convversion"></param>
        /// <returns>Result status, ionc_noerr if successful.</returns>
        int ionc_inq_conventions(ref int ioncid, ref int iconvtype, ref double convversion);

        /// <summary>
        /// Tries to open a NetCDF file and initialize based on its specified conventions.
        /// </summary>
        /// <param name="c_path">File name for netCDF dataset to be opened.</param>
        /// <param name="mode">NetCDF open mode, e.g. NF90_NOWRITE.</param>
        /// <param name="ioncid">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncid)%ncid.</param>
        /// <param name="iconvtype">The detected conventions in the file.</param>
        /// <param name="convversion"></param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_open([In] string c_path, [In, Out] ref int mode, [In, Out] ref int ioncid, [In, Out] ref int iconvtype, ref double convversion);

        /// <summary>
        /// Tries to close an open io_netcdf data set.
        /// </summary>
        /// <param name="ioncid">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncid)%ncid.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_close([In] ref int ioncid);

        /// <summary>
        /// Gets the number of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="nmesh">Number of meshes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_mesh_count([In, Out] ref int ioncid, [In, Out] ref int nmesh);

        /// <summary>
        /// Gets the name of mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="nmesh">Mesh id.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_mesh_name([In, Out] ref int ioncid, [In, Out] ref int meshid, [MarshalAs(UnmanagedType.LPStr)][In, Out] StringBuilder meshName);

        /// <summary>
        /// Gets the number of nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nnode">Number of nodes.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_node_count(ref int ioncid, ref int meshid, ref int nnode);

        /// <summary>
        /// Gets the number of edges in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nedge">Number of edges.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_edge_count(ref int ioncid, ref int meshid, ref int nedge);

        /// <summary>
        /// Gets the number of faces in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nface">Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_face_count(ref int ioncid, ref int meshid, ref int nface);

        /// <summary>
        /// Gets the maximum number of nodes for any face in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="nmaxfacenodes">The maximum number of nodes per face in the mesh.Number of faces.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_max_face_nodes(ref int ioncid, ref int meshid, ref int nmaxfacenodes);

        /// <summary>
        /// Gets the x,y coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="c_xptr">Pointer to array for x-coordinates</param>
        /// <param name="c_yptr">Pointer to array for y-coordinates</param>
        /// <param name="nnode">The number of nodes in the mesh.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_node_coordinates([In, Out] ref int ioncid, [In, Out] ref int meshid, [In, Out] ref IntPtr c_xptr, [In, Out]ref IntPtr c_yptr, [In, Out] ref int nnode);

        /// <summary>
        /// Gets the edge-node connectvit table for all edges in the specified mesh.
        /// The output edge_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncid">The IONC data set id.</param>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <param name="c_edge_nodes_ptr">Pointer to array for the edge-node connectivity table.</param>
        /// <param name="nedge">The number of edges in the mesh.</param>
        /// <returns>Result status (IONC_NOERR if successful).</returns>
        int ionc_get_edge_nodes(ref int ioncid, ref int meshid, ref IntPtr c_edge_nodes_ptr, ref int nedge);

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
        int ionc_get_face_nodes(ref int ioncid, ref int meshid, ref IntPtr c_face_nodes_ptr, ref int nface, ref int nmaxfacenodes, ref int fillvalue);

        int ionc_write_geom_ugrid(string filename);
        int ionc_write_map_ugrid(string filename);
        int ionc_get_coordinate_system([In] ref int ioncid, [In, Out] ref int nmesh);
        int ionc_get_var_count([In] ref int ioncid,[In] ref int mesh,[In] ref int location,[In,Out] ref int nCount);
        int ionc_inq_varids(ref int ioncid, ref int meshId, ref int location, ref IntPtr ptr, ref int nVar);
        int ionc_initialize(GridWrapper.IO_NetCDF_Message_Callback c_message_callback, GridWrapper.IO_NetCDF_Progress_Callback c_progress_callback);
        int ionc_get_var(ref int ioncid, ref int meshId, ref int location, string varname, ref IntPtr c_zptr, ref int nNode, ref double c_fillvalue);
        int ionc_put_var(ref int ioncid, ref int meshid, ref int iloctype, string c_varname, ref IntPtr c_values_ptr,ref int nVal);
        int ionc_put_node_coordinates(ref int ioncid, ref int meshid, ref IntPtr c_xvalues_ptr, ref IntPtr c_yvalues_ptr, ref int nNode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ioncid"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        int ionc_add_global_attributes( [In] ref int ioncid, GridWrapper.interop_metadata metadata);

        /// <summary>
        /// This function creates a new netCDF file
        /// </summary>
        /// <param name="c_path">The path where the file will be created (in)</param>
        /// <param name="mode"> The netCDF opening mode (in)</param>
        /// <param name="ioncid">The netCDF file id (out)</param>
        /// <returns></returns>
        int ionc_create([In] string c_path, [In] ref int mode, [In, Out] ref int ioncid);

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
        int ionc_create_1d_network([In] ref int ioncid, [In, Out] ref int networkid, [In] string networkName, [In] ref int nNodes, [In] ref int nBranches, [In] ref int nGeometry);

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
        int ionc_write_1d_network_nodes([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_nodesX, [In] ref IntPtr c_nodesY, GridWrapper.interop_charinfo[] nodesinfo, [In] ref int nNodes);

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
        int ionc_write_1d_network_branches( [In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_sourcenodeid, [In] ref IntPtr c_targetnodeid, GridWrapper.interop_charinfo[] branchinfo, [In] ref IntPtr c_branchlengths, [In] ref IntPtr c_nbranchgeometrypoints, [In] ref int nBranches);

        /// <summary>
        /// Writes the branch geometry (the geometry points)  
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_geopointsX">The x coordinates of the geometry points (in)</param>
        /// <param name="c_geopointsY">The y coordinates of the geometry points (in)</param>
        /// <param name="nGeometry">The number of geometry points (in)</param>
        /// <returns></returns>
        int ionc_write_1d_network_branches_geometry([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_geopointsX, [In] ref IntPtr c_geopointsY, [In] ref int nGeometry);

        /// <summary>
        /// Writes a 1d mesh. The geometrical features (e.g. the branches and geometry points) are described in the network above
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="meshname">The mesh name (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <param name="nmeshedges">The number of mesh edges (in)</param>
        /// <returns></returns>
        int ionc_create_1d_mesh([In] ref int ioncid, [In] ref int networkid, string meshname, [In] ref int nmeshpoints, [In] ref int nmeshedges);

        /// <summary>
        /// Writes the mesh coordinates points 
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_branchidx">The branch id for each mesh point (in)</param>
        /// <param name="c_offset">The offset along the branch from the starting point (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <returns></returns>
        int ionc_write_1d_mesh_discretisation_points([In] ref int ioncid, [In] ref int networkid, [In] ref IntPtr c_branchidx, [In] ref IntPtr c_offset, [In] ref int nmeshpoints);

        /// <summary>
        /// Get the number of network nodes
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="nNodes">The number of nodes(out)</param>
        /// <returns></returns>
        int ionc_get_1d_network_nodes_count([In] ref int ioncid, [In] ref int networkid, [In, Out] ref int nNodes);

        /// <summary>
        /// Get the number of branches
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="nBranches">The number of branches (out)</param>
        /// <returns></returns>
        int ionc_get_1d_network_branches_count([In] ref int ioncid, [In] ref int networkid, [In,Out] ref int nBranches);

        /// <summary>
        /// Get the number of geometry points
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="ngeometrypoints">The number of geometry points (out)</param>
        /// <returns></returns>
        int ionc_get_1d_network_branches_geometry_coordinate_count([In] ref int ioncid, [In] ref int networkid, [In, Out] ref int ngeometrypoints);

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
        int ionc_read_1d_network_nodes([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_nodesX, [In, Out] ref IntPtr c_nodesY, [In, Out]  GridWrapper.interop_charinfo[] nodesinfo, [In] ref int nNodes);

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
        int ionc_read_1d_network_branches([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_sourcenodeid, [In, Out] ref IntPtr c_targetnodeid, [In, Out] ref IntPtr c_branchlengths, [In, Out]  GridWrapper.interop_charinfo[] branchinfo, [In, Out] ref IntPtr c_nbranchgeometrypoints, [In] ref int nBranches);

        /// <summary>
        /// Reads the branch geometry
        /// </summary>
        /// <param name="ioncid">The netCDF file id</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_geopointsX">The x coordinates of the geometry points (out)</param>
        /// <param name="c_geopointsY">The y coordinates of the geometry points (out)</param>
        /// <param name="nNodes">The number of nodes (in)</param>
        /// <returns></returns>
        int ionc_read_1d_network_branches_geometry( [In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_geopointsX, [In, Out] ref IntPtr c_geopointsY, [In] ref int nNodes);

        /// <summary>
        /// Get the number of mesh discretization points 
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="nmeshpoints">The number of mesh points (out)</param>
        /// <returns></returns>
        int ionc_get_1d_mesh_discretisation_points_count([In] ref int ioncid, [In] ref int networkid, [In, Out] ref int nmeshpoints);

        /// <summary>
        /// Read the coordinates of the mesh points  
        /// </summary>
        /// <param name="ioncid">The netCDF file id (in)</param>
        /// <param name="networkid">The network id (in)</param>
        /// <param name="c_branchidx">The branch id for each mesh point (out)</param>
        /// <param name="c_offset">The offset along the branch from the starting point (out)</param>
        /// <param name="nmeshpoints">The number of mesh points (in)</param>
        /// <returns></returns>
        int ionc_read_1d_mesh_discretisation_points([In] ref int ioncid, [In] ref int networkid, [In, Out] ref IntPtr c_branchidx, [In, Out] ref IntPtr c_offset, [In] ref int nmeshpoints);
    }
}