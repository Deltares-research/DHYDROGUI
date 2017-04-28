using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGridApi : IDisposable
    {
        /// <summary>
        /// Read the convention from the grid nc file via the io_netcdf.dll
        /// </summary>
        /// <param name="file">The grid nc file</param>
        /// <returns>The convention in the grid nc file (or other)</returns>
        GridApiDataSet.DataSetConventions GetConvention(string file);
        
        /// <summary>
        /// Checks whether the specified data set adheres to a specific set of conventions.
        /// Datasets may adhere to multiple conventions at the same time, so use this method
        /// to check for individual conventions.
        /// </summary>
        /// <param name="convtype">The NetCDF conventions type to check for.</param>
        /// <returns>Whether or not the file adheres to the specified conventions.</returns>
        bool adherestoConventions(GridApiDataSet.DataSetConventions convtype);
        
        /// <summary>
        /// Read the convention from the initialized grid nc file 
        /// </summary>
        /// <returns>The convention in the initialized grid nc file (or other)</returns>
        GridApiDataSet.DataSetConventions GetConvention();

        /// <summary>
        /// Tries to open a NetCDF file and initialize based on its specified conventions.
        /// </summary>
        /// <param name="c_path">File name for netCDF dataset to be opened.</param>
        /// <param name="mode">NetCDF open mode, e.g. NF90_NOWRITE.</param>
        void Open(string c_path, GridApiDataSet.NetcdfOpenMode mode);

        /// <summary>
        /// Checks if the gridapi is initialized with a nc file
        /// </summary>
        /// <returns>Initialization status</returns>
        bool Initialized();

        /// <summary>
        /// Tries to close an open io_netcdf data set.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the number of mesh from a data set.
        /// </summary>
        /// <returns>Number of meshes.</returns>
        int GetMeshCount();

        int GetCoordinateSystemCode();

        /// <summary>
        /// Read the version from the initialized grid nc file 
        /// </summary>
        /// <returns>The version in the initialized grid nc file (or NaN)</returns>
        double GetVersion();

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

        int Initialize();
    }
}