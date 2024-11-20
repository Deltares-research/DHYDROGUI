using System;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    [Entity]
    public class VerheijVdKnaap2002BreachSettings : LeveeBreachSettings
    {
        public override LeveeBreachGrowthFormula GrowthFormula { get; } = LeveeBreachGrowthFormula.VerheijvdKnaap2002;

        public double InitialBreachWidth { get; set; } = 10.0; // in m

        public double InitialCrestLevel { get; set; } // in m AD

        public double Factor1Alfa { get; set; } = 1.3;

        public double Factor2Beta { get; set; } = 0.04;

        public double MinimumCrestLevel { get; set; } // in m AD

        public double CriticalFlowVelocity { get; set; } = 0.2; // in m/s

        public TimeSpan PeriodToReachZmin { get; set; } = new TimeSpan(1, 0, 0);
    }
}