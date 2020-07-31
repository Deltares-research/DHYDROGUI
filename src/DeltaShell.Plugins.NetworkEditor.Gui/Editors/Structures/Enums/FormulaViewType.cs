using System.ComponentModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Enums
{
    /// <summary>
    /// <see cref="FormulaViewType"/> defines the possible weir structure views
    /// currently supported within the D-HYDRO application.
    /// </summary>
    public enum FormulaViewType
    {
        [Description("Simple Weir")]
        SimpleWeir = 1,
        [Description("Simple Gate")]
        SimpleGate = 2,
        [Description("General Structure")]
        GeneralStructure = 3,
    }
}