using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers
{
    [TestFixture]
    public class MdwFileBackwardsCompatibilityConfigurationValuesTest : IDelftIniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected override IEnumerable<string> ObsoleteProperties { get; } =
            new HashSet<string>();

        protected override IEnumerable<KeyValuePair<string, string>> LegacyPropertyMapping { get; } =
            new Dictionary<string, string> {{"tscale", "TimeInterval"}};

        protected override IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping { get; } =
            new Dictionary<string, string>();

        protected override IDelftIniBackwardsCompatibilityConfigurationValues GetConfigurationValues() =>
            new MdwFileBackwardsCompatibilityConfigurationValues();
    }
}