using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Location
{
    public static class LocationRegion
    {
        // Common Location Properties
        public static readonly ConfigurationSetting Id = new ConfigurationSetting(key: "id", description: "Unique branch feature id");
        public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "branchid", description: "Branch on which the branch feature is located");
        public static readonly ConfigurationSetting Chainage = new ConfigurationSetting(key: "chainage", description: "Location on the branch (m)");
        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Long name in the user interface");

        public static readonly ConfigurationSetting PipeId = new ConfigurationSetting(key: "Id", description: "Unique location id for pipe cross sections");
        public static readonly ConfigurationSetting Branch = new ConfigurationSetting(key: "Branch", description: "Unique pipe id");
        public static readonly ConfigurationSetting PipeChainage = new ConfigurationSetting(key: "Chainage", description: "Location on the branch (m)");
        public static readonly ConfigurationSetting Shift = new ConfigurationSetting(key: "Shift", description: "Level shift of the cross section definition (m)");
        public static readonly ConfigurationSetting Definition = new ConfigurationSetting(key: "Definition", description: "Id of cross section definition");
    }

    public static class LateralSourceLocationRegion
    {
        public static readonly ConfigurationSetting Length = new ConfigurationSetting(key: "length", description: "(Optional, default=0) If greater than 0 : Length of the diffuse lateral source (m)");
    }

    public static class ObservationPointRegion
    {
        public const string IniHeader = "ObservationPoint";
        public const string ObservationPointType = "observationpoint";
    }

    public static class BoundaryRegion
    {
        public const string BoundaryHeader = "Boundary";
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
            public const string Zw = "tabulated";
            public const string Standard = "standard";
            public const string Elliptical = "ellipse";
            public const string Circle = "circle";
            public const string Rectangle = "rectangle";
            public const string Egg = "egg";
            public const string Arch = "arch";
            public const string Cunette = "cunette";
            public const string SteelCunette = "steelcunette";
            public const string Trapezium = "trapezium";
        }

        public static readonly ConfigurationSetting Shift = new ConfigurationSetting(key: "shift", description: "Level shift of the cross section definition(m)");
        public static readonly ConfigurationSetting Definition = new ConfigurationSetting(key: "definition", description: "Id of cross section definition");
    }
}
