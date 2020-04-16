using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Spectrum import type of the wave boundary
    /// </summary>
    public enum SpectrumImportType
    {
        [Description(KnownWaveBoundariesFileConstants.FromFileSpectrumType)]
        FromFile = 1,

        [Description(KnownWaveBoundariesFileConstants.ParametrizedSpectrumType)]
        Parametrized = 2
    }
}