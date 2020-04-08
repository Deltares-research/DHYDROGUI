using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Shape import type of the wave boundary
    /// </summary>
    public enum ShapeImportType
    {
        [Description("gauss")]
        Gauss,

        [Description("jonswap")]
        Jonswap,

        [Description("pierson-moskowitz")]
        PiersonMoskowitz
    }
}