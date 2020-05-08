using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums
{
    /// <summary>
    /// The Forcing types as defined in the View.
    /// </summary>
    public enum ForcingViewType
    {
        [Description("Parametrized (Constant)")]
        Constant = 1,

        [Description("Parametrized (Time Series)")]
        TimeSeries = 2,

        [Description("Filebased Spectrum")]
        FileBased = 3,
    }
}