using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Shape import type of the wave boundary
    /// </summary>
    public enum ShapeImportType
    {
        [Description(KnownWaveBoundariesFileConstants.GaussShape)]
        Gauss,

        [Description(KnownWaveBoundariesFileConstants.JonswapShape)]
        Jonswap,

        [Description(KnownWaveBoundariesFileConstants.PiersonMoskowitzShape)]
        PiersonMoskowitz
    }
}