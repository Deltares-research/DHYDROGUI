using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data
{
    /// <summary>
    /// Type of initial field (spatial) interpolation.
    /// </summary>
    public enum InitialFieldInterpolationMethod
    {
        /// <summary>
        /// No type defined
        /// </summary>
        [Description("")]
        None,

        /// <summary>
        /// Constant interpolation. Only relevant for <see cref="InitialFieldDataFileType.Polygon"/>.
        /// </summary>
        [Description("constant")]
        Constant,

        /// <summary>
        /// Delaunay triangulation + linear interpolation.
        /// </summary>
        [Description("triangulation")]
        Triangulation,

        /// <summary>
        /// Grid cell averaging.
        /// </summary>
        [Description("averaging")]
        Averaging
    }
}