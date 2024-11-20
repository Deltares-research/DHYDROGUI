using System.ComponentModel;

namespace DelftTools.Hydro.Structures
{
    public static class SewerProfileMapping
    {
        public enum SewerProfileType
        {
            [Description("Unknown")] Unknown,
            [Description("EIV")] Egg,
            [Description("EIG")] InvertedEgg,
            [Description("HEU")] Arch,
            [Description("MVR")] Cunette,
            [Description("OVA")] Elliptical,
            [Description("RHK")] Rectangle,
            [Description("TAB")] Tabulated,
            [Description("RND")] Circle,
            [Description("TPZ")] Trapezoid,
            [Description("UVR")] UShape,
            [Description("YZP")] YZ_Profile,
        }

        public enum SewerProfileMaterial
        {
            [Description("Unknown")] Unknown,
            [Description("BET")] Concrete,
            [Description("GIJ")] CastIron,
            [Description("GRE")] StoneWare,
            [Description("HDP")] Hdpe,
            [Description("MSW")] Masonry,
            [Description("PIJ")] SheetMetal,
            [Description("HPE")] Polyester,
            [Description("PVC")] Polyvinylchlorid,
            [Description("STL")] Steel
        }

        public static class PropertyKeys
        {
            public const string SewerProfileId = "CROSSSECTION_ID";
            public const string SewerProfileMaterial = "CROSS_SECTION_MATERIAL";
            public const string SewerProfileShape = "CROSS_SECTION_SHAPE";
            public const string SewerProfileWidth = "CROSS_SECTION_WIDTH";
            public const string SewerProfileHeight = "CROSS_SECTION_HEIGHT";
            public const string Slope1 = "SLOPE_1";
            public const string Slope2 = "SLOPE_2";
            public const string SewerProfileLevel = "CROSS_SECTION_LEVEL";
            public const string WetArea = "WET_AREA";
            public const string WetPerimeter = "WET_PERIMETER";
            public const string ACrossSectionWidth = "A_CROSS_SECTION_WIDTH";
            public const string Remarks = "REMARKS";
        }
    }
}