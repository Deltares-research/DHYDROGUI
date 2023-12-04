using System.Collections.Generic;
using System.Linq;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers
{
    /// <summary>
    /// <see cref="MdwFileBackwardsCompatibilityConfigurationValues"/> defines the obsolete and legacy categories
    /// and properties for the <see cref="MdwFile"/>
    /// </summary>
    /// <seealso cref="IIniBackwardsCompatibilityConfigurationValues"/>
    public sealed class MdwFileBackwardsCompatibilityConfigurationValues : IIniBackwardsCompatibilityConfigurationValues
    {
        public ISet<string> ObsoleteProperties { get; } = new HashSet<string>();

        public IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; } = 
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; } =
            new Dictionary<string, NewPropertyData>
            {
                {"tscale", new NewPropertyData("TimeInterval", new DefaultPropertyUpdater())}
            };

        public IReadOnlyDictionary<string, string> LegacySectionMapping { get; } = new Dictionary<string, string>();

        public IEnumerable<IniPropertyInfo> UnsupportedPropertyValues { get; } = Enumerable.Empty<IniPropertyInfo>();
    }
}