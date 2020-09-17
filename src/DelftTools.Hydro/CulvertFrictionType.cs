using System.ComponentModel;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Friction types to be used in culvert
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum CulvertFrictionType
    {
        Chezy = 0,
        Manning = 1,

        [Description("Strickler Kn")]
        StricklerKn = 2,

        [Description("Strickler Ks")]
        StricklerKs = 3,

        [Description("White colebrook")]
        WhiteColebrook = 4
    }
}