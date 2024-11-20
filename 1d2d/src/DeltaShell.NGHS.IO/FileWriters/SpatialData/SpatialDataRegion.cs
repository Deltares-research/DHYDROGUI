using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.SpatialData
{
    public static class SpatialDataRegion
    {
        public const string ContentIniHeader = "Content";
        public static readonly ConfigurationSetting Quantity = new ConfigurationSetting(key: "quantity", description: "Possible values: InitialWaterLevel, InitialWaterDepth, InitialDischarge, InitialSalinity, Dispersion, WindShielding, ThatcherHarleman");
        public static readonly ConfigurationSetting Interpolate = new ConfigurationSetting(key: "interpolate", description: "0=false, 1=true");

        public const string DefinitionIniHeader = "Definition";
        public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "branchId", description: "");
        public static readonly ConfigurationSetting Chainage = new ConfigurationSetting(key: "chainage", description: "", format: "F6");
        public static readonly ConfigurationSetting Value = new ConfigurationSetting(key: "value", description: "", format:"F5");
    }
}