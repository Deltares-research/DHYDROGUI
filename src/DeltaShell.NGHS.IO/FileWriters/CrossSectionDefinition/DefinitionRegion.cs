using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class DefinitionRegion
    {
        public const string Header = "Definition";

        public static readonly ConfigurationSetting DefinitionType = new ConfigurationSetting(key: "type");
        public static readonly ConfigurationSetting Id = new ConfigurationSetting(key: "id", description: "unique definition id");
        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Given name in the user interface");
        public static readonly ConfigurationSetting LevelCount = new ConfigurationSetting(key: "levelsCount");

        public static readonly ConfigurationSetting XYZCount = new ConfigurationSetting(key: "xyzCount", description: "Number of XYZ-coordinates");
        public static readonly ConfigurationSetting XCoors = new ConfigurationSetting(key: "xCoors", description: "x-coordinates of the definition");
        public static readonly ConfigurationSetting YCoors = new ConfigurationSetting(key: "yCoors", description: "y-coordinates of the definition");
        public static readonly ConfigurationSetting ZCoors = new ConfigurationSetting(key: "zCoors", description: "z-coordinates of the definition");

        public static readonly ConfigurationSetting YZCount = new ConfigurationSetting(key: "yzCount", description: "Number of YZ-coordinates");
        public static readonly ConfigurationSetting YValues = new ConfigurationSetting(key: "yValues", description: "y-values as used in the computational core (m)");
        public static readonly ConfigurationSetting ZValues = new ConfigurationSetting(key: "zValues", description: "z-values as used in the computational core (m)");

        public static readonly ConfigurationSetting Thalweg = new ConfigurationSetting(key: "thalweg", description: "(GUI ONLY)");
        public static readonly ConfigurationSetting DeltaZStorage = new ConfigurationSetting(key: "deltaZStorage", description: "(GUI ONLY)");

        public static readonly ConfigurationSetting NumLevels = new ConfigurationSetting(key: "numLevels", description: "Number of levels in table");
        public static readonly ConfigurationSetting Levels = new ConfigurationSetting(key: "levels", description: "Levels / Levels (m AD)", format: "F5");

        public static readonly ConfigurationSetting FlowWidths = new ConfigurationSetting(key: "flowWidths", description: "Flow widths at levels. (m)", format: "F5");
        public static readonly ConfigurationSetting TotalWidths = new ConfigurationSetting(key: "totalWidths", description: "Total widths at levels. (m)", format: "F5");
        //public static readonly ConfigurationSetting StorageWidths = new ConfigurationSetting(key: "storageWidths", description: "Storage widths at levels. (m)");

        public static readonly ConfigurationSetting GroundlayerUsed = new ConfigurationSetting(key: "groundlayerUsed", description: "Flag for ground layer (0=None, <>0=Yes)");
        public static readonly ConfigurationSetting Groundlayer = new ConfigurationSetting(key: "groundlayer", description: "Thickness of ground layer (m))");
        
        public static readonly ConfigurationSetting CrestSummerdike = new ConfigurationSetting(key: "sd_crest", description: "Summer dike crest level of (m AD)");
        public static readonly ConfigurationSetting BaseLevelSummerdike = new ConfigurationSetting(key: "sd_baseLevel", description: "Summer dike base level(m AD)");
        public static readonly ConfigurationSetting FlowAreaSummerdike = new ConfigurationSetting(key: "sd_flowArea", description: "Flow area behind summerdike. (m2)");
        public static readonly ConfigurationSetting TotalAreaSummerdike = new ConfigurationSetting(key: "sd_totalArea", description: "Total area behind summerdike. (m2)");
        
        public static readonly ConfigurationSetting Main = new ConfigurationSetting(key: "main", description: "Width of main secion. (m)");
        public static readonly ConfigurationSetting FloodPlain1 = new ConfigurationSetting(key: "floodPlain1", description: "Width of Floodplain 1 (m)");
        public static readonly ConfigurationSetting FloodPlain2 = new ConfigurationSetting(key: "floodPlain2", description: "Width of Floodplain 2 (m)");

        public static readonly ConfigurationSetting SectionCount = new ConfigurationSetting(key: "sectionCount", description: "Number of friction sections");
        public static readonly ConfigurationSetting RoughnessNames = new ConfigurationSetting(key: "roughnessNames", description: "Names of the roughness sections.");
        public static readonly ConfigurationSetting RoughnessPositions = new ConfigurationSetting(key: "roughnessPositions", description: "Locations where the roughness section start and end.");
        public static readonly ConfigurationSetting RoughnessTypesPos = new ConfigurationSetting(key: "roughnessTypesPos", description: "Temporary array: Roughness type for each roughness section in positive direction");
        public static readonly ConfigurationSetting RoughnessValuesPos = new ConfigurationSetting(key: "roughnessValuesPos", description: "Temporary array: Roughness value for each roughness section in positive direction");
        public static readonly ConfigurationSetting RoughnessTypesNeg = new ConfigurationSetting(key: "roughnessTypesNeg", description: "Temporary array: Roughness type for each roughness section in negative direction");
        public static readonly ConfigurationSetting RoughnessValuesNeg = new ConfigurationSetting(key: "roughnessValuesNeg", description: "Temporary array: Roughness value for each roughness section in negative direction");
        
        public static readonly ConfigurationSetting Diameter = new ConfigurationSetting(key: "diameter", description: "Diameter of the circle (m)");
        public static readonly ConfigurationSetting EllipseHeight = new ConfigurationSetting(key: "height", description: "Height of the ellipse (m)");
        public static readonly ConfigurationSetting EllipseWidth = new ConfigurationSetting(key: "width", description: "Height of the ellipse (m)");

        public static readonly ConfigurationSetting RectangleHeight = new ConfigurationSetting(key: "height", description: "Height of the rectangle (m)");
        public static readonly ConfigurationSetting RectangleWidth = new ConfigurationSetting(key: "width", description: "Width of the rectangle (m)");
        public static readonly ConfigurationSetting Closed = new ConfigurationSetting(key: "closed", description: "1 = closed channel, 0 = open");
        
        public static readonly ConfigurationSetting EggWidth = new ConfigurationSetting(key: "width", description: "Width of the egg profile (m)");

        public static readonly ConfigurationSetting ArchCrossSectionWidth = new ConfigurationSetting(key: "width", description: "Width of the cross section (m)");
        public static readonly ConfigurationSetting ArchCrossSectionHeight = new ConfigurationSetting(key: "height", description: "Height of cross section (m)");
        public static readonly ConfigurationSetting ArchHeight = new ConfigurationSetting(key: "arcHeight", description: "Height of arch (m)");
        
        public static readonly ConfigurationSetting CunetteWidth = new ConfigurationSetting(key: "width", description: "Width of the cunette (m)");

        public static readonly ConfigurationSetting SteelCunetteHeight = new ConfigurationSetting(key: "height", description: "Height of the cunette (m)");
        public static readonly ConfigurationSetting SteelCunetteR = new ConfigurationSetting(key: "r", description: "Radius r (m))");
        public static readonly ConfigurationSetting SteelCunetteR1 = new ConfigurationSetting(key: "r1", description: "Radius r1 (m)");
        public static readonly ConfigurationSetting SteelCunetteR2 = new ConfigurationSetting(key: "r2", description: "Radius r2 (m))");
        public static readonly ConfigurationSetting SteelCunetteR3 = new ConfigurationSetting(key: "r3", description: "Radius r3 (m)");
        public static readonly ConfigurationSetting SteelCunetteA = new ConfigurationSetting(key: "a", description: "Radius a (m)");
        public static readonly ConfigurationSetting SteelCunetteA1 = new ConfigurationSetting(key: "a1", description: "Radius a1 (m)");

        public static readonly ConfigurationSetting Slope = new ConfigurationSetting(key: "slope", description: "Slope of trapezium (m)");
        public static readonly ConfigurationSetting MaximumFlowWidth = new ConfigurationSetting(key: "maximumFlowWidth", description: "Maximum flow width of trapezium (m)");
        public static readonly ConfigurationSetting BottomWidth = new ConfigurationSetting(key: "bottomWidth", description: "Bottom width of trapezium (m)");

        public static readonly ConfigurationSetting IsShared = new ConfigurationSetting(key: "isShared", description: "(Optional, default=false=0) if is shared set to true=1");
        


        
    }
}
