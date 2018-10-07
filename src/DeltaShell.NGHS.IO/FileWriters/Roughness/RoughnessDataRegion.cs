using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Roughness
{
    public static class RoughnessDataRegion
    {
        public const string ContentIniHeader = "Content";
        public static readonly ConfigurationSetting SectionId = new ConfigurationSetting(key: "sectionId", description: "The name of the roughness section");
        
        public static readonly ConfigurationSetting FlowDirection = new ConfigurationSetting(key: "flowDirection",defaultValue: "0", 
            description: "Type of flow direction, possible values: Normal = 0 (false), Reverse = 1 (true)");
        
        public static readonly ConfigurationSetting Interpolate = new ConfigurationSetting(key: "interpolate", description: "0=false, 1=true");

        public static readonly ConfigurationSetting GlobalType = new ConfigurationSetting(key: "globalType",
            description:"Type of roughness definition, possible values: Chezy = 1, Manning = 4, Nikuradse = 5, Strickler = 6, WhiteColebrook = 7, BosBijkerk = 9");
        public static readonly ConfigurationSetting GlobalValue = new ConfigurationSetting(key: "globalValue", description: "The global default value for this section");

        public const string BranchPropertiesIniHeader = "BranchProperties";
        //do not use this branchid but the one of the base classs SpatialDataRegion!!
        //public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "branchId", description: "The name of the branch");

        public static readonly ConfigurationSetting RoughnessType = new ConfigurationSetting(key: "roughnessType",
            description: "Type of roughness definition, possible values: Chezy = 1, Manning = 4, Nikuradse = 5, Strickler = 6, WhiteColebrook = 7, BosBijkerk = 9");

        public static readonly ConfigurationSetting FunctionType = new ConfigurationSetting(key: "functionType",
            description: "Function type for the calculation of the value, possible values: Constant = 0, FunctionOfDischarge = 1, FunctionOfWaterLevel = 2");

        public static readonly ConfigurationSetting NumberOfLevels = new ConfigurationSetting(key: "numLevels", description: "Number of levels in table");
        public static readonly ConfigurationSetting Levels = new ConfigurationSetting(key: "levels");

        public const string DefinitionIniHeader = "Definition";
        //Use the one of the base classs SpatialDataRegion!!
        //public static readonly ConfigurationSetting Chainage = new ConfigurationSetting(key: "chainage", description: "");
        //public static readonly ConfigurationSetting Value = new ConfigurationSetting(key: "value", description: "");
        public static readonly ConfigurationSetting Values = new ConfigurationSetting(key: "values", description: "", format:"F5");

        
    }
}