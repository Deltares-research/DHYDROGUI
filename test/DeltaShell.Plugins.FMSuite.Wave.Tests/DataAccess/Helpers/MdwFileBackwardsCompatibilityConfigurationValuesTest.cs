using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers;
using DHYDRO.Common.IO.Ini.BackwardCompatibility;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers
{
    [TestFixture]
    public class MdwFileBackwardsCompatibilityConfigurationValuesTest : IniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected override IEnumerable<string> ObsoleteProperties { get; } =
            new HashSet<string>();

        protected override IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; } = 
            new Dictionary<string, string>();

        protected override IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; } =
            new Dictionary<string, NewPropertyData>
            {
                {"tscale", new NewPropertyData("TimeInterval", new DefaultPropertyUpdater())}
            };

        protected override IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping { get; } =
            new Dictionary<string, string>();

        protected override IIniBackwardsCompatibilityConfigurationValues GetConfigurationValues() =>
            new MdwFileBackwardsCompatibilityConfigurationValues();
    }
}