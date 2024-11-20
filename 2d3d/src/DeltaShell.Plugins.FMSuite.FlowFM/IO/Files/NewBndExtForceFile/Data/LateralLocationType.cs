using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data
{
    /// <summary>
    /// Lateral location type for import and export.
    /// </summary>
    public enum LateralLocationType
    {
        /// <summary>
        /// Two-dimensional lateral location.
        /// </summary>
        [Description("2d")]
        TwoD,

        /// <summary>
        /// Unsupported lateral location type
        /// </summary>
        [Description("")]
        Unsupported,

        /// <summary>
        /// No lateral location type
        /// </summary>
        [Description("")]
        None
    }
}