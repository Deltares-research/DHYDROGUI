using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi : IGridApi, IDisposable
    {
        /// <summary>
        /// contains the fill value for z-Coordinates
        /// </summary>
        double ZCoordinateFillValue { get; set; }

        /// <summary>
        /// Gets the number of nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id of the specified data set.</param>
        /// <param name="numberOfNodes">The number of nodes of the specified data set.</param>
        /// <returns>Error code</returns>
        int GetNumberOfNodes(int meshId, out int numberOfNodes);

        /// <summary>
        /// Gets the number of edges in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="numberOfEdges">Number of edges.</param>
        /// <returns>Error code</returns>
        int GetNumberOfEdges(int meshId, out int numberOfEdges);

        /// <summary>
        /// Gets the number of faces in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="numberOfFaces">Number of faces.</param>
        /// <returns>Error code</returns>
        int GetNumberOfFaces(int meshId, out int numberOfFaces);

        /// <summary>
        /// Gets the maximum number of nodes for any face in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="maxFaceNodes">The maximum number of nodes per face in the mesh. Number of faces.</param>
        /// <returns>Error code</returns>
        int GetMaxFaceNodes(int meshId, out int maxFaceNodes);

        /// <summary>
        /// Gets the x coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="xCoordinates">The X coordinates of the nodes.</param>
        /// <returns>Error code</returns>
        int GetNodeXCoordinates(int meshId, out double[] xCoordinates);

        /// <summary>
        /// Gets the y coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="yCoordinates">The Y coordinates of the nodes.</param>
        /// <returns>Error code</returns>
        int GetNodeYCoordinates(int meshId, out double[] yCoordinates);

        /// <summary>
        /// Gets the z coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="zCoordinates">The Z coordinates of the nodes.</param>
        /// <returns>Error code</returns>
        int GetNodeZCoordinates(int meshId, out double[] zCoordinates);

        /// <summary>
        /// Gets the edge-node connectivity table for all edges in the specified mesh.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="edgeNodes">Pointer to array for the edge-node connectivity table.</param>
        /// <returns>Error code</returns>
        int GetEdgeNodesForMesh(int meshId, out int[,] edgeNodes);

        /// <summary>
        /// Gets the face-node connectivity table for all faces in the specified mesh.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <param name="faceNodes">Pointer to array for the face-node connectivity table.</param>
        /// <returns>Error code</returns>
        int GetFaceNodesForMesh(int meshId, out int[,] faceNodes);

        int GetVarCount(int meshId, GridApiDataSet.LocationType locationType, out int nCount);
        int GetVarNames(int meshId, GridApiDataSet.LocationType locationType, out int[] varIds);
        int WriteXYCoordinateValues(int meshId, double[] xValues, double[] yValues);
        int WriteZCoordinateValues(int meshId, GridApiDataSet.LocationType locationType, string varName, string longName, double[] zValues);
        int ReadZCoordinateValues(int meshId, GridApiDataSet.LocationType locationType, string varName, out double[] zValues);
        int GetMeshName(int meshId, out string meshName);
        int write_geom_ugrid(string filename);
        int write_map_ugrid(string filename);
    }
}