using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Retention
{
    public static class RetentionRegion
    {
        public const string Header = "Retention";
        public static readonly ConfigurationSetting Id = new ConfigurationSetting(key: "id", description: "Unique network id");
        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Given name in the user interface");
        
        public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "branchId", description: "");
        public static readonly ConfigurationSetting NodeId = new ConfigurationSetting(key: "nodeId", description: "");
        public static readonly ConfigurationSetting X = new ConfigurationSetting(key: "x", description: "");
        public static readonly ConfigurationSetting Y = new ConfigurationSetting(key: "y", description: "");
        public static readonly ConfigurationSetting Chainage = new ConfigurationSetting(key: "chainage", description: "", format:"F6");
        public static readonly ConfigurationSetting StorageType = new ConfigurationSetting(key: "storageType", description: "Possible values: Reservoir (default), Closed and Loss");
        public static readonly ConfigurationSetting UseTable = new ConfigurationSetting(key: "useTable", description: "0=false, 1=true");

        //usetable == false (manhole)
        public static readonly ConfigurationSetting BedLevel = new ConfigurationSetting(key: "bedLevel", description: "Bed level of the retention area");
        public static readonly ConfigurationSetting Area = new ConfigurationSetting(key: "area", description: "Area at the bed level");
        public static readonly ConfigurationSetting StreetLevel = new ConfigurationSetting(key: "streetLevel", description: "Street level of the retention area");
        public static readonly ConfigurationSetting StreetStorageArea = new ConfigurationSetting(key: "streetStorageArea", description: "Area at street level");

        //usetable == true (retention)
        public static readonly ConfigurationSetting NumLevels = new ConfigurationSetting(key: "numLevels", description: "Number of levels in table");
        public static readonly ConfigurationSetting Levels = new ConfigurationSetting(key: "levels", description: "Levels / Levels (m AD)");
        public static readonly ConfigurationSetting StorageArea = new ConfigurationSetting(key: "storageArea", description: "Areas in storage area table");
        public static readonly ConfigurationSetting Interpolate = new ConfigurationSetting(key: "interpolate", description: "interpolation type 0 = linear (default), 1 = block function");
    }
}