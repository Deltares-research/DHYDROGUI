using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Interpolation for hydraulic types.
    /// Note that since the items are defined in terms of InterpolationType, it is
    /// valid to cast from/to InterpolationType
    /// </summary>
    public enum InterpolationHydraulicType
    {
        Constant = InterpolationType.Constant,
        Linear = InterpolationType.Linear
    }
}