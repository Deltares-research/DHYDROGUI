using System.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// Defines the interface used for providing an <see cref="IFeature"/> collection for D-Waves.
    /// </summary>
    public interface IWaveFeatureProvider
    {
        /// <summary>
        /// Gets the features.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to never be <c>null</c>.
        /// </remarks>
        IEnumerable<IFeature> Features { get; }
    }
}