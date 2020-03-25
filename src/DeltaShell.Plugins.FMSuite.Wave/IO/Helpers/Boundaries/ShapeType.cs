using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Shape type of the wave boundary
    /// </summary>
    public enum ShapeType
    {
        [Description("gauss")]
        Gauss,

        [Description("jonswap")]
        Jonswap,

        [Description("pierson-moskowitz")]
        PiersonMoskowitz
    }
}