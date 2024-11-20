using System.ComponentModel;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    public enum LeveeBreachGrowthFormula
    {
        [Description("Verheij - vd Knaap (2002)")]
        VerheijvdKnaap2002 = 2,

        [Description("User defined breach")]
        UserDefinedBreach = 3
    }
}