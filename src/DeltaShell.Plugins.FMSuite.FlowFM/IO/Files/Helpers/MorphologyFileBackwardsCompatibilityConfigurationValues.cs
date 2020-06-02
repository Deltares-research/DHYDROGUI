using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    /// <summary>
    /// <see cref="MorphologyFileBackwardsCompatibilityConfigurationValues"/> defines the obsolete and legacy categories
    /// and properties for the <see cref="MorphologyFile"/>
    /// </summary>
    /// <seealso cref="IDelftIniBackwardsCompatibilityConfigurationValues"/>
    public sealed class MorphologyFileBackwardsCompatibilityConfigurationValues : IDelftIniBackwardsCompatibilityConfigurationValues
    {
        public ISet<string> ObsoleteProperties { get; } = new HashSet<string>()
        {
            "neubcmud",
            "neubcsand",
            "eqmbc"
        };

        public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; } = new Dictionary<string, string>()
        {
            {"bslhd", "Bshld"}
        };

        public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; } = new Dictionary<string, string>();
    }
}