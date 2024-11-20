namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public enum ShortCutType
    {
        /// <summary>
        /// A tab on the settings view
        /// </summary>
        SettingsTab,
        /// <summary>
        /// An unstructured grid shortcut -> used for opening an instance of the grid editor
        /// </summary>
        Grid,
        /// <summary>
        /// Spatial coverage -> used for setting spatial editor to this coverage
        /// </summary>
        SpatialCoverage,
        /// <summary>
        /// A set of features -> used for opening attribute table
        /// </summary>
        FeatureSet,
        Default
    }
}