using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features
{
    /// <summary>
    /// <see cref="BoundaryLineFeature"/> describes the line of a <see cref="IWaveBoundary"/>
    /// as a <see cref="Feature2D"/>.
    /// </summary>
    public class BoundaryLineFeature : Feature2D
    {
        /// <summary>
        /// Gets or sets the observed <see cref="IWaveBoundary"/>.
        /// </summary>
        public IWaveBoundary ObservedWaveBoundary { get; set; }
    }
}