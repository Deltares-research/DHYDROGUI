using System.ComponentModel;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Type of external forcing data file.
    /// </summary>
    public enum BndExtForceDataFileType
    {
        /// <summary>
        /// No data file type defined.
        /// </summary>
        [Description("")]
        None,

        /// <summary>
        /// Space-uniform time series file.
        /// </summary>
        [Description("uniform")]
        Uniform,

        /// <summary>
        /// Space-uniform wind magnitude and direction file.
        /// </summary>
        [Description("unimagdir")]
        UniMagDir,

        /// <summary>
        /// Space- and time-varying wind and pressure file.
        /// </summary>
        [Description("arcinfo")]
        ArcInfo,

        /// <summary>
        /// Space- and time-varying cyclone wind and pressure file.
        /// </summary>
        [Description("spiderweb")]
        SpiderWeb,

        /// <summary>
        /// Space- and time-varying wind and pressure on a curvilinear grid file.
        /// </summary>
        [Description("curvigrid")]
        CurviGrid,

        /// <summary>
        /// NetCDF grid data file.
        /// </summary>
        [Description("netcdf")]
        NetCDF
    }
}