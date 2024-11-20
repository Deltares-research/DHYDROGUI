using DelftTools.Hydro.GroupableFeatures;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public static class FeatureGroupExtensions
    {
        public static void TrySetGroupName(this IFeature feature, string filePath)
        {
            if (!(feature is IGroupableFeature groupableFeature))
            {
                return;
            }

            groupableFeature.GroupName = filePath;
        }

        public static bool HasDefaultGroupName(this IGroupableFeature feature, string featureExtension,
                                               string defaultGroupName)
        {
            string featureGroupName = feature.GroupName;
            return string.IsNullOrEmpty(featureGroupName) ||
                   featureGroupName.Replace(featureExtension, string.Empty).Equals(defaultGroupName);
        }
    }
}