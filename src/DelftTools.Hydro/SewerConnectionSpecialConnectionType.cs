using System.ComponentModel;

namespace DelftTools.Hydro
{

    public enum SewerConnectionSpecialConnectionType
    {
        [Description("None")] None,
        [Description("Pressurized pipe")] Pump,
        [Description("Weir")] Weir
    }
}