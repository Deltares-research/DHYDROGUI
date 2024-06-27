using System;
using System.Collections;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers
{
    /// <summary>
    /// <see cref="BoundaryReadOnlyMapFeatureProvider"/> is responsible for
    /// generating the features corresponding with the the provided
    /// <see cref="IFeaturesFromBoundaryBehaviour"/> of all
    /// <see cref="IWaveBoundary"/> provided by the <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <remarks>
    /// Several assumptions are made, which might be invalidated in future
    /// implementations, this will most likely require this class to be
    /// rewritten.
    /// * It is assumed that the produced features are read-only, as such
    /// no methods are provided to interact with them.
    /// Follow up issues might invalidate this invariant. In this
    /// case, <see cref="BoundaryReadOnlyMapFeatureProvider"/> will need to be
    /// extended or rewritten, to allow for such operations.
    /// * Currently it assumed any features are not explicitly added , instead we assume
    /// that once a <see cref="IWaveBoundary"/> is added through the
    /// <see cref="BoundaryLineMapFeatureProvider"/>, a refresh is triggered,
    /// which will generate the features of this <see cref="BoundaryReadOnlyMapFeatureProvider"/>
    /// anew. This is not an ideal solution, but given the minimal amount of features which will exist
    /// within a model it should sufficient.
    /// </remarks>
    /// <seealso cref="FeatureCollection"/>
    public sealed class BoundaryReadOnlyMapFeatureProvider : FeatureCollection
    {
        private readonly IBoundaryProvider boundaryProvider;
        private readonly IFeaturesFromBoundaryBehaviour featuresFromBoundaryBehaviour;

        /// <summary>
        /// Creates a new <see cref="BoundaryReadOnlyMapFeatureProvider"/>.
        /// </summary>
        /// <param name="boundaryProvider">The boundary container.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <param name="featuresFromBoundaryBehaviour">
        /// The behaviour to construct a collection of <see cref="IFeature"/> from a provided <see cref="IWaveBoundary"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryProvider"/> or
        /// <paramref name="featuresFromBoundaryBehaviour"/> is <c>null</c>.
        /// </exception>
        public BoundaryReadOnlyMapFeatureProvider(IBoundaryProvider boundaryProvider,
                                                  ICoordinateSystem coordinateSystem,
                                                  IFeaturesFromBoundaryBehaviour featuresFromBoundaryBehaviour)
        {
            Ensure.NotNull(boundaryProvider, nameof(boundaryProvider));
            Ensure.NotNull(featuresFromBoundaryBehaviour, nameof(featuresFromBoundaryBehaviour));

            this.boundaryProvider = boundaryProvider;
            this.featuresFromBoundaryBehaviour = featuresFromBoundaryBehaviour;

            CoordinateSystem = coordinateSystem;
            FeatureType = typeof(Feature2DPoint);
        }

        /// <summary>
        /// Gets the collection of features.
        /// </summary>
        /// <exception cref="NotSupportedException">Setting is currently not supported, implement when needed.</exception>
        /// <remarks>
        /// Note that calling the get method will create a new instance of the features list, with new instances
        /// of features. As such it should be cached locally when possible.
        /// </remarks>
        public override IList Features
        {
            get => boundaryProvider.Boundaries.SelectMany(featuresFromBoundaryBehaviour.Execute).ToList();
            set => throw new NotSupportedException("This is currently not supported, implement when needed.");
        }

        public override IFeature Add(IGeometry geometry) =>
            throw new NotSupportedException("This is currently not supported, implement when needed.");

        public override bool Add(IFeature feature) =>
            throw new NotSupportedException("This is currently not supported, implement when needed.");
    }
}