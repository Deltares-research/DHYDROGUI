using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums
{
    /// <summary>
    /// <see cref="DirectionalSpreadingViewType"/> defines the possible
    /// options of directional spreading types within the View layer.
    /// </summary>
    public enum DirectionalSpreadingViewType
    {
        [Description("Power")]
        Power = 1,

        [Description("Degrees")]
        Degrees = 2
    }
}