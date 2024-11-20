using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Spreading import type of the wave boundary
    /// </summary>
    public enum SpreadingImportType
    {
        [Description(KnownWaveBoundariesFileConstants.DegreesDefinedSpreading)]
        Degrees,

        [Description(KnownWaveBoundariesFileConstants.PowerDefinedSpreading)]
        Power
    }
}