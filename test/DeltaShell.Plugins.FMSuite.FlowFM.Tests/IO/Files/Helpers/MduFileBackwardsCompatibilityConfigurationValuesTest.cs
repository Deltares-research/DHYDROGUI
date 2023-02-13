using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class MduFileBackwardsCompatibilityConfigurationValuesTest : IDelftIniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected override IEnumerable<string> ObsoleteProperties => new HashSet<string> {"hdam","writebalancefile", "transportmethod" };

        protected override IEnumerable<KeyValuePair<string, string>> LegacyPropertyMapping =>
            new Dictionary<string, string>
            {
                {"enclosurefile", "GridEnclosureFile"},
                {"trtdt", "DtTrt"},
                {"botlevuni", "BedLevUni"},
                {"botlevtype", "BedLevType"},
                {"mduformatversion", "FileVersion"},
                {"locationfile", "locationFile"},
                {"forcingfile", "forcingFile"},
                {"return_time", "returnTime"}
            };

        protected override IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping =>
            new Dictionary<string, string> {{"model", "General"}};

        protected override IDelftIniBackwardsCompatibilityConfigurationValues GetConfigurationValues() =>
            new MduFileBackwardsCompatibilityConfigurationValues();
    }
}