using System.ComponentModel;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum SewerConnectionWaterType
    {
        [Description("None")] None, // => Transport riool leiding
        [Description("Storm water")] StormWater, // Hemel water afvoer (HWA) => Hemelwaterriool
        [Description("Dry weather")] DryWater, // Droog water afvoer (DWA) => vuilwaterriool
        [Description("Combined")] Combined // Gemengd riool
    }
}