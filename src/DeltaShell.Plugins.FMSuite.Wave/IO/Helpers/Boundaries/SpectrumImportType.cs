using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Spectrum import type of the wave boundary
    /// </summary>
    public enum SpectrumImportType
    {
        [Description("from file")]
        FromFile,

        [Description("parametric")]
        Parametrized
    }
}