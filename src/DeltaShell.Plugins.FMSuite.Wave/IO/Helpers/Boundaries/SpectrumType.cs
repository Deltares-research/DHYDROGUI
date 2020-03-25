using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Spectrum type of the wave boundary
    /// </summary>
    public enum SpectrumType
    {
        [Description("from file")]
        FromFile,

        [Description("parametric")]
        Parametrized
    }
}