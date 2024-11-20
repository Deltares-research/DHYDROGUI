using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class DefinitionPropertySettings
    {
        public const string Header = "Definition";

        public static readonly ConfigurationSetting DefinitionType = new ConfigurationSetting(key: "Type");
        public static readonly ConfigurationSetting Id = new ConfigurationSetting(key: "Id", description: "unique definition id");
        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Given name in the user interface");
        public static readonly ConfigurationSetting LevelCount = new ConfigurationSetting(key: "levelsCount");

        public static readonly ConfigurationSetting XYZCount = new ConfigurationSetting(key: "xyzCount", description: "Number of XYZ-coordinates");
        public static readonly ConfigurationSetting XCoors = new ConfigurationSetting(key: "xCoordinates", description: "x-coordinates of the definition", format: "F5");
        public static readonly ConfigurationSetting YCoors = new ConfigurationSetting(key: "yCoordinates", description: "y-coordinates of the definition", format: "F5");
        public static readonly ConfigurationSetting ZCoors = new ConfigurationSetting(key: "zCoordinates", description: "z-coordinates of the definition", format: "F5");

        public static readonly ConfigurationSetting SingleValuedZ = new ConfigurationSetting(key: "singleValuedZ", description: "", defaultValue:"1");
        public static readonly ConfigurationSetting YZCount = new ConfigurationSetting(key: "yzCount", description: "Number of YZ-coordinates");
        
        public static readonly ConfigurationSetting Thalweg = new ConfigurationSetting(key: "Thalweg", description: "(GUI ONLY)");

        public static readonly ConfigurationSetting NumLevels = new ConfigurationSetting(key: "numLevels", description: "Number of levels in table");
        public static readonly ConfigurationSetting Levels = new ConfigurationSetting(key: "levels", description: "Levels / Levels (m AD)", format: "F5");

        public static readonly ConfigurationSetting FlowWidths = new ConfigurationSetting(key: "flowWidths", description: "Flow widths at levels. (m)", format: "F5");
        public static readonly ConfigurationSetting TotalWidths = new ConfigurationSetting(key: "totalWidths", description: "Total widths at levels. (m)", format: "F5");
        
        public static readonly ConfigurationSetting GroundlayerUsed = new ConfigurationSetting(key: "GroundLayerUsed", description: "Flag for ground layer (0=None, <>0=Yes)");
        public static readonly ConfigurationSetting Groundlayer = new ConfigurationSetting(key: "GroundLayer", description: "Thickness of ground layer (m))");
        
        public static readonly ConfigurationSetting CrestLevee = new ConfigurationSetting(key: "leveeCrestLevel", description: "Levee crest level of (m AD)");
        public static readonly ConfigurationSetting BaseLevelLevee = new ConfigurationSetting(key: "leveeBaseLevel", description: "Levee dike base level(m AD)");
        public static readonly ConfigurationSetting FlowAreaLevee = new ConfigurationSetting(key: "leveeFlowArea", description: "Flow area behind Levee. (m2)");
        public static readonly ConfigurationSetting TotalAreaLevee = new ConfigurationSetting(key: "leveeTotalArea", description: "Total area behind Levee. (m2)");
        
        public static readonly ConfigurationSetting Main = new ConfigurationSetting(key: "mainWidth", description: "Width of main section. (m)");
        public static readonly ConfigurationSetting FloodPlain1 = new ConfigurationSetting(key: "fp1Width", description: "Width of Floodplain 1 (m)");
        public static readonly ConfigurationSetting FloodPlain2 = new ConfigurationSetting(key: "fp2Width", description: "Width of Floodplain 2 (m)");

        public static readonly ConfigurationSetting Conveyance = new ConfigurationSetting(key: "conveyance", description: "No comments", defaultValue:"segmented");
        public static readonly ConfigurationSetting SectionCount = new ConfigurationSetting(key: "sectionCount", description: "Number of friction sections");
        public static readonly ConfigurationSetting FrictionPositions = new ConfigurationSetting(key: "frictionPositions", description: "Location where the roughness sections start and end. Always one location more than sectionCount. The first value should equal 0 and the last value should equal the crosssection length. Keyword may be skipped if sectionCount = 1.", format: "F5");
        public static readonly ConfigurationSetting FrictionIds = new ConfigurationSetting(key: "frictionIds", description: "Names of the friction sections.");
        public static readonly ConfigurationSetting FrictionId = new ConfigurationSetting(key: "frictionId", description: "Names of the friction section.");

        public static readonly ConfigurationSetting Diameter = new ConfigurationSetting(key: "Diameter", description: "Diameter of the circle (m)");
        public static readonly ConfigurationSetting EllipseHeight = new ConfigurationSetting(key: "Height", description: "Height of the ellipse (m)");
        public static readonly ConfigurationSetting EllipseWidth = new ConfigurationSetting(key: "Width", description: "Height of the ellipse (m)");

        public static readonly ConfigurationSetting Template = new ConfigurationSetting(key: "template", description: "Name of ZW cross section template");

        public static readonly ConfigurationSetting RectangleHeight = new ConfigurationSetting(key: "Height", description: "Height of the rectangle (m)");
        public static readonly ConfigurationSetting RectangleWidth = new ConfigurationSetting(key: "Width", description: "Width of the rectangle (m)");
        public static readonly ConfigurationSetting Closed = new ConfigurationSetting(key: "Closed", description: "1 = closed channel, 0 = open");
        
        public static readonly ConfigurationSetting EggWidth = new ConfigurationSetting(key: "Width", description: "Width of the egg profile (m)");
        public static readonly ConfigurationSetting EggHeight = new ConfigurationSetting(key: "Height", description: "Height of the egg profile (m)");

        public static readonly ConfigurationSetting ArchCrossSectionWidth = new ConfigurationSetting(key: "Width", description: "Width of the cross section (m)");
        public static readonly ConfigurationSetting ArchCrossSectionHeight = new ConfigurationSetting(key: "Height", description: "Height of cross section (m)");
        public static readonly ConfigurationSetting ArchHeight = new ConfigurationSetting(key: "ArcHeight", description: "Height of arch (m)");
        
        public static readonly ConfigurationSetting CunetteWidth = new ConfigurationSetting(key: "Width", description: "Width of the cunette (m)");
        public static readonly ConfigurationSetting CunetteHeight = new ConfigurationSetting(key: "Height", description: "Height of the cunette (m)");

        public static readonly ConfigurationSetting SteelCunetteHeight = new ConfigurationSetting(key: "Height", description: "Height of the cunette (m)");
        public static readonly ConfigurationSetting SteelCunetteR = new ConfigurationSetting(key: "R", description: "Radius r (m))");
        public static readonly ConfigurationSetting SteelCunetteR1 = new ConfigurationSetting(key: "R1", description: "Radius r1 (m)");
        public static readonly ConfigurationSetting SteelCunetteR2 = new ConfigurationSetting(key: "R2", description: "Radius r2 (m))");
        public static readonly ConfigurationSetting SteelCunetteR3 = new ConfigurationSetting(key: "R3", description: "Radius r3 (m)");
        public static readonly ConfigurationSetting SteelCunetteA = new ConfigurationSetting(key: "A", description: "Radius a (m)");
        public static readonly ConfigurationSetting SteelCunetteA1 = new ConfigurationSetting(key: "A1", description: "Radius a1 (m)");

        public static readonly ConfigurationSetting Slope = new ConfigurationSetting(key: "Slope", description: "Slope of trapezium (m)");
        public static readonly ConfigurationSetting MaximumFlowWidth = new ConfigurationSetting(key: "width", description: "Maximum flow width of trapezium (m)");
        public static readonly ConfigurationSetting BottomWidth = new ConfigurationSetting(key: "baseWidth", description: "Bottom width of trapezium (m)");

        public static readonly ConfigurationSetting IsShared = new ConfigurationSetting(key: "IsShared", description: "(Optional, default=false=0) if is shared set to true=1");
    }
}
