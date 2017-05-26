namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGridApi
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
        /// Tries to create a NetCDF file.
        /// </summary>
        /// <param name="c_path">File name for netCDF dataset to be opened.</param>
        void CreateFile(string c_path, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite);

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
        bool Initialized { get; }

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
        
        int Initialize();
    }
}