using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Extrapolation for hydraulic types.
    /// Note that since the items are defined in terms of ExtrapolationType, it is
    /// valid to cast from/to ExtrapolationType
    /// </summary>
    public enum ExtrapolationHydraulicType
    {
        Constant = ExtrapolationType.Constant,
        Linear = ExtrapolationType.Linear
    }
}