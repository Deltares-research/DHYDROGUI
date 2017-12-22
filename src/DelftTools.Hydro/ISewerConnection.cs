using System.ComponentModel;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface ISewerConnection : IBranch
    {
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
        [Description("HWA")] StormWater,
        [Description("DWA")] DWF,
        [Description("GMD")] Combined
    }
}