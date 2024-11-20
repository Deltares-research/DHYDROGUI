using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers
{
    /// <summary>
    /// <see cref="IBoundaryMapFeaturesContainer"/> defines the interface to access the different
    /// feature providers of the boundary layer.
    /// </summary>
    public interface IBoundaryMapFeaturesContainer
    {
        /// <summary>
        /// Gets the boundary line map feature provider of this
        /// <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <value>
        /// The boundary line map feature provider.
        /// </value>
        IFeatureProvider BoundaryLineMapFeatureProvider { get; }

        /// <summary>
        /// Gets the boundary start point map feature provider of this
        /// <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <value>
        /// The boundary start point map feature provider.
        /// </value>
        IFeatureProvider BoundaryStartPointMapFeatureProvider { get; }

        /// <summary>
        /// Gets the boundary end point map feature provider of this
        /// <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <value>
        /// The boundary end point map feature provider.
        /// </value>
        IFeatureProvider BoundaryEndPointMapFeatureProvider { get; }

        /// <summary>
        /// Gets the support point map feature provider of this
        /// <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <value>
        /// The boundary support point map feature provider.
        /// </value>
        IFeatureProvider SupportPointMapFeatureProvider { get; }
    }
}