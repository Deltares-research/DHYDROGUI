using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
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
