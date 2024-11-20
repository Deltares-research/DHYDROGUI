using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// The directional space types.
    /// </summary>
    /// <remarks>Specifically used for the UI.</remarks>
    public enum DirectionalSpaceType
    {
        [Description("Circle")]
        Circle,

        [Description("Sector")]
        Sector
    }
}