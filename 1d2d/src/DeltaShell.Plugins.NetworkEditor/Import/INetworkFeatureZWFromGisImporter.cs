namespace DeltaShell.Plugins.NetworkEditor.Import
{
    /// <summary>
    /// Interface for ZW for Gis importer.
    /// </summary>
    public interface INetworkFeatureZwFromGisImporter
    {
        /// <summary>
        /// Number of levels.
        /// </summary>
        int NumberOfLevels { get; set; }
    }
}