using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum Operator
    {
        Overwrite,
        [Description("Overwr. missing")]
        ApplyOnly,
        Add,
        Multiply,
        Maximum,
        Minimum
    }
}