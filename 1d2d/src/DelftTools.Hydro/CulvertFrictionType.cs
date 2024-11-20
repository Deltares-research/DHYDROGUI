using System.ComponentModel;
using DelftTools.Utils;

namespace DelftTools.Hydro
{
    /// <summary>
    /// shapetypes of supported culvert cross section
    /// all types are send as Tabulated to modelapi
    /// </summary>
    public enum CulvertGeometryType
    {
        Tabulated,
        Round, // not yet in modelapi
        Egg, // not yet in model api
        InvertedEgg, // not yet in model api
        Rectangle,
        Ellipse, 
        Arch,
        Cunette,
        SteelCunette,
        UShape
    }

    ///<summary>
    /// Friction types to be used in culvert
    ///</summary>
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
        WhiteColebrook = 4,
    }

    public enum CulvertType
    {
        Culvert = 1,
        InvertedSiphon = 3
    }
}