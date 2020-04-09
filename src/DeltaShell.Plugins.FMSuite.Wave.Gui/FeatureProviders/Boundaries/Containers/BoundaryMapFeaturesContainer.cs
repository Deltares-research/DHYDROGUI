using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers
{
    /// <summary>
    /// <see cref="BoundaryMapFeaturesContainer"/> acts as a convenience to
    /// group the different BoundaryMapFeatureProviders that are part of
    /// the <see cref="IWaveBoundary"/> visualisation.
    ///
    /// Upon construction the different factories, and FeatureProviders are
    /// created.
    /// </summary>
    public class BoundaryMapFeaturesContainer : IBoundaryMapFeaturesContainer
    {
        /// <summary>
        /// Creates a new <see cref="BoundaryMapFeaturesContainer"/>.
        /// </summary>
        /// <param name="boundaryContainer">The boundary container.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="boundaryContainer"/> is <c>null</c>.
        /// </exception>
        public BoundaryMapFeaturesContainer(IBoundaryContainer boundaryContainer,
                                            ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));

            var parametersFactory = new BoundaryParametersFactory();
            var dataComponentFactory = new BoundaryConditionDataComponentFactory(parametersFactory);

            var waveBoundaryFactory = new WaveBoundaryFactory(boundaryContainer,
                                                              new WaveBoundaryFactoryHelper(dataComponentFactory), 
                                                              new UniqueBoundaryNameProvider(boundaryContainer));
            var geometryFactory = new WaveBoundaryGeometryFactory(boundaryContainer, boundaryContainer);

            var addBehaviour = new BoundaryFromLineAddBehaviour(boundaryContainer, 
                                                                waveBoundaryFactory);

            BoundaryLineMapFeatureProvider = 
                new BoundaryLineMapFeatureProvider(boundaryContainer,
                                                   coordinateSystem,
                                                   geometryFactory, 
                                                   addBehaviour);
            BoundaryEndPointMapFeatureProvider = 
                new BoundaryEndPointMapFeatureProvider(boundaryContainer, 
                                                       coordinateSystem, 
                                                       geometryFactory);

            SupportPointMapFeatureProvider =
                new BoundarySupportPointMapFeatureProvider(boundaryContainer,
                                                           coordinateSystem,
                                                           geometryFactory);
        }

        public IFeatureProvider BoundaryLineMapFeatureProvider { get; }
        public IFeatureProvider BoundaryEndPointMapFeatureProvider { get; }
        public IFeatureProvider SupportPointMapFeatureProvider { get; }
    }
}