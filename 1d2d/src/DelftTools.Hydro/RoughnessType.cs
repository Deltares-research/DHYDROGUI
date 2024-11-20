using System.ComponentModel;

namespace DelftTools.Hydro
{
    ///<summary>
    /// Is this a friction type?
    ///</summary>
    public enum RoughnessType
    {
        [Description("Chezy")]
        Chezy = 0,
        [Description("Manning")]
        Manning = 1,
        [Description("Strickler kn")]
        StricklerNikuradse = 2,
        [Description("Strickler ks")]
        Strickler = 3,
        [Description("White & Colebrook")]
        WhiteColebrook = 4,
        [Description("Bos & Bijkerk")]
        DeBosBijkerk = 7,
        [Description("Wall Law Nikuradse")]
        WallLawNikuradse = 8 
    }
}