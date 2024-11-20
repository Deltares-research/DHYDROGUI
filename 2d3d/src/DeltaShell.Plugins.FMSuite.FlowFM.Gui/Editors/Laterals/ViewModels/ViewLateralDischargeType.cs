using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.ViewModels
{
    /// <summary>
    /// The discharge data types as defined in the view.
    /// </summary>
    public enum ViewLateralDischargeType
    {
        /// <summary>
        /// The lateral discharge is constant
        /// </summary>
        [Description("Constant discharge")]
        Constant,
        
        /// <summary>
        /// The lateral discharge is time-dependent
        /// </summary>
        [Description("Discharge time series")]
        TimeSeries,
        
        /// <summary>
        /// The lateral discharge is provided externally
        /// </summary>
        [Description("Real time")]
        RealTime
    }
}