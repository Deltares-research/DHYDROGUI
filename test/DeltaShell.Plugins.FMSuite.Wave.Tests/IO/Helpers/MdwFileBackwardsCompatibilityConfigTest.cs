using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers
{
    [TestFixture]
    public class MdwFileBackwardsCompatibilityConfigTest : IDelftIniBackwardsCompatibilityConfigTestFixture
    {
        protected override IDelftIniBackwardsCompatibilityConfig GetConfig() =>
            new MdwFileBackwardsCompatibilityConfig();

        protected override IEnumerable<string> ObsoleteProperties { get; } = 
            new HashSet<string>();

        protected override IEnumerable<KeyValuePair<string, string>> LegacyPropertyMapping { get; } =
            new Dictionary<string, string> {{"tscale", "TimeInterval"}};

        protected override IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping { get; } =
            new Dictionary<string, string>();
    }
}