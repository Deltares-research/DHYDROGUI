using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// Determinator for legacy property names in mdu files.
    /// </summary>
    public static class MduFileLegacyPropertyDeterminator
    {
        private static readonly string[] legacyPropertyNames = 
        {
            "hdam"
        };

        /// <summary>
        /// Determines whether a property name is a legacy name.
        /// </summary>
        /// <param name="propertyName"> The property name. </param>
        /// <returns> True if legacy, else false. </returns>
        public static bool IsLegacyPropertyName(string propertyName)
        {
            return legacyPropertyNames.Contains(propertyName.ToLowerInvariant());
        }
    }
}