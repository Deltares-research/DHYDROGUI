using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers
{
    /// <summary>
    /// <see cref="MduFileBackwardsCompatibilityConfigurationValues"/> defines the obsolete and legacy sections
    /// and properties for the <see cref="MduFile"/>
    /// </summary>
    /// <seealso cref="IDelftIniBackwardsCompatibilityConfigurationValues"/>
    public sealed class MduFileBackwardsCompatibilityConfigurationValues : IDelftIniBackwardsCompatibilityConfigurationValues
    {
        public ISet<string> ObsoleteProperties { get; } = new HashSet<string>()
        {
            "hdam",
            "writebalancefile",
            "transportmethod",
            "transporttimestepping"
        };

        public IReadOnlyDictionary<string, string> ConditionalObsoleteProperties { get; } = new Dictionary<string, string>()
        {
            { "tstart", "StartDateTime" },
            { "tstop", "StopDateTime" }
        };

        public IReadOnlyDictionary<string, NewPropertyData> LegacyPropertyMapping { get; } = new Dictionary<string, NewPropertyData>()
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

        public IReadOnlyDictionary<string, string> LegacySectionMapping { get; } = new Dictionary<string, string>() {{"model", "General"}};
    }
}