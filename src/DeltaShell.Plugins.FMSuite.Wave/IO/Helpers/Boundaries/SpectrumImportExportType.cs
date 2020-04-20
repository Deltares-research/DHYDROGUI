using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Spectrum import and export type of the wave boundary.
    /// </summary>
    public enum SpectrumImportExportType
    {
        [Description(KnownWaveBoundariesFileConstants.FromFileSpectrumType)]
        FromFile,

        [Description(KnownWaveBoundariesFileConstants.ParametrizedSpectrumType)]
        Parametrized
    }
}