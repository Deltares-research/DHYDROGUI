using System;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    [Entity]
    public class VerheijVdKnaap2002BreachSettings : LeveeBreachSettings
    {
        public override LeveeBreachGrowthFormula GrowthFormula { get; } = LeveeBreachGrowthFormula.VerheijvdKnaap2002;

        public double InitialBreachWidth { get; set; }

        public double InitialCrestLevel { get; set; }
        
        public double Factor1Alfa { get; set; }

        public double Factor2Beta { get; set; }

        public double MinimumCrestLevel { get; set; }

        public double CriticalFlowVelocity { get; set; }

        public TimeSpan PeriodToReachZmin { get; set; } = new TimeSpan(1, 0, 0);
    }
}