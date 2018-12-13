using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO
{
    public static class SalinityRegion
    {
        public const string MouthHeader = "Mouth";
        public static readonly ConfigurationSetting NodeId = new ConfigurationSetting(key: "nodeId", description: "#Estuary mouth node id");

        //TODO: Give the following ConfigurationSetting objects a correct description
        public const string NumericalOptionsHeader = "NumericalOptions";
        public static readonly ConfigurationSetting Teta = new ConfigurationSetting(key: "teta");
        public static readonly ConfigurationSetting TidalPeriod = new ConfigurationSetting(key: "tidalPeriod");
        public static readonly ConfigurationSetting AdvectionScheme = new ConfigurationSetting(key: "advectionScheme");
        public static readonly ConfigurationSetting C3 = new ConfigurationSetting(key: "c3");
        public static readonly ConfigurationSetting C4 = new ConfigurationSetting(key: "c4");
        public static readonly ConfigurationSetting C5 = new ConfigurationSetting(key: "c5");
        public static readonly ConfigurationSetting C6 = new ConfigurationSetting(key: "c6");
        public static readonly ConfigurationSetting C7 = new ConfigurationSetting(key: "c7");
        public static readonly ConfigurationSetting C8 = new ConfigurationSetting(key: "c8");
        public static readonly ConfigurationSetting C9 = new ConfigurationSetting(key: "c9");
        public static readonly ConfigurationSetting C10 = new ConfigurationSetting(key: "c10");
        public static readonly ConfigurationSetting C11 = new ConfigurationSetting(key: "c11");
    }
}
