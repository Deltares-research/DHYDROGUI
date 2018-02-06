using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum SewerConnectionWaterType
    {
        [Description("NVT")] None,
        [Description("HWA")] StormWater,
        [Description("DWA")] DWF,
        [Description("GMD")] Combined
    }
}