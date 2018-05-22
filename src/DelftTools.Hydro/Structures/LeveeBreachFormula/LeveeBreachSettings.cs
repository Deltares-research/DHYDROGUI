using System;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    [Entity]
    public abstract class LeveeBreachSettings
    {
        public abstract LeveeBreachGrowthFormula GrowthFormula { get; }

        public DateTime StartTimeBreachGrowth { get; set; } = new DateTime(2000, 1, 1, 1, 0, 0);
    }
}