using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// Implements the interface used for providing the features of an <see cref="IWaveModel"/>.
    /// </summary>
    public class WaveFeatureProvider : IWaveFeatureProvider
    {
        private readonly IWaveModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveFeatureProvider"/> class.
        /// </summary>
        /// <param name="model">The wave model from which to obtain the features.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public WaveFeatureProvider(IWaveModel model)
        {
            Ensure.NotNull(model, nameof(model));

            this.model = model;
        }

        public IEnumerable<Feature2D> ObservationPoints => model.ObservationPoints ?? Enumerable.Empty<Feature2D>();
    }
}