using DelftTools.Hydro;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public static class RainfallRunoffValidationHelper
    {
        public static bool IsConsideredAsBoundary(IHydroObject target)
        {
            return target is LateralSource ||
                   target is HydroNode ||
                   target is RunoffBoundary;
        }
    }
}