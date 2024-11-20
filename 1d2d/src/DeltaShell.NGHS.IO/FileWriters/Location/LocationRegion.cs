using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public static class LocationRegion
    {
        // Common Location Properties
        public static readonly ConfigurationSetting Id = new ConfigurationSetting(key: "Id", description: "Unique branch feature id");
        public static readonly ConfigurationSetting ObsId = new ConfigurationSetting(key: "name", description: "Unique branch feature id");
        public static readonly ConfigurationSetting Chainage = new ConfigurationSetting(key: "chainage", description: "Location on the branch (m)", format: "G17");
        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Long name in the user interface");

        public static readonly ConfigurationSetting PipeId = new ConfigurationSetting(key: "Id", description: "Unique location id for pipe cross sections");
        public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "branchId", description: "Unique pipe id");
        public static readonly ConfigurationSetting PipeChainage = new ConfigurationSetting(key: "chainage", description: "Location on the branch (m)", format:"G17");
        public static readonly ConfigurationSetting Shift = new ConfigurationSetting(key: "shift", description: "Level shift of the cross section definition (m)");
        public static readonly ConfigurationSetting Definition = new ConfigurationSetting(key: "definitionId", description: "Id of cross section definition");
    }

    public static class LateralSourceLocationRegion
    {
        public static readonly ConfigurationSetting Length = new ConfigurationSetting(key: "length", description: "(Optional, default=0) If greater than 0 : Length of the diffuse lateral source (m)");
    }

    public static class ObservationPointRegion
    {
        public const string IniHeader = "ObservationPoint";
        public const string IniHeaderCrs = "ObservationCrossSection";
        public const string ObservationPointType = "observationpoint";
    }

    public static class BoundaryRegion
    {
        public const string BoundaryHeader = "Boundary";
        public const string LateralHeader = "Lateral";
        public const string LateralDischargeHeader = "LateralDischarge";

        public static readonly ConfigurationSetting NodeId = new ConfigurationSetting(key: "nodeId", description: "Node on which the boundary is located");
        public static readonly ConfigurationSetting Type = new ConfigurationSetting(key: "type", description: "Boundary type");
        public static readonly ConfigurationSetting ThatcherHarlemanCoeff = new ConfigurationSetting(key: "thatcher-harlemancoeff",
            description: "for salt boundaries: thatcher-harlemancoeff time lag in seconds");
    }

    public static class CrossSectionRegion
    {
        public const string IniHeader = "CrossSection";

        public static class CrossSectionDefinitionType
        {
            public const string Yz = "yz";
            public const string Xyz = "xyz";
            public const string Zw = "zwRiver"; 
            public const string Zw_Template = "zw"; 
            public const string Standard = "standard";
            public const string Elliptical = "ellipse";
            public const string Circle = "circle";
            public const string Rectangle = "rectangle";
            public const string Egg = "egg";
            public const string InvertedEgg = "InvEgg";
            public const string Arch = "arch";
            public const string UShape = "uShape";
            public const string Cunette = "cunette";
            public const string SteelCunette = "steelcunette";

            public const string Mouth = "mouth";
            public const string SteelMouth = "steelMouth";

            public const string Trapezium = "trapezium";
        }
    }
}
