using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// <see cref="IWaveFeatureContainer"/> provides the interface to manage a set of D-Waves features.
    /// </summary>
    public interface IWaveFeatureContainer
    {
        /// <summary>
        /// Gets the observation points.
        /// </summary>
        IEventedList<Feature2DPoint> ObservationPoints { get; }

        /// <summary>
        /// Gets the observation cross sections.
        /// </summary>
        IEventedList<Feature2D> ObservationCrossSections { get; }

        /// <summary>
        /// Gets the obstacles.
        /// </summary>
        IEventedList<WaveObstacle> Obstacles { get; }

        /// <summary>
        /// Gets all the features this <see cref="IWaveFeatureContainer"/> contains.
        /// </summary>
        IEnumerable<IFeature> GetAllFeatures();
    }
}