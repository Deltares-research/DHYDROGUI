using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Spreading import type of the wave boundary
    /// </summary>
    public enum SpreadingImportType
    {
        [Description("degrees")]
        Degrees,

        [Description("power")]
        Power
    }
}