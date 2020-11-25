using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// The <see cref="WaveFeatureContainer"/> is responsible for managing a set of D-Waves features.
    /// </summary>
    public class WaveFeatureContainer : IWaveFeatureContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WaveFeatureContainer"/> class.
        /// </summary>
        public WaveFeatureContainer()
        {
            ObservationPoints = new EventedList<Feature2DPoint>();
            ObservationCrossSections = new EventedList<Feature2D>();
            Obstacles = new EventedList<WaveObstacle>();
        }

        public IEventedList<Feature2DPoint> ObservationPoints { get; set; }

        public IEventedList<Feature2D> ObservationCrossSections { get; set; }

        public IEventedList<WaveObstacle> Obstacles { get; }
    }
}