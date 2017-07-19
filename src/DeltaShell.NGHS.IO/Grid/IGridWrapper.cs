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
        /// <param name="ioncId"></param>
        /// <param name="convtype">The NetCDF conventions type to check for.</param>
        /// <returns>Whether or not the file adheres to the specified conventions.</returns>
        bool AdherestoConventions(int ioncId, int convtype);

        /// <summary>
        /// Inquire the NetCDF conventions used in the dataset.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="convtype">The NetCDF conventions type of the dataset.</param>
        /// <param name="convversion"></param>
        /// <returns>Result status, noerr if successful.</returns>
        int InqueryConventions(int ioncId, ref int convtype, ref double convversion);

        /// <summary>
        /// Tries to open a NetCDF file and initialize based on its specified conventions.
        /// </summary>
        /// <param name="path">File name for netCDF dataset to be opened.</param>
        /// <param name="mode">NetCDF open mode, e.g. NF90_NOWRITE.</param>
        /// <param name="ioncId">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncId)%ncid.</param>
        /// <param name="convtype">The detected conventions in the file.</param>
        /// <param name="convversion"></param>
        /// <returns>Result status (NOERR if successful).</returns>
        int Open(string path, int mode, ref int ioncId, ref int convtype, ref double convversion);

        /// <summary>
        /// Tries to close an open io_netcdf data set.
        /// </summary>
        /// <param name="ioncId">The io_netcdf dataset id (this is not the NetCDF ncid, which is stored in datasets(ioncId)%ncid.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int Close(int ioncId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ioncId"></param>
        /// <param name="networkId"></param>
        /// <param name="networkName"></param>
        /// <returns></returns>
        int GetNetworkName(int ioncId, int networkId, [MarshalAs(UnmanagedType.LPStr)] StringBuilder networkName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ioncId"></param>
        /// <param name="meshId"></param>
        /// <param name="varId"></param>
        /// <param name="type"></param>
        /// <param name="locationType"></param>
        /// <param name="varName"></param>
        /// <param name="standardName"></param>
        /// <param name="longName"></param>
        /// <param name="unit"></param>
        /// <param name="fillValue"></param>
        /// <returns></returns>
        int DefineVariable(int ioncId, int meshId, int varId, int type, GridApiDataSet.LocationType locationType, string varName, string standardName, string longName, string unit, double fillValue);

        /// <summary>
        /// Gets the number of mesh from a data set.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="numberOfMesh">Number of meshes.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetMeshCount(int ioncId, ref int numberOfMesh);

        /// <summary>
        /// Gets the name of mesh from a data set.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">Mesh id.</param>
        /// <param name="meshName">The mesh name.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetMeshName(int ioncId, int meshId, [MarshalAs(UnmanagedType.LPStr)] [In] [Out] StringBuilder meshName);

        /// <summary>
        /// Gets the number of nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="numberOfNodes">Number of nodes.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetNodeCount(int ioncId, int meshId, ref int numberOfNodes);

        /// <summary>
        /// Gets the number of edges in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="numberOfEdges">Number of edges.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetEdgeCount(int ioncId, int meshId, ref int numberOfEdges);

        /// <summary>
        /// Gets the number of faces in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="numberOfFaces">Number of faces.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetFaceCount(int ioncId, int meshId, ref int numberOfFaces);

        /// <summary>
        /// Gets the maximum number of nodes for any face in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="numberOfMaxFaceNodes">The maximum number of nodes per face in the mesh.Number of faces.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetMaxFaceNodes(int ioncId, int meshId, ref int numberOfMaxFaceNodes);

        /// <summary>
        /// Gets the x,y coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="xptr">Pointer to array for x-coordinates</param>
        /// <param name="yptr">Pointer to array for y-coordinates</param>
        /// <param name="numberOfNodes">The number of nodes in the mesh.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetNodeCoordinates(int ioncId, int meshId, ref IntPtr xptr, ref IntPtr yptr, int numberOfNodes);

        /// <summary>
        /// Gets the edge-node connectvit table for all edges in the specified mesh.
        /// The output edge_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="edgeNodesPtr">Pointer to array for the edge-node connectivity table.</param>
        /// <param name="numberOfEdges">The number of edges in the mesh.</param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetEdgeNodes(int ioncId, int meshId, ref IntPtr edgeNodesPtr, int numberOfEdges);

        /// <summary>
        /// Gets the face-node connectvit table for all faces in the specified mesh.
        /// The output face_nodes array is supposed to be of exact correct size already.
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="faceNodesPtr">Pointer to array for the face-node connectivity table.</param>
        /// <param name="numberOfFaces">The number of faces in the mesh.</param>
        /// <param name="numberOfMaxFaceNodes">The maximum number of nodes per face in the mesh.</param>
        /// <param name="fillvalue"></param>
        /// <returns>Result status (NOERR if successful).</returns>
        int GetFaceNodes(int ioncId, int meshId, ref IntPtr faceNodesPtr, int numberOfFaces, int numberOfMaxFaceNodes, ref int fillvalue);

        int WriteGeomUgrid(string filename);
        int WriteMapUgrid(string filename);
        int GetCoordinateSystem(int ioncId, ref int epsg);
        int GetVariablesCount(int ioncId, int meshId, GridApiDataSet.LocationType locationType, ref int numberOfVarCount);
        int InqueryVariableId(int ioncId, int meshId, string varName, ref int varId);
        int InqueryVariableIdByStandardName(int ioncId, int meshId, GridApiDataSet.LocationType locationId, string standardName, ref int varId);
        int InqueryVariableIds(int ioncId, int meshId, GridApiDataSet.LocationType locationType, ref IntPtr ptr, int numberOfVar);
        int initialize(GridWrapper.IO_NetCDF_Message_Callback messageCallback, GridWrapper.IO_NetCDF_Progress_Callback progressCallback);
        int GetVariable(int ioncId, int meshId, int location, string varname, ref IntPtr valuesPtr, int numberOfValues, ref double fillvalue);
        int PutVariable(int ioncId, int meshId, GridApiDataSet.LocationType locationType, string varname, IntPtr valuesPtr, int numberOfValues);
        int PutNodeCoordinates(int ioncId, int meshId, IntPtr xvaluesPtr, IntPtr yvaluesPtr, int numberOfNodes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ioncId"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        int AddGlobalAttributes(int ioncId, GridWrapper.interop_metadata metadata);

        /// <summary>
        /// This function creates a new netCDF file
        /// </summary>
        /// <param name="path">The path where the file will be created (in)</param>
        /// <param name="mode"> The netCDF opening mode (in)</param>
        /// <param name="ioncId">The netCDF file id (out)</param>
        /// <returns></returns>
        int create(string path, int mode, ref int ioncId);

        /// <summary>
        /// Create a 1d network in an opened netCDF file  
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (out)</param>
        /// <param name="networkName">The network name (in) </param>
        /// <param name="numberOfNodes">The number of network nodes (in) </param>
        /// <param name="numberOfBranches">The number of network branches (in)</param>
        /// <param name="numberOfGeometry">The number of geometry points (in)</param>
        /// <returns></returns>
        int Create1DNetwork(int ioncId, ref int networkId, string networkName, int numberOfNodes, int numberOfBranches, int numberOfGeometry);

        /// <summary>
        /// Write the coordinates of the network nodes
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="xNodesPtr">The x coordinates of the network nodes (in)</param>
        /// <param name="yNodesPtr">The y coordinates of the network nodes (in)</param>
        /// <param name="nodesinfo">The network infos (in)</param>
        /// <param name="numberOfNodes">The number of network nodes (in)</param>
        /// <returns></returns>
        int Write1DNetworkNodes(int ioncId, int networkId, IntPtr xNodesPtr, IntPtr yNodesPtr, GridWrapper.interop_charinfo[] nodesinfo, int numberOfNodes);

        /// <summary>
        /// Write the coordinates of the network branches
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="sourceNodeIdsPtr">The source node id (in)</param>
        /// <param name="targetNodeIdsPtr">The target node id (in)</param>
        /// <param name="branchinfo">The branch info (in)</param>
        /// <param name="branchLengthsPtr">The branch lengths (in)</param>
        /// <param name="numberOfBranchGeometryPointsPtr">The number of geometry points in each branch (in)</param>
        /// <param name="numberOfBranches">The number of branches (in)</param>
        /// <returns></returns>
        int Write1DNetworkBranches(int ioncId, int networkId, IntPtr sourceNodeIdsPtr, IntPtr targetNodeIdsPtr, GridWrapper.interop_charinfo[] branchinfo, IntPtr branchLengthsPtr, IntPtr numberOfBranchGeometryPointsPtr, int numberOfBranches);

        /// <summary>
        /// Writes the branch geometry (the geometry points)  
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="xGeometryPointsPtr">The x coordinates of the geometry points (in)</param>
        /// <param name="yGeometryPointsPtr">The y coordinates of the geometry points (in)</param>
        /// <param name="numberOfGeometryPoints">The number of geometry points (in)</param>
        /// <returns></returns>
        int Write1DNetworkBranchesGeometry(int ioncId, int networkId, IntPtr xGeometryPointsPtr, IntPtr yGeometryPointsPtr, int numberOfGeometryPoints);

        /// <summary>
        /// Writes a 1d mesh. The geometrical features (e.g. the branches and geometry points) are described in the network above
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="meshId">The mesh id (out)</param>
        /// <param name="meshname">The mesh name (in)</param>
        /// <param name="numberOfMeshPoints">The number of mesh points (in)</param>
        /// <param name="numberOfMeshEdges">The number of mesh edges (in)</param>
        /// <returns></returns>
        int Create1DMesh(int ioncId, int networkId, ref int meshId, string meshname, int numberOfMeshPoints, int numberOfMeshEdges);

        /// <summary>
        /// Writes the mesh coordinates points 
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="meshId">The network id (in)</param>
        /// <param name="branchIndicesPtr">The branch id for each mesh point (in)</param>
        /// <param name="offsetPtr">The offset along the branch from the starting point (in)</param>
        /// <param name="numberOfMeshPoints">The number of mesh points (in)</param>
        /// <returns></returns>
        int Write1DMeshDiscretisationPoints(int ioncId, int meshId, IntPtr branchIndicesPtr, IntPtr offsetPtr, int numberOfMeshPoints);

        /// <summary>
        /// Get the number of network nodes
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="numberOfNodes">The number of nodes(out)</param>
        /// <returns></returns>
        int Get1DNetworkNodesCount(int ioncId, int networkId, ref int numberOfNodes);

        /// <summary>
        /// Get the number of branches
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="numberOfBranches">The number of branches (out)</param>
        /// <returns></returns>
        int Get1DNetworkBranchesCount(int ioncId, int networkId, ref int numberOfBranches);

        /// <summary>
        /// Get the number of geometry points
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="numberOfGeometryPoints">The number of geometry points (out)</param>
        /// <returns></returns>
        int Get1DNetworkBranchesGeometryCoordinateCount(int ioncId, int networkId, ref int numberOfGeometryPoints);

        /// <summary>
        /// Read the node coordinates and the charinfo
        /// </summary>
        /// <param name="ioncId">The netCDF file id</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="xNodesPtr">The x coordinates of the network nodes (out)</param>
        /// <param name="yNodesPtr">The y coordinates of the network nodes (out)</param>
        /// <param name="nodesinfo">The network infos (out)</param>
        /// <param name="numberOfNodes">The number of network nodes (in)</param>
        /// <returns></returns>
        int Read1DNetworkNodes(int ioncId, int networkId, ref IntPtr xNodesPtr, ref IntPtr yNodesPtr, GridWrapper.interop_charinfo[] nodesinfo, int numberOfNodes);

        /// <summary>
        /// Read the coordinates of the network branches
        /// </summary>
        /// <param name="ioncId">The netCDF file id</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="sourceNodeIdsPtr">The source node id (out)</param>
        /// <param name="targetNodeIdsPtr">The target node id (out)</param>
        /// <param name="branchLengthsPtr">The branch lengths (out)</param>
        /// <param name="branchinfo">The branch info (out)</param>
        /// <param name="numberOfBranchGeometryPointsPtr">he number of geometry points in each branch (out)</param>
        /// <param name="numberOfBranches">The number of branches (in)</param>
        /// <returns></returns>
        int Read1DNetworkBranches(int ioncId, int networkId, ref IntPtr sourceNodeIdsPtr, ref IntPtr targetNodeIdsPtr, ref IntPtr branchLengthsPtr, GridWrapper.interop_charinfo[] branchinfo, ref IntPtr numberOfBranchGeometryPointsPtr, int numberOfBranches);

        /// <summary>
        /// Reads the branch geometry
        /// </summary>
        /// <param name="ioncId">The netCDF file id</param>
        /// <param name="networkId">The network id (in)</param>
        /// <param name="xGeometryPointsPtr">The x coordinates of the geometry points (out)</param>
        /// <param name="yGeometryPointsPtr">The y coordinates of the geometry points (out)</param>
        /// <param name="numberOfNodes">The number of nodes (in)</param>
        /// <returns></returns>
        int Read1DNetworkBranchesGeometry(int ioncId, int networkId, ref IntPtr xGeometryPointsPtr, ref IntPtr yGeometryPointsPtr, int numberOfNodes);

        /// <summary>
        /// Get the number of mesh discretization points 
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="meshId">The mesh id (in)</param>
        /// <param name="numberOfMeshPoints">The number of mesh points (out)</param>
        /// <returns></returns>
        int Get1DMeshDiscretisationPointsCount(int ioncId, int meshId, ref int numberOfMeshPoints);

        /// <summary>
        /// Read the coordinates of the mesh points  
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="meshId">The mesh id (in)</param>
        /// <param name="xBranchIndicesPtr">The branch id for each mesh point (out)</param>
        /// <param name="offsetPtr">The offset along the branch from the starting point (out)</param>
        /// <param name="numberOfMeshPoints">The number of mesh points (in)</param>
        /// <returns></returns>
        int Read1DMeshDiscretisationPoints(int ioncId, int meshId, ref IntPtr xBranchIndicesPtr, ref IntPtr offsetPtr, int numberOfMeshPoints);

        /// <summary>
        /// Reads the network id for the 1D network
        /// </summary>
        /// <param name="ioncId">The netCDF file id (in)</param>
        /// <param name="networkId">The network id (out)</param>
        /// <returns></returns>
        int Get1DNetworkId(int ioncId, ref int networkId);

        /// <summary>
        /// Reads the mesh id for the 1D mesh
        /// </summary>
        /// <param name="ioncId"></param>
        /// <param name="meshId"></param>
        /// <returns></returns>
        int Get1DMeshId(int ioncId, ref int meshId);

        /// <summary>
        /// Gets the network ids
        /// </summary>
        /// <param name="ioncId">The IONC data set id</param>
        /// <param name="pointerToNetworkIds">Pointer to array of network ids (out)</param>
        /// <param name="numberOfNetworks"></param>
        /// <returns></returns>
        int GetNetworkIds(int ioncId, ref IntPtr pointerToNetworkIds, int numberOfNetworks);

        /// <summary>
        /// Gets the mesh ids for a specified mesh type
        /// </summary>
        /// <param name="ioncId">The IONC data set id</param>
        /// <param name="meshType">Mesh type: 0 = any type, 1 = 1D mesh, 2 = 2D mesh, 3 = 3D mesh</param>
        /// <param name="pointerToMeshIds">Pointer to array of mesh ids</param>
        /// <param name="numberOfMeshes">Number of meshes</param>
        /// <returns></returns>
        int GetMeshIds(int ioncId, UGridMeshType meshType, ref IntPtr pointerToMeshIds, int numberOfMeshes);

        /// <summary>
        /// Gets the number of network in a NetCDF file
        /// </summary>
        /// <param name="ioncId">The IONC data set id.</param>
        /// <param name="numberOfNetworks">The number of networks in the file (out)</param>
        /// <returns></returns>
        int GetNumberOfNetworks(int ioncId, ref int numberOfNetworks);

        /// <summary>
        /// Gets the number of meshes by type in a NetCDF file
        /// </summary>
        /// <param name="ioncId">The IONC data set id</param>
        /// <param name="meshType">Mesh type: 0 = any type, 1 = 1D mesh, 2 = 2D mesh, 3 = 3D mesh</param>
        /// <param name="numberOfMeshes">The number of meshes for the specified type (out)</param>
        /// <returns></returns>
        int GetNumberOfMeshes(int ioncId, int meshType, ref int numberOfMeshes);


        int GetNetworkIdFromMeshId(int ioncId, int meshId, ref int networkId);


        //-Branch order functions -------------------//
        int Put1DNetworkBranchorder(int ioncId, int networkId, IntPtr pointerToBranchOrder, int numberOfBranches);

        int Get1DNetworkBranchorder(int ioncId, int networkId, ref IntPtr pointerToBranchOrder, int numberOfBranches);

    }
}