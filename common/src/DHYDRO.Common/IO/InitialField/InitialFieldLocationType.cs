using System.ComponentModel;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Type of initial field location describing the target location of interpolation.
    /// </summary>
    public enum InitialFieldLocationType
    {
        /// <summary>
        /// Interpolate only to 1D nodes/links.
        /// </summary>
        [Description("1d")]
        OneD,

        /// <summary>
        /// Interpolate only to 2D nodes/links.
        /// </summary>
        [Description("2d")]
        TwoD,

        /// <summary>
        /// Interpolate to all nodes/links.
        /// </summary>
        [Description("all")]
        All
    }
}