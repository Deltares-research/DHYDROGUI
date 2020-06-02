using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers
{
    /// <summary>
    /// <see cref="MdwFileBackwardsCompatibilityConfigurationValues"/> defines the obsolete and legacy categories
    /// and properties for the <see cref="MdwFile"/>
    /// </summary>
    /// <seealso cref="IDelftIniBackwardsCompatibilityConfigurationValues"/>
    public sealed class MdwFileBackwardsCompatibilityConfigurationValues : IDelftIniBackwardsCompatibilityConfigurationValues
    {
        public ISet<string> ObsoleteProperties { get; } = new HashSet<string>();

        public IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; } =
            new Dictionary<string, string> {{"tscale", "TimeInterval"}};

        public IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; } =
            new Dictionary<string, string>();
    }
}