using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum Structure2DType
    {
        [Description("pump")] Pump,
        [Description("gate")] Gate,
        [Description("weir")] Weir,
        [Description("generalstructure")] GeneralStructure,
        [Description("dambreak")] LeveeBreach,
        InvalidType
    }
}