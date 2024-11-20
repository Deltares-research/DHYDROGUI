using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class InitialConditionRegion
    {
        public const string ChannelInitialConditionDefinitionIniHeader = "Branch";
        public const string GlobalDefinitionIniHeader = "Global";
        public const string InitialConditionIniHeader = "Initial";
        public const string ParameterIniHeader = "Parameter";

        public static readonly ConfigurationSetting Quantity = new ConfigurationSetting(key: "quantity", description: "");
        public static readonly ConfigurationSetting Unit = new ConfigurationSetting(key: "unit", description: "");
        public static readonly ConfigurationSetting Value = new ConfigurationSetting(key: "value", description: "", format: "F3");
        public static readonly ConfigurationSetting NumLocations = new ConfigurationSetting(key: "numLocations", description: "", format: "F0");
        public static readonly ConfigurationSetting Values = new ConfigurationSetting(key: "values", description: "", format: "F5");
        public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "branchId", description: "");
        public static readonly ConfigurationSetting Chainage = new ConfigurationSetting(key: "chainage", description: "", format:"F3");
        public static readonly ConfigurationSetting DataFile = new ConfigurationSetting(key: "dataFile", description: "");
        public static readonly ConfigurationSetting DataFileType = new ConfigurationSetting(key: "dataFileType", description: "");
        public static readonly ConfigurationSetting InterpolationMethod = new ConfigurationSetting(key: "interpolationMethod", description: "");
        public static readonly ConfigurationSetting Operand = new ConfigurationSetting(key: "operand", description: "");
        public static readonly ConfigurationSetting AveragingType = new ConfigurationSetting(key: "averagingType", description: "");
        public static readonly ConfigurationSetting AveragingRelSize = new ConfigurationSetting(key: "averagingRelSize", description: "");
        public static readonly ConfigurationSetting AveragingNumMin = new ConfigurationSetting(key: "averagingNumMin", description: "");
        public static readonly ConfigurationSetting AveragingPercentile = new ConfigurationSetting(key: "averagingPercentile", description: "");
        public static readonly ConfigurationSetting ExtrapolationMethod = new ConfigurationSetting(key: "extrapolationMethod", description: "");
        public static readonly ConfigurationSetting LocationType = new ConfigurationSetting(key: "locationType", description: "");
    }
}