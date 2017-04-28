using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain
{
    [TypeConverter(typeof (EnumDescriptionAttributeTypeConverter))]
    public enum PolderSubTypes
    {
        None,
        [Description("Unpaved: Grass")] Grass,
        [Description("Unpaved: Corn")] Corn,
        [Description("Unpaved: Potatoes")] Potatoes,
        [Description("Unpaved: Sugarbeet")] Sugarbeet,
        [Description("Unpaved: Grain")] Grain,
        [Description("Unpaved: Miscellaneous")] Miscellaneous,
        [Description("Unpaved: Non-arable land")] NonArableLand,
        [Description("Unpaved: Greenhouse Area")] GreenhouseArea,
        [Description("Unpaved: Orchard")] Orchard,
        [Description("Unpaved: Bulbous Plants")] BulbousPlants,
        [Description("Unpaved: Foliage Forest")] FoliageForest,
        [Description("Unpaved: Pine Forest")] PineForest,
        [Description("Unpaved: Nature")] Nature,
        [Description("Unpaved: Fallow")] Fallow,
        [Description("Unpaved: Vegetables")] Vegetables,
        [Description("Unpaved: Flowers")] Flowers,
        Paved,
        [Description("Greenhouse: < 500 m³/ha")] lessThan500,
        [Description("Greenhouse: 500-1000 m³/ha")] from500to1000,
        [Description("Greenhouse: 1000-1500 m³/ha")] from1000to1500,
        [Description("Greenhouse: 1500-2000 m³/ha")] from1500to2000,
        [Description("Greenhouse: 2000-2500 m³/ha")] from2000to2500,
        [Description("Greenhouse: 2500-3000 m³/ha")] from2500to3000,
        [Description("Greenhouse: 3000-4000 m³/ha")] from3000to4000,
        [Description("Greenhouse: 4000-5000 m³/ha")] from4000to5000,
        [Description("Greenhouse: 5000-6000 m³/ha")] from5000to6000,
        [Description("Greenhouse: > 6000 m³/ha")] moreThan6000,
        [Description("Open water")] OpenWater
    }
}