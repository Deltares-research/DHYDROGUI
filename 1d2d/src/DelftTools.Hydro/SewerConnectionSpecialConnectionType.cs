using System.ComponentModel;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum SewerConnectionSpecialConnectionType
    {
        [Description("None")] None,
        [Description("Pressurized pipe")] Pump,
        [Description("Weir / Orifice ")] Weir
    }
}