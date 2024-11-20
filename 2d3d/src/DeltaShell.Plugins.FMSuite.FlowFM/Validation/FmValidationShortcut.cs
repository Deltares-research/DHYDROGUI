using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// This class describes a shortcut that can be used to link a validation issue for FM to a tab on the FM settings view.
    /// </summary>
    public class FmValidationShortcut
    {
        /// <summary>
        /// The FM model that is used as data for the view that is to be opened.
        /// </summary>
        public WaterFlowFMModel FlowFmModel { get; set; }

        /// <summary>
        /// The tab name of the FM settings view that needs to be opened.
        /// </summary>
        public string TabName { get; set; }
    }
}