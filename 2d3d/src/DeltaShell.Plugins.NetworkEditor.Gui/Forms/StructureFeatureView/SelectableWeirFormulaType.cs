using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum SelectableWeirFormulaType
    {
        [Description("Simple weir")]
        SimpleWeir,

        [Description("Simple gate")]
        SimpleGate,

        [Description("General structure")]
        GeneralStructure
    }
}