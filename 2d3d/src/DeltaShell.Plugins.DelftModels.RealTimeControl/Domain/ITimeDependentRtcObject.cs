using DelftTools.Functions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    public interface ITimeDependentRtcObject
    {
        TimeSeries TimeSeries { get; set; }
    }
}