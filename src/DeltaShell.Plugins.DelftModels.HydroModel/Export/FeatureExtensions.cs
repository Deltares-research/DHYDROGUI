using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Export
{
    static internal class FeatureExtensions
    {
        public static string GetFeatureCategory(this IFeature feature)
        {
            return "test";
        }
    }
}