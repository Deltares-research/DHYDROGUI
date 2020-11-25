using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// Implements the interface used for providing the features of an <see cref="IWaveFeatureContainer"/>.
    /// </summary>
    public class WaveFeatureProvider : IWaveFeatureProvider
    {
        private readonly IWaveFeatureContainer featureContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveFeatureProvider"/> class.
        /// </summary>
        /// <param name="featureContainer">The feature container from which to obtain the features.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="featureContainer"/> is <c>null</c>.
        /// </exception>
        public WaveFeatureProvider(IWaveFeatureContainer featureContainer)
        {
            Ensure.NotNull(featureContainer, nameof(featureContainer));

            this.featureContainer = featureContainer;
        }

        public IEnumerable<Feature2D> ObservationPoints => featureContainer.ObservationPoints ?? Enumerable.Empty<Feature2D>();
    }
}