using System.ComponentModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public enum SelectableWeirFormulaType
    {
        [Description("Simple weir")]
        SimpleWeir,
        [Description("Simple gate")]
        SimpleGate,
        [Description("General structure")]
        GeneralStructure,    
    }
}