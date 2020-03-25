using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Spreading type of the wave boundary
    /// </summary>
    public enum SpreadingType
    {
        [Description("degrees")]
        Degrees,

        [Description("power")]
        Power
    }
}