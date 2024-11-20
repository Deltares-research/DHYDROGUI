namespace DeltaShell.Plugins.FMSuite.Wave.Validation
{
    /// <summary>
    /// This class describes a shortcut that can be used to link a validation issue to a tab on the wave settings view.
    /// </summary>
    public class WaveValidationShortcut
    {
        /// <summary>
        /// The wave model that is used as data for the view that is to be opened.
        /// </summary>
        public WaveModel WaveModel { get; set; }

        /// <summary>
        /// The tab name of the wave settings view that needs to be opened.
        /// </summary>
        public string TabName { get; set; }
    }
}