using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours
{
    /// <summary>
    /// <see cref="IFeaturesFromBoundaryBehaviour"/> defines the behaviours to
    /// construct collections of <see cref="IFeature"/> from a given <see cref="IWaveBoundary"/>.
    /// This is utilised by the <see cref="BoundaryReadOnlyMapFeatureProvider"/> to construct its
    /// features.
    /// </summary>
    public interface IFeaturesFromBoundaryBehaviour
    {
        /// <summary>
        /// Executes this <see cref="IFeaturesFromBoundaryBehaviour"/> by constructing the relevant
        /// collection of <see cref="IFeature"/> from the given <paramref name="waveBoundary"/>
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <returns>
        /// The collection of <see cref="IFeature"/> constructed from the given <paramref name="waveBoundary"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        IEnumerable<IFeature> Execute(IWaveBoundary waveBoundary);
    }
}