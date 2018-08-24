using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM
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
        }
    }
}