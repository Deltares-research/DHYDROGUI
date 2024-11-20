using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// The usage types describing whether and how to use the parameter from flow data.
    /// </summary>
    /// <remarks>Specifically used for the UI.</remarks>
    public enum HydroDynamicsUseParameterType
    {
        [Description("Do not use")]
        DoNotUse,

        [Description("Use")]
        Use,

        [Description("Use extend")]
        UseExtend
    }
}