using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.Structures
{
    public enum LeveeBreachGrowthFormula
    {
        [Description("vd Knaap (2000)")]
        VdKnaap2000,
        [Description("Verheij - vd Knaap (2002)")]
        VerweijvdKnaap2002,
    }

    public enum LeveeMaterial
    {
        Sand,
        Clay
    }

    public abstract class LeveeBreachSettings
    {
        public abstract LeveeBreachGrowthFormula GrowthFormula { get; }

        public double InitialBreachWidth { get; set; }

        public double InitialCrestLevel { get; set; }

        public DateTime StartTimeBreachGrowth { get; set; } = new DateTime(2000, 1, 1, 1, 0, 0);
    }

    [Entity]
    public class LeveeBreachSettingsVdKnaap2000 : LeveeBreachSettings
    {
        public override LeveeBreachGrowthFormula GrowthFormula { get; } = LeveeBreachGrowthFormula.VdKnaap2000;

        public double MaxBreachDepth { get; set; }

        public TimeSpan PeriodToReachMaxBreachDepth { get; set; } = new TimeSpan(1, 0, 0, 0);

        public TimeSpan PeriodToReachMaxWidth { get; set; } = new TimeSpan(1, 0, 0, 0);

        public bool UsePeriodToReachMaxWidth { get; set; }

        public bool UseMaxBreachingWidth { get; set; }

        public double MaxBreachWidth { get; set; }

        public LeveeMaterial LeveeMaterial { get; set; } = LeveeMaterial.Sand;

        public ObservableCollection<SomeSettingClass> BreachGrowthSettings { get; set; } = new ObservableCollection<SomeSettingClass>();
    }

    
    public class SomeSettingClass // TODO Give an appropriate name :) 
    {
        public TimeSpan TimeSpan { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double AreaMearea
        {
            get { return Width * Height; }
            set { }
        }
    }

    [Entity]
    public class LeveeBreachSettingsVerheijVdKnaap2002 : LeveeBreachSettings
    {
        public override LeveeBreachGrowthFormula GrowthFormula { get; } = LeveeBreachGrowthFormula.VerweijvdKnaap2002;

        public double Factor1Alfa { get; set; }

        public double Factor2Beta { get; set; }

        public double LowestCrestLevel { get; set; }

        public double CriticalFlowVelocity { get; set; }

        public TimeSpan PeriodToReachZmin { get; set; } = new TimeSpan(1, 0, 0, 0);
    }
}