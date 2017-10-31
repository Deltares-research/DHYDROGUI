using DelftTools.Hydro;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class FeatureGroupExtensions
    {
        public static void TrySetGroupName(this IFeature feature, string filePath)
        {
            var groupableFeature = feature as IGroupableFeature;
            if (groupableFeature == null) return;

            groupableFeature.GroupName = filePath;
        }
    }
}
