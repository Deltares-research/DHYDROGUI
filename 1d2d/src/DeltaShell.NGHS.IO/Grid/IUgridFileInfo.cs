namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// Interface to use for netcdf files containing ugrid entities.
    /// </summary>
    public interface IUgridFileInfo
    {
        /// <summary>
        /// The location of the netcdf files containing ugrid entities.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Checks the <c>Path</c> property if it is valid.
        /// </summary>
        /// <returns>True when valid, False when invalid.</returns>
        bool IsValidPath();
    }
}