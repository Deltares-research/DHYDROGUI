using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public static class MduFileLegacyPropertyDeterminator
    {
        private static readonly string[] legacyPropertyNames = 
        {
            "hdam"
        };

        public static bool IsLegacyPropertyName(string propertyName)
        {
            return legacyPropertyNames.Contains(propertyName.ToLowerInvariant());
        }
    }
}