using System.ComponentModel;

namespace DelftTools.Hydro.Helpers
{
    public enum LeveeBreachPointLocationType
    {
        [Description("Breach location")]
        BreachLocation,
        [Description("Waterlevelstream up location")]
        WaterLevelUpstreamLocation,
        [Description("Waterlevelstream down location")]
        WaterLevelDownstreamLocation
    }
}