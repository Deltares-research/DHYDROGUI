using System.ComponentModel;

namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Type of initial field averaging method. Only relevant for <see cref="InitialFieldInterpolationMethod.Averaging"/>.
    /// </summary>
    public enum InitialFieldAveragingType
    {
        /// <summary>
        /// Simple average.
        /// </summary>
        [Description("mean")]
        Mean,

        /// <summary>
        /// Nearest neighbour value.
        /// </summary>
        [Description("nearestNb")]
        NearestNb,

        /// <summary>
        /// Highest value.
        /// </summary>
        [Description("max")]
        Max,

        /// <summary>
        /// Lowest value.
        /// </summary>
        [Description("min")]
        Min,

        /// <summary>
        /// Inverse-weighted distance average.
        /// </summary>
        [Description("invDist")]
        InverseDistance,

        /// <summary>
        /// Smallest absolute value.
        /// </summary>
        [Description("minAbs")]
        MinAbsolute,

        /// <summary>
        /// Median value.
        /// </summary>
        [Description("median")]
        Median
    }
}