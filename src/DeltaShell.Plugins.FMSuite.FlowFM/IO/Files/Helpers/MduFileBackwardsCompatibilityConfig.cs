using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    /// <summary>
    /// <see cref="MduFileBackwardsCompatibilityConfig"/> defines the obsolete and legacy categories
    /// and properties for the <see cref="MduFile"/>
    /// </summary>
    /// <seealso cref="IDelftIniBackwardsCompatibilityConfig" />
    public sealed class MduFileBackwardsCompatibilityConfig : IDelftIniBackwardsCompatibilityConfig
    {
        public ISet<string> ObsoleteProperties { get; } = new HashSet<string>()
        {
            "hdam",
        };
        
        public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; } = new Dictionary<string, string>()
        {
            {"enclosurefile", "GridEnclosureFile"},
            {"trtdt", "DtTrt"},
            {"botlevuni", "BedLevUni"},
            {"botlevtype", "BedLevType"},
            {"mduformatversion", "FileVersion"}
        };

        public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; } = new Dictionary<string, string>()
        {
            {"model", "General" },
        };
    }
}