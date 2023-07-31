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

        public IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; } = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; } = new Dictionary<string, NewPropertyData>()
        {
            {"bslhd", new NewPropertyData("Bshld", new DefaultPropertyUpdater())}
        };

        public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; } = new Dictionary<string, string>();
    }
}