using System.ComponentModel;

namespace DeltaShell.NGHS.IO.Grid
{
    public enum BranchType
    {
        [Description("Foul Water Flow")]
        DryWeatherFlow = 1,

        [Description("Storm Water Flow")]
        StormWaterFlow = 2,

        [Description("Mixed Flow")]
        MixedFlow = 3,

        [Description("Surface Water")]
        SurfaceWater = 4,

        [Description("Transport Water")]
        TransportWater = 5
    }
}

