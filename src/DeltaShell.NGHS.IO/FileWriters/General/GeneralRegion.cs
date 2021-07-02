using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.General
{
    public static class GeneralRegion
    {
        /* section general */
        public const string Header = "[General]";
        public const string IniHeader = "General";
        public const int CrossSectionDefinitionsMajorVersion = 3;
        public const int CrossSectionDefinitionsMinorVersion = 0;

        public const int CrossSectionLocationsMajorVersion = 1;
        public const int CrossSectionLocationsMinorVersion = 1;
        
        public const int StructureDefinitionsMajorVersion = 3;
        public const int StructureDefinitionsMinorVersion = 0;

        public const int ModelDefinitionsMajorVersion = 1;
        public const int ModelDefinitionsMinorVersion = 0;

        public const int NetworkDefinitionsMajorVersion = 1;
        public const int NetworkDefinitionsMinorVersion = 0;
        
        public const int SpatialDataMajorVersion = 1;
        public const int SpatialDataMinorVersion = 0;

        public const int ObservationPointLocationsMajorVersion = 2;
        public const int ObservationPointLocationsMinorVersion = 0;

        public const int BoundaryLocationsMajorVersion = 1;
        public const int BoundaryLocationsMinorVersion = 0;

        public const int LateralDischargeLocationsMajorVersion = 1;
        public const int LateralDischargeLocationsMinorVersion = 0;

        public const int BoundaryConditionsMajorVersion = 1;
        public const int BoundaryConditionsMinorVersion = 1;

        public const int BoundaryConditionsExternalForcingMajorVersion = 2;
        public const int BoundaryConditionsExternalForcingMinorVersion = 0;

        public const int RoughnessDataMajorVersion = 3;
        public const int RoughnessDataMinorVersion = 0;

        public const int RetentionMajorVersion = 2;
        public const int RetentionMinorVersion = 0;

        public const int Iterative1D2DCouplerMajorVersion = 1;
        public const int Iterative1D2DCouplerMinorVersion = 0;

        public const int Iterative1D2DCouplerMappingMajorVersion = 1;
        public const int Iterative1D2DCouplerMappingMinorVersion = 0;

        public const int InitialConditionDataMajorVersion = 2;
        public const int InitialConditionDataMinorVersion = 0;


        public static class FileTypeName
        {
            public const string Iterative1D2DCouplerMapping = "1D2Dmapping";
            public const string Iterative1D2DCoupler = "1D2D";
            public const string CrossSectionDefinition = "crossDef";
            public const string CrossSectionLocation = "crossLoc";
            public const string StructureDefinition = "structure";
            public const string ModelDefinition = "modelDef";
            public const string NetworkDefinition = "network";
            public const string ObservationPoint = "obsPoints";
            public const string ObservationCross = "obsCross";
            public const string SpatialData = "spatialData";
            public const string BoundaryLocation = "boundLocs";
            public const string LateralDischargeLocation = "latLocs";
            public const string BoundaryConditions = "boundConds";
            public const string BoundaryConditionExternalForcing = "extForce";
            public const string RoughnessData = "roughness";
            public const string Retention = "retentions";
            public const string StorageNodes = "storageNodes";
            public const string InitialConditionQuantity = "1dField";
            public const string InitialFields = "iniField";
        }

        public static readonly ConfigurationSetting FileVersion = new ConfigurationSetting(key:"fileVersion", description: "#File version. Do not edit this.");
        public static readonly ConfigurationSetting FileMajorVersion = new ConfigurationSetting(key:"majorVersion", description: "#Major file version. Do not edit this.");
        public static readonly ConfigurationSetting FileMinorVersion = new ConfigurationSetting(key: "minorVersion", description: "#Minor file version. Do not edit this.");
        public static readonly ConfigurationSetting FileType = new ConfigurationSetting(key: "fileType", description: "#File type. Do not edit this.");
    }
}