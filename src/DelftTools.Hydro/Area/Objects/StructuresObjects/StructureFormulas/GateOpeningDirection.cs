using System.ComponentModel;
using DelftTools.Utils;

namespace DelftTools.Hydro.Area.Objects.StructuresObjects.StructureFormulas
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum GateOpeningDirection
    {
        [Description("Symmetric")]
        Symmetric,

        [Description("From left")]
        FromLeft,

        [Description("From right")]
        FromRight
    }
}