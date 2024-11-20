using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums
{
    /// <summary>
    /// The Forcing types as defined in the View.
    /// </summary>
    public enum ForcingViewType
    {
        [ResourcesDescription(typeof(Resources), nameof(Resources.ForcingViewType_Constant_Description))]
        Constant = 1,

        [ResourcesDescription(typeof(Resources), nameof(Resources.ForcingViewType_TimeSeries_Description))]
        TimeSeries = 2,

        [ResourcesDescription(typeof(Resources), nameof(Resources.ForcingViewType_FileBased_Description))]
        FileBased = 3
    }
}