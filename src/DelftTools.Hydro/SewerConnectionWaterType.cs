using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum SewerConnectionWaterType
    {
        [Description("NVT")] None, // => Transport riool leiding
        [Description("HWA")] StormWater, // Hemel water afvoer (HWA) => Hemelwaterriool
        [Description("DWA")] DryWater, // Droog water afvoer (DWA) => vuilwaterriool
        [Description("Combined")] Combined // Gemengd riool
    }
}