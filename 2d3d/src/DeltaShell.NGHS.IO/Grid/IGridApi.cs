namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGridApi
    {
        /// <summary>
        /// Checks if the gridapi is initialized with a nc file
        /// </summary>
        /// <returns>Initialization status</returns>
        bool Initialized { get; }

        /// <summary>
        /// Read the convention from the grid nc file via the io_netcdf.dll
        /// </summary>
        /// <param name="file">The grid nc file</param>
        /// <param name="convention">The convention in the grid nc file (or other) (out)</param>
        /// <returns>Error code</returns>
        int GetConvention(string file, out GridApiDataSet.DataSetConventions convention);

        /// <summary>
        /// Read the convention from the initialized grid nc file
        /// </summary>
        /// <returns>The convention in the initialized grid nc file (or other)</returns>
        GridApiDataSet.DataSetConventions GetConvention();

        /// <summary>
        /// Checks whether the specified data set adheres to a specific set of conventions.
        /// Datasets may adhere to multiple conventions at the same time, so use this method
        /// to check for individual conventions.
        /// </summary>
        /// <param name="convtype">The NetCDF conventions type to check for.</param>
        /// <returns>Whether or not the file adheres to the specified conventions.</returns>
        bool AdheresToConventions(GridApiDataSet.DataSetConventions convtype);

        /// <summary>
        /// Tries to create a NetCDF file.
        /// </summary>
        /// <param name="filePath">File name for NetCDF dataset to be opened.</param>
        /// <param name="uGridGlobalMetaData">The global metadata of the NetCDF file</param>
        /// <param name="mode">NetCDF open mode, e.g. NF90_NOWRITE.</param>
        int CreateFile(string filePath,
                       UGridGlobalMetaData uGridGlobalMetaData,
                       GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write);

        /// <summary>
        /// Tries to open a NetCDF file and initialize based on its specified conventions.
        /// </summary>
        /// <param name="filePath">File name for netCDF dataset to be opened.</param>
        /// <param name="mode">NetCDF open mode, e.g. NF90_NOWRITE.</param>
        int Open(string filePath, GridApiDataSet.NetcdfOpenMode mode);

        /// <summary>
        /// Tries to close an open io_netcdf data set.
        /// </summary>
        int Close();

        /// <summary>
        /// Gets the number of mesh from a data set.
        /// </summary>
        /// <returns>Number of meshes.</returns>
        int GetMeshCount(out int numberOfMeshes);

        /// <summary>
        /// Gets the coordinate system code.
        /// </summary>
        /// <param name="coordinateSystemCode">The epsg coordinate system code.</param>
        /// <returns>The return code.</returns>
        int GetCoordinateSystemCode(out int coordinateSystemCode);

        /// <summary>
        /// Read the version from the initialized grid nc file
        /// </summary>
        /// <returns>The version in the initialized grid nc file (or NaN)</returns>
        double GetVersion();

        int Initialize();

        int GetNumberOfMeshByType(UGridMeshType meshType, out int numberOfMesh);

        int GetMeshIdsByMeshType(UGridMeshType meshType, int numberOfMeshes, out int[] meshIds);
    }
}