using System.ComponentModel;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Type of lateral discharge location.
    /// </summary>
    public enum BndExtForceLocationType
    {
        /// <summary>
        /// Only affects enclosed 1D grid points.
        /// </summary>
        [Description("1d")]
        OneD,

        /// <summary>
        /// Only affects enclosed 2D grid points.
        /// </summary>
        [Description("2d")]
        TwoD,

        /// <summary>
        /// Affects all enclosed grid points.
        /// </summary>
        [Description("all")]
        All
    }
}