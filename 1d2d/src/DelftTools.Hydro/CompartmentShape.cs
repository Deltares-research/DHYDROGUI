using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum CompartmentShape
    {
        [Description("Unknown")] Unknown,
        [Description("Rectangular")] Rectangular,
        [Description("Round")] Round
    }
}