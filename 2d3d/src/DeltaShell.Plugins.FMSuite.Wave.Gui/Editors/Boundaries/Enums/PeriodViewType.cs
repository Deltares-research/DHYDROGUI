using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums
{
    /// <summary>
    /// <see cref="PeriodViewType"/> defines the possible options of
    /// wave period types within the View layer.
    /// </summary>
    public enum PeriodViewType
    {
        [Description("Peak")]
        Peak = 1,

        [Description("Mean")]
        Mean = 2
    }
}