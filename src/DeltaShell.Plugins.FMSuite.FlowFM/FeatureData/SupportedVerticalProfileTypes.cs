using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    // TODO: Move to flow plugin or remove entirely, once the flow kernel supports all vertical profiles.

    public static class SupportedVerticalProfileTypes
    {
        public static IEnumerable<VerticalProfileType> BoundaryConditionProfileTypes
        {
            get
            {
                yield return VerticalProfileType.Uniform;
                yield return VerticalProfileType.PercentageFromBed;
            }
        }

        public static IEnumerable<VerticalProfileType> InitialConditionProfileTypes
        {
            get
            {
                yield return VerticalProfileType.Uniform;
                yield return VerticalProfileType.TopBottom;
            }
        }
    }
}