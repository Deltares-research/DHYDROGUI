using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// <see cref="IDelftIniBackwardsCompatibilityConfig"/> defines the legacy
    /// mappings and obsolete categories and properties.
    /// </summary>
    public interface IDelftIniBackwardsCompatibilityConfig
    {
        /// <summary>
        /// Gets the obsolete properties.
        /// </summary>
        ISet<string> ObsoleteProperties { get; }

        /// <summary>
        /// Gets the legacy mapping of properties.
        /// </summary>
        IReadOnlyDictionary<string, string> LegacyPropertyMapping { get; }

        /// <summary>
        /// Gets the legacy mapping of categories.
        /// </summary>
        IReadOnlyDictionary<string, string> LegacyCategoryMapping { get; }
    }
}
