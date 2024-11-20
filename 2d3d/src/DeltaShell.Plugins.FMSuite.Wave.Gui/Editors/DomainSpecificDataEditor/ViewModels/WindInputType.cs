using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// The wind input type
    /// </summary>
    /// <remarks>Specifically used for the UI.</remarks>
    public enum WindInputType
    {
        [Description("XY components")]
        XYComponents,

        [Description("Wind vector")]
        WindVector,

        [Description("Spiderweb grid")]
        SpiderWebGrid
    }
}