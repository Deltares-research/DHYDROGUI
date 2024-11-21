using System.ComponentModel;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Type of initial field data file.
    /// </summary>
    public enum InitialFieldDataFileType
    {
        /// <summary>
        /// No type defined
        /// </summary>
        [Description("")]
        None,

        [Description("arcinfo")]
        ArcInfo,

        [Description("GeoTIFF")]
        GeoTIFF,

        [Description("sample")]
        Sample,

        [Description("1dField")]
        OneDField,

        [Description("polygon")]
        Polygon
    }
}