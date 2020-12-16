using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// The <see cref="WaveFeatureContainer"/> is responsible for managing a set of D-Waves features.
    /// </summary>
    public class WaveFeatureContainer : IWaveFeatureContainer
    {
        public IEventedList<Feature2DPoint> ObservationPoints { get; } = new EventedList<Feature2DPoint>();

        public IEventedList<Feature2D> ObservationCrossSections { get; } = new EventedList<Feature2D>();

        public IEventedList<WaveObstacle> Obstacles { get; } = new EventedList<WaveObstacle>();

        public IEnumerable<IFeature> GetAllFeatures()
        {
            return ObservationPoints.Concat(ObservationCrossSections).Concat(Obstacles);
        }
    }
}