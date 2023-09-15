namespace DHYDRO.Common.IO.Ini.Configuration
{
    /// <summary>
    /// Represents the configuration for merging INI files.
    /// </summary>
    public sealed class IniMergeConfiguration
    {
        /// <summary>
        /// Gets or sets a value that indicates whether new sections should be added
        /// to the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool AddAddedSections { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether new properties should be added
        /// to the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool AddAddedProperties { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether removed sections should be removed
        /// from the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveRemovedSections { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether removed properties should be removed
        /// from the merge result. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveRemovedProperties { get; set; } = true;
    }
}