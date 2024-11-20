using System.ComponentModel;

namespace DelftTools.Hydro.Structures
{
    ///<summary>
    /// Friction types to be used in bridge
    ///</summary>
    public enum BridgeFrictionType
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
}