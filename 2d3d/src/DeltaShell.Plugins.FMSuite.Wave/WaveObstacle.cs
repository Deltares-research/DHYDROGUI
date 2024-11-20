using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    [Entity]
    public class WaveObstacle : Feature2D
    {
        [FeatureAttribute]
        public ObstacleType Type { get; set; }

        [FeatureAttribute]
        public double TransmissionCoefficient { get; set; }

        [FeatureAttribute]
        public double Height { get; set; }

        [FeatureAttribute]
        public double Alpha { get; set; }

        [FeatureAttribute]
        public double Beta { get; set; }

        [FeatureAttribute]
        public ReflectionType ReflectionType { get; set; }

        [FeatureAttribute]
        public double ReflectionCoefficient { get; set; }
    }

    public enum ReflectionType
    {
        No,
        Specular,
        Diffuse
    }

    public enum ObstacleType
    {
        Sheet,
        Dam
    }
}