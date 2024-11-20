using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum TimeDependency
    {
        [Description("Constant")]
        Constant,
        [Description("Time dependent")]
        TimeDependent
    }
}