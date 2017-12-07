using System.ComponentModel;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface ISewerConnection : IBranch
    {
        string ConnectionId { get; set; }
        double Length { get; set; }
        double LevelSource { get; set; }
        double LevelTarget { get; set; }
        SewerConnectionWaterType WaterType { get; set; }
        Compartment SourceCompartment { get; set; }
        Compartment TargetCompartment { get; set; }
    }

    public enum SewerConnectionWaterType
    {
        [Description("NVT")] None,
        [Description("HWA")] FlowingRainWater,
        [Description("DWA")] DryWeatherRainage,
        [Description("GMD")] MixedWasteWater,
    }
}