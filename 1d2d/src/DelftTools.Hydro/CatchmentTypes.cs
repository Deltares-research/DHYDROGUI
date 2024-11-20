using System.ComponentModel;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Catchment Types enum based on CatchmentType
    /// </summary>
    public enum CatchmentTypes
    {
        [Description("None")]
        None,
        [Description("Greenhouse")]
        Greenhouse,
        [Description("OpenWater")]
        OpenWater,
        [Description("Paved")]
        Paved,
        [Description("Unpaved")]
        Unpaved,
        [Description("Sacramento")]
        Sacramento,
        [Description("Hbv")]
        Hbv,
        [Description("NWRW")]
        NWRW,
    }
}