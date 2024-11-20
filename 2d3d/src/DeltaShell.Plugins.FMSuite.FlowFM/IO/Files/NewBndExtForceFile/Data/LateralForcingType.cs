using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Data
{
    /// <summary>
    /// Lateral forcing type for import and export.
    /// </summary>
    public enum LateralForcingType
    {
        /// <summary>
        /// Discharge as lateral forcing type
        /// </summary>
        [Description("discharge")]
        Discharge,

        /// <summary>
        /// Unsupported lateral forcing type
        /// </summary>
        [Description("")]
        Unsupported,

        /// <summary>
        /// No lateral forcing type
        /// </summary>
        [Description("")]
        None
    }
}