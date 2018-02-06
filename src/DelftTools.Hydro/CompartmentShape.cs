using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum CompartmentShape
    {
        [Description("Unknown")] Unknown,
        [Description("RHK")] Rectangular,
        [Description("RND")] Square
    }
}