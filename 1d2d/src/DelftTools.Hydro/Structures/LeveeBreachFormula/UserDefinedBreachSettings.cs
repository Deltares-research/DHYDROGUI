using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro.Structures.LeveeBreachFormula
{
    [Entity]
    public class UserDefinedBreachSettings : LeveeBreachSettings
    {
        public override LeveeBreachGrowthFormula GrowthFormula { get; } = LeveeBreachGrowthFormula.UserDefinedBreach;

        public EventedList<BreachGrowthSetting> ManualBreachGrowthSettings { get; set; } = new EventedList<BreachGrowthSetting>();
        
    }
}