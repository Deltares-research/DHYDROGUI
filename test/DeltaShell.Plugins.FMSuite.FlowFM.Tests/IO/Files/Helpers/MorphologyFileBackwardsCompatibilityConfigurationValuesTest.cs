using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class MorphologyFileBackwardsCompatibilityConfigurationValuesTest : IniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected override IEnumerable<string> ObsoleteProperties => new HashSet<string>
        {
            "neubcmud",
            "neubcsand",
            "eqmbc"
        };

        protected override IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; } = 
            new Dictionary<string, string>();

        protected override IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping =>
            new Dictionary<string, NewPropertyData>
            {
                {"bslhd", new NewPropertyData("Bshld", new DefaultPropertyUpdater())}
            };

        protected override IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping =>
            new Dictionary<string, string>();

        protected override IIniBackwardsCompatibilityConfigurationValues GetConfigurationValues() =>
            new MorphologyFileBackwardsCompatibilityConfigurationValues();
    }
}