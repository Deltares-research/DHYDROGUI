using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories
{
    /// <summary>
    /// <see cref="BoundaryMapFeaturesContainerFactory"/> implements the
    /// construction methods for the different configured <see cref="IBoundaryMapFeaturesContainer"/>.
    /// </summary>
    public static class BoundaryMapFeaturesContainerFactory
    {
        /// <summary>
        /// Constructs an editable <see cref="IBoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new <see cref="IBoundaryMapFeaturesContainer"/> with <see cref="BoundaryFromLineAddBehaviour"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> is <c>null</c>.
        /// </exception>
        public static IBoundaryMapFeaturesContainer ConstructEditableBoundaryMapFeaturesContainer(IBoundaryContainer boundaryContainer,
                                                                                                  ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));

            IWaveBoundaryGeometryFactory geometryFactory = ConstructGeometryFactory(boundaryContainer);
            IAddBehaviour addBehaviour = ConstructBoundaryFromLineAddBehaviour(boundaryContainer);

            return ConstructFeaturesContainer(boundaryContainer,
                                              coordinateSystem,
                                              addBehaviour,
                                              geometryFactory);
        }

        /// <summary>
        /// Constructs a read-only <see cref="IBoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <param name="boundaryProvider">The boundary container.</param>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <returns>
        /// A new <see cref="IBoundaryMapFeaturesContainer"/> with <see cref="ReadOnlyAddBehaviour"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundaryProvider"/> or
        /// <paramref name="geometryFactory"/> are <c>null</c>.
        /// </exception>
        public static IBoundaryMapFeaturesContainer ConstructReadOnlyBoundaryMapFeaturesContainer(IBoundaryProvider boundaryProvider,
                                                                                                  IWaveBoundaryGeometryFactory geometryFactory,
                                                                                                  ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(boundaryProvider, nameof(boundaryProvider));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));

            IAddBehaviour addBehaviour = constructReadOnlyAddBehaviour();

            return ConstructFeaturesContainer(boundaryProvider,
                                              coordinateSystem,
                                              addBehaviour,
                                              geometryFactory);
        }

        private static BoundaryMapFeaturesContainer ConstructFeaturesContainer(IBoundaryProvider boundaryProvider,
                                                                               ICoordinateSystem coordinateSystem,
                                                                               IAddBehaviour addBehaviour,
                                                                               IWaveBoundaryGeometryFactory geometryFactory)
        {
            var boundaryLineMapFeatureProvider =
                new BoundaryLineMapFeatureProvider(boundaryProvider,
                                                   coordinateSystem,
                                                   geometryFactory,
                                                   addBehaviour);

            var boundaryStartPointMapFeatureProvider =
                new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                       coordinateSystem,
                                                       new StartingPointFromBoundaryBehaviour(geometryFactory));

            var boundaryEndPointMapFeatureProvider =
                new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                       coordinateSystem,
                                                       new EndPointFromBoundaryBehaviour(geometryFactory));

            var supportPointMapFeatureProvider =
                new BoundarySupportPointMapFeatureProvider(boundaryProvider,
                                                           coordinateSystem,
                                                           geometryFactory);

            return new BoundaryMapFeaturesContainer(boundaryLineMapFeatureProvider,
                                                    boundaryStartPointMapFeatureProvider,
                                                    boundaryEndPointMapFeatureProvider,
                                                    supportPointMapFeatureProvider);
        }

        private static BoundaryFromLineAddBehaviour ConstructBoundaryFromLineAddBehaviour(IBoundaryContainer boundaryContainer)
        {
            var parametersFactory = new ForcingTypeDefinedParametersFactory();
            var dataComponentFactory = new SpatiallyDefinedDataComponentFactory(parametersFactory);
            var waveBoundaryFactory = new WaveBoundaryFactory(boundaryContainer,
                                                              new WaveBoundaryFactoryHelper(dataComponentFactory),
                                                              new UniqueBoundaryNameProvider(boundaryContainer));

            return new BoundaryFromLineAddBehaviour(boundaryContainer,
                                                    waveBoundaryFactory);
        }

        private static ReadOnlyAddBehaviour constructReadOnlyAddBehaviour() =>
            new ReadOnlyAddBehaviour();

        private static IWaveBoundaryGeometryFactory ConstructGeometryFactory(IBoundaryContainer boundaryContainer) =>
            new WaveBoundaryGeometryFactory(boundaryContainer, boundaryContainer);
    }
}