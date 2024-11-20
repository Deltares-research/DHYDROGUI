using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas
{
    public enum ObservationPointType
    {
        [Description("Single point")]
        SinglePoint = 0,

        [Description("Average of all layers")]
        Average = 1,

        [Description("Point at each layer")]
        OneOnEachLayer = 2
    }
}