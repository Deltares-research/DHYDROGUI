using System.ComponentModel;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    public enum LeveeBreachGrowthFormula
    {
        [Description("Verheij - vd Knaap (2002)")]
        VerweijvdKnaap2002,

        [Description("User defined breach")]
        UserDefinedBreach,
    }
}