using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Roughness
{
    public static class RoughnessDataRegion
    {
        public const string GlobalIniHeader = "Global";
        public static readonly ConfigurationSetting SectionId = new ConfigurationSetting(key: "frictionId", description: "The name of the roughness section", defaultValue: "Channels");
        public static readonly ConfigurationSetting FrictionType = new ConfigurationSetting(key: "frictionType", description: "The global roughness type for this variable which is used if no branch specific roughness definition is given.See the table at the beginning of this section for the encoding of the different roughness types.");
        public static readonly ConfigurationSetting FrictionValue = new ConfigurationSetting(key: "frictionValue", description: "The global default value for this roughness variable");
        
        public static readonly ConfigurationSetting FlowDirection = new ConfigurationSetting(key: "flowDirection",defaultValue: "0", 
            description: "Type of flow direction, possible values: Normal = 0 (false), Reverse = 1 (true)");
        
        public static readonly ConfigurationSetting Interpolate = new ConfigurationSetting(key: "interpolate", description: "0=false, 1=true");

        public static readonly ConfigurationSetting GlobalType = new ConfigurationSetting(key: "globalType",
            description:"Type of roughness definition, possible values: Chezy = 1, Manning = 4, Nikuradse = 5, Strickler = 6, WhiteColebrook = 7, BosBijkerk = 9");
        public static readonly ConfigurationSetting GlobalValue = new ConfigurationSetting(key: "globalValue", description: "The global default value for this section");

        public const string BranchPropertiesIniHeader = "Branch";
        //do not use this branchid but the one of the base classs SpatialDataRegion!!

        public static readonly ConfigurationSetting RoughnessType = new ConfigurationSetting(key: "frictionType",
            description: "Type of roughness definition, possible values: Chezy = 1, Manning = 4, Nikuradse = 5, Strickler = 6, WhiteColebrook = 7, BosBijkerk = 9");

        public static readonly ConfigurationSetting FunctionType = new ConfigurationSetting(key: "functionType",
            description: "Function type for the calculation of the value, possible values: Constant = 0, FunctionOfDischarge = 1, FunctionOfWaterLevel = 2");

        public static readonly ConfigurationSetting NumberOfLevels = new ConfigurationSetting(key: "numLevels", description: "Number of levels in table");
        public static readonly ConfigurationSetting Levels = new ConfigurationSetting(key: "levels");

        public static readonly ConfigurationSetting NumberOfLocations = new ConfigurationSetting(key: "numLocations", description: "Number of locations in branch");

        public const string DefinitionIniHeader = "Definition";
        //Use the one of the base classs SpatialDataRegion!!
        public static readonly ConfigurationSetting Values = new ConfigurationSetting(key: "frictionValues", description: "", format:"F5");
    }
}