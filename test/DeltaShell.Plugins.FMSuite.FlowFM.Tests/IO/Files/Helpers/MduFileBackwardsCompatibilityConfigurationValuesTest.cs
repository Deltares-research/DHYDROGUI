using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.Common.Tests.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.Helpers
{
    [TestFixture]
    public class MduFileBackwardsCompatibilityConfigurationValuesTest : IDelftIniBackwardsCompatibilityConfigurationValuesTestFixture
    {
        protected override IEnumerable<string> ObsoleteProperties => new HashSet<string>
        {
            "hdam",
            "writebalancefile",
            "transportmethod",
            "transporttimestepping"
        };

        protected override IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; } =
            new Dictionary<string, string>()
            {
                { "tstart", "StartDateTime" },
                { "tstop", "StopDateTime" }
            };

        protected override IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping =>
            new Dictionary<string, NewPropertyData>
            {
                {"enclosurefile", new NewPropertyData("GridEnclosureFile", new DefaultPropertyUpdater())},
                {"trtdt", new NewPropertyData("DtTrt", new DefaultPropertyUpdater())},
                {"botlevuni", new NewPropertyData("BedLevUni", new DefaultPropertyUpdater())},
                {"botlevtype", new NewPropertyData("BedLevType", new DefaultPropertyUpdater())},
                {"mduformatversion", new NewPropertyData("FileVersion", new DefaultPropertyUpdater())},
                {"locationfile", new NewPropertyData("locationFile", new DefaultPropertyUpdater())},
                {"forcingfile", new NewPropertyData("forcingFile", new DefaultPropertyUpdater())},
                {"return_time", new NewPropertyData("returnTime", new DefaultPropertyUpdater())},
                {"tstart", new NewPropertyData("StartDateTime", new LegacyStartAndStopTimeUpdater())},
                {"tstop", new NewPropertyData("StopDateTime", new LegacyStartAndStopTimeUpdater())},
            };

        protected override IEnumerable<KeyValuePair<string, string>> LegacyCategoryMapping =>
            new Dictionary<string, string> {{"model", "General"}};

        protected override IDelftIniBackwardsCompatibilityConfigurationValues GetConfigurationValues() =>
            new MduFileBackwardsCompatibilityConfigurationValues();
    }
}