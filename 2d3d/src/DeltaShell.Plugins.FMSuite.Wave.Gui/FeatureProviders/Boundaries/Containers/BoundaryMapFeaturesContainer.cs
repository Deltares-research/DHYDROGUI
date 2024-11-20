using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers
{
    /// <summary>
    /// <see cref="BoundaryMapFeaturesContainer"/> acts as a convenience to
    /// group the different BoundaryMapFeatureProviders that are part of
    /// the <see cref="Wave.Boundaries.IWaveBoundary"/> visualisation.
    /// </summary>
    public class BoundaryMapFeaturesContainer : IBoundaryMapFeaturesContainer
    {
        /// <summary>
        /// Creates a new <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <param name="boundaryLineMapFeatureProvider">The boundary line map feature provider.</param>
        /// <param name="boundaryStartPointMapFeatureProvider">The boundary start point map feature provider.</param>
        /// <param name="boundaryEndPointMapFeatureProvider">The boundary end point map feature provider.</param>
        /// <param name="supportPointMapFeatureProvider">The support point map feature provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public BoundaryMapFeaturesContainer(BoundaryLineMapFeatureProvider boundaryLineMapFeatureProvider,
                                            BoundaryReadOnlyMapFeatureProvider boundaryStartPointMapFeatureProvider,
                                            BoundaryReadOnlyMapFeatureProvider boundaryEndPointMapFeatureProvider,
                                            BoundarySupportPointMapFeatureProvider supportPointMapFeatureProvider)
        {
            Ensure.NotNull(boundaryLineMapFeatureProvider, nameof(boundaryLineMapFeatureProvider));
            Ensure.NotNull(boundaryStartPointMapFeatureProvider, nameof(boundaryStartPointMapFeatureProvider));
            Ensure.NotNull(boundaryEndPointMapFeatureProvider, nameof(boundaryEndPointMapFeatureProvider));
            Ensure.NotNull(supportPointMapFeatureProvider, nameof(supportPointMapFeatureProvider));

            BoundaryLineMapFeatureProvider = boundaryLineMapFeatureProvider;
            BoundaryStartPointMapFeatureProvider = boundaryStartPointMapFeatureProvider;
            BoundaryEndPointMapFeatureProvider = boundaryEndPointMapFeatureProvider;
            SupportPointMapFeatureProvider = supportPointMapFeatureProvider;
        }

        public IFeatureProvider BoundaryLineMapFeatureProvider { get; }
        public IFeatureProvider BoundaryStartPointMapFeatureProvider { get; }
        public IFeatureProvider BoundaryEndPointMapFeatureProvider { get; }
        public IFeatureProvider SupportPointMapFeatureProvider { get; }
    }
}