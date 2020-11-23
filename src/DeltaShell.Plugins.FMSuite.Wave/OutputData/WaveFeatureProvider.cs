using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// Implements the interface used for providing an <see cref="IFeature"/> collection for D-Waves.
    /// </summary>
    public class WaveFeatureProvider : IWaveFeatureProvider
    {
        private readonly IWaveModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveFeatureProvider"/> class.
        /// </summary>
        /// <param name="model">The wave model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public WaveFeatureProvider(IWaveModel model)
        {
            Ensure.NotNull(model, nameof(model));

            this.model = model;
        }

        public IEnumerable<IFeature> Features => model.ObservationPoints ?? Enumerable.Empty<IFeature>();
    }
}