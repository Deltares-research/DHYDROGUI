using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class StructureMapping
    {
        public enum StructureType /*Field KWK_TYP*/
        {
            [Description("DRL")] Orifice,
            [Description("OVS")] Crest /*Should be created as a pipe*/,
            [Description("UIT")] Outlet,
            [Description("PMP")] Pump
        }

        public static class PropertyKeys
        {
            public const string UniqueId = "UNIQUE_ID";
            public const string SurfaceWaterLevel = "SURFACE_WATER_LEVEL";
            public const string StructureType = "STRUCTURE_TYPE";
            public const string PumpCapacity = "PUMP_CAPACITY";
            public const string StartLevelDownstreams = "START_LEVEL_DOWNSTREAMS";
            public const string StopLevelDownstreams = "STOP_LEVEL_DOWNSTREAMS";
            public const string StartLevelUpstreams = "START_LEVEL_UPSTREAMS";
            public const string StopLevelUpstreams = "STOP_LEVEL_UPSTREAMS";
        }
    }
}