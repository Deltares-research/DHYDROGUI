using System.Collections.Generic;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// Defines the interface used for providing the features of an <see cref="IWaveModel"/>.
    /// </summary>
    public interface IWaveFeatureProvider
    {
        /// <summary>
        /// Gets the observation points.
        /// </summary>
        /// <remarks>
        /// This is guaranteed to never be <c>null</c>.
        /// </remarks>
        IEnumerable<Feature2D> ObservationPoints { get; }
    }
}