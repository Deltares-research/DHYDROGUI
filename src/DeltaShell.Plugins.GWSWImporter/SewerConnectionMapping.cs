using System.ComponentModel;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public static class SewerConnectionMapping
    {
        public enum ConnectionType
        {
            [Description("Unknown")] Unknown,
            [Description("DRL")] Orifice,
            [Description("GSL")] ClosedConnection /*Should be created as a pipe*/,
            [Description("ITR")] InfiltrationPipe /*Should be created as a pipe*/,
            [Description("OPL")] Open /*Should be created as a pipe*/,
            [Description("OVS")] Crest,
            [Description("PMP")] Pump
        }

        public enum FlowDirection /*Field STR_RCH*/
        {
            [Description("GSL")] Closed,
            [Description("OPN")] Open,
            [Description("1_2")] FromStartToEnd,
            [Description("2_1")] FromEndToStart,
        }

        public static class PropertyKeys
        {
            public const string UniqueId = "UNIQUE_ID";
            public const string SourceCompartmentId = "NODE_UNIQUE_ID_START";
            public const string TargetCompartmentId = "NODE_UNIQUE_ID_END";
            public const string PipeType = "PIPE_TYPE";
            public const string LevelStart = "LEVEL_START";
            public const string LevelEnd = "LEVEL_END";
            public const string Length = "LENGTH";
            public const string CrossSectionDefinitionId = "CROSS_SECTION_DEF";
            public const string PipeId = "PIPE_INDICATOR";
            public const string WaterType = "WATER_TYPE";
            public const string InletLossStart = "INLETLOSS_START";
            public const string OutletLossStart = "OUTLETLOSS_START";
            public const string InletLossEnd = "INLETLOSS_END";
            public const string OutletLossEnd = "OUTLETLOSS_END";
            public const string FlowDirection = "FLOW_DIRECTION";
            public const string InfiltrationDef = "INFILTRATION_DEF";
            public const string Status = "STATUS";
            public const string ALevelStart = "A_LEVEL_START";
            public const string ALevelEnd = "A_LEVEL_END";
            public const string InitialWaterLevel = "INITIAL_WATER_LEVEL";
            public const string Remarks = "REMARKS";
            public const string SurfaceStorage = "SURFACE_STORAGE";
            public const string InfiltrationCapacityMax = "INFILTRATION_CAPACITY_MAX";
            public const string InfiltrationCapacityMin = "INFILTRATION_CAPACITY_MIN";
            public const string InfiltrationCapacityReduction = "INFILTRATION_CAPACITY_REDUCTION";
            public const string InfiltrationCapacityRecovery = "INFILTRATION_CAPACITY_RECOVERY";
            public const string RunoffDelay = "RUNOFF_DELAY";
            public const string RunoffLength = "RUNOFF_LENGTH";
            public const string RunoffSlope = "RUNOFF_SLOPE";
            public const string TerrainRoughness = "TERRAIN_ROUGHNESS";
            public const string MeteoStationId = "WEATHER_STATION";
            public const string SurfaceId = "SURFACE_ID";
            public const string Surface = "SURFACE";
            public const string DistributionId = "DISTRIBUTION_ID";
            public const string DistributionType = "DISTRIBUTION_TYPE";
            public const string DayNumber = "DAY";
            public const string DailyVolume = "VOLUME";
            public const string HourlyPercentage00 = "H00_PERCENTAGE_DAY_VOLUME_AT_00_HOUR";
            public const string HourlyPercentage01 = "H01_PERCENTAGE_DAY_VOLUME_AT_01_HOUR";
            public const string HourlyPercentage02 = "H02_PERCENTAGE_DAY_VOLUME_AT_02_HOUR";
            public const string HourlyPercentage03 = "H03_PERCENTAGE_DAY_VOLUME_AT_03_HOUR";
            public const string HourlyPercentage04 = "H04_PERCENTAGE_DAY_VOLUME_AT_04_HOUR";
            public const string HourlyPercentage05 = "H05_PERCENTAGE_DAY_VOLUME_AT_05_HOUR";
            public const string HourlyPercentage06 = "H06_PERCENTAGE_DAY_VOLUME_AT_06_HOUR";
            public const string HourlyPercentage07 = "H07_PERCENTAGE_DAY_VOLUME_AT_07_HOUR";
            public const string HourlyPercentage08 = "H08_PERCENTAGE_DAY_VOLUME_AT_08_HOUR";
            public const string HourlyPercentage09 = "H09_PERCENTAGE_DAY_VOLUME_AT_09_HOUR";
            public const string HourlyPercentage10 = "H10_PERCENTAGE_DAY_VOLUME_AT_10_HOUR";
            public const string HourlyPercentage11 = "H11_PERCENTAGE_DAY_VOLUME_AT_11_HOUR";
            public const string HourlyPercentage12 = "H12_PERCENTAGE_DAY_VOLUME_AT_12_HOUR";
            public const string HourlyPercentage13 = "H13_PERCENTAGE_DAY_VOLUME_AT_13_HOUR";
            public const string HourlyPercentage14 = "H14_PERCENTAGE_DAY_VOLUME_AT_14_HOUR";
            public const string HourlyPercentage15 = "H15_PERCENTAGE_DAY_VOLUME_AT_15_HOUR";
            public const string HourlyPercentage16 = "H16_PERCENTAGE_DAY_VOLUME_AT_16_HOUR";
            public const string HourlyPercentage17 = "H17_PERCENTAGE_DAY_VOLUME_AT_17_HOUR";
            public const string HourlyPercentage18 = "H18_PERCENTAGE_DAY_VOLUME_AT_18_HOUR";
            public const string HourlyPercentage19 = "H19_PERCENTAGE_DAY_VOLUME_AT_19_HOUR";
            public const string HourlyPercentage20 = "H20_PERCENTAGE_DAY_VOLUME_AT_20_HOUR";
            public const string HourlyPercentage21 = "H21_PERCENTAGE_DAY_VOLUME_AT_21_HOUR";
            public const string HourlyPercentage22 = "H22_PERCENTAGE_DAY_VOLUME_AT_22_HOUR";
            public const string HourlyPercentage23 = "H23_PERCENTAGE_DAY_VOLUME_AT_23_HOUR";
            public const string DischargeId = "DISCHARGE_ID";
            public const string DischargeType = "DISCHARGE_TYPE";
            public const string PollutingUnits = "POLLUTING_UNITS";
            public const string RunoffDefinitionFile = "WWF_ID";
        }
    }
}