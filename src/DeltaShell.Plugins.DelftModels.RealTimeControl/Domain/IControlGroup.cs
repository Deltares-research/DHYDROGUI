using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public interface IControlGroup
    {
        string Name { get; set; }
        IEventedList<RuleBase> Rules { get; set; }
        IEventedList<ConditionBase> Conditions { get; set; }
        IEventedList<Input> Inputs { get; set; }
        IEventedList<Output> Outputs { get; set; }
        IEventedList<SignalBase> Signals { get; set; }
    }
}