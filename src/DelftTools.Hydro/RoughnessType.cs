using System.ComponentModel;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Is this a friction type?
    /// </summary>
    public enum RoughnessType
    {
        [Description("Chezy")]
        Chezy = 0,

        [Description("Manning")]
        Manning = 1,

        [Description("Strickler kn")]
        StricklerKn = 2,

        [Description("Strickler ks")]
        StricklerKs = 3,

        [Description("White & Colebrook")]
        WhiteColebrook = 4,

        [Description("Bos & Bijkerk")]
        DeBosAndBijkerk = 7
    }
}