using System.ComponentModel;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Type of external forcing data interpolation.
    /// </summary>
    public enum BndExtForceInterpolationMethod
    {
        /// <summary>
        /// No type defined.
        /// </summary>
        [Description("")]
        None,

        /// <summary>
        /// Space and time.
        /// </summary>
        [Description("linearspacetime")]
        LinearSpaceTime,

        /// <summary>
        /// Constant interpolation.
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