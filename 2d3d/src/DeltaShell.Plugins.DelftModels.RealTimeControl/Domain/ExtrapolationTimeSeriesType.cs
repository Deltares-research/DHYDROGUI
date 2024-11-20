using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Extrapolation for timeseries types.
    /// Note that since the items are defined in terms of ExtrapolationType, it is
    /// valid to cast from/to ExtrapolationType
    /// </summary>
    public enum ExtrapolationTimeSeriesType
    {
        Constant = ExtrapolationType.Constant,
        Periodic = ExtrapolationType.Periodic
    }
}