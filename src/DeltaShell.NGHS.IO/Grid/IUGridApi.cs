using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi : IGridApi, IDisposable
    {
        /// <summary>
        /// Gets the number of nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <returns>Number of nodes.</returns>
        int GetNumberOfNodes(int meshid);

        /// <summary>
        /// Gets the number of edges in a single mesh from a data set.
        /// </summary>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <returns>Number of edges.</returns>
        int GetNumberOfEdges(int meshid);
        
        /// <summary>
        /// Gets the number of faces in a single mesh from a data set.
        /// </summary>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <returns>Number of faces.</returns>
        int GetNumberOfFaces(int meshid);

        /// <summary>
        /// Gets the maximum number of nodes for any face in a single mesh from a data set.
        /// </summary>
        /// <param name="meshid">The mesh id in the specified data set.</param>
        /// <returns>The maximum number of nodes per face in the mesh.Number of faces.</returns>
        int GetMaxFaceNodes(int meshid);

        /// <summary>
        /// Gets the x coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <returns>The X coordinates of the nodes.</returns>
        double[] GetNodeXCoordinates(int meshId);

        /// <summary>
        /// Gets the y coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <returns>The Y coordinates of the nodes.</returns>
        double[] GetNodeYCoordinates(int meshId);
        
        /// <summary>
        /// Gets the z coordinates for all nodes in a single mesh from a data set.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <returns>The Z coordinates of the nodes.</returns>
        double[] GetNodeZCoordinates(int meshId);

        /// <summary>
        /// contains the fill value for z-Coordinates
        /// </summary>
        double zCoordinateFillValue { get; set; }

        /// <summary>
        /// Gets the edge-node connectivity table for all edges in the specified mesh.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <returns>Pointer to array for the edge-node connectivity table.</returns>
        int[,] GetEdgeNodesForMesh(int meshId);
        
        /// <summary>
        /// Gets the face-node connectivity table for all faces in the specified mesh.
        /// </summary>
        /// <param name="meshId">The mesh id in the specified data set.</param>
        /// <returns>Pointer to array for the face-node connectivity table.</returns>
        int[,] GetFaceNodesForMesh(int meshId);

        int GetVarCount(int meshId, int locationId);
        int[] GetVarNames(int meshId, int locationId);
        void WriteXYCoordinateValues(int meshId, double[] xValues, double[] yValues);
        void WriteZCoordinateValues(int meshId, double[] zValues);
        string GetMeshName(int meshId);
        
        int ionc_write_geom_ugrid(string filename);
        int ionc_write_map_ugrid(string filename);
    }
}