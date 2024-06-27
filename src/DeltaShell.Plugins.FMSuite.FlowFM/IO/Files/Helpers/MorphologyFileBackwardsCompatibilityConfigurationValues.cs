using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.IO.Ini.BackwardCompatibility;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    /// <summary>
    /// <see cref="MorphologyFileBackwardsCompatibilityConfigurationValues"/> defines the obsolete and legacy sections
    /// and properties for the <see cref="MorphologyFile"/>
    /// </summary>
    /// <seealso cref="IIniBackwardsCompatibilityConfigurationValues"/>
    public sealed class MorphologyFileBackwardsCompatibilityConfigurationValues : IIniBackwardsCompatibilityConfigurationValues
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

        public IReadOnlyDictionary<string, string> LegacySectionMapping { get; } = new Dictionary<string, string>();

        public IEnumerable<IniPropertyInfo> UnsupportedPropertyValues { get; } = Enumerable.Empty<IniPropertyInfo>();
    }
}