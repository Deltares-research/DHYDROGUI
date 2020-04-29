using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Api;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
{
    /// <summary>
    /// <see cref="GeometryPreviewMapConfigurator"/> implements the interface
    /// with which to configure the <see cref="IMap"/> of a geometry preview.
    /// </summary>
    /// <seealso cref="IGeometryPreviewMapConfigurator" />
    public class GeometryPreviewMapConfigurator : IGeometryPreviewMapConfigurator
    {
        private readonly IWaveBoundaryGeometryFactory geometryFactory;
        private readonly IWaveLayerFactory layerFactory;
        private readonly ICoordinateSystem coordinateSystem;

        /// <summary>
        /// Creates a new <see cref="GeometryPreviewMapConfigurator"/>.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <param name="layerFactory">The layer factory.</param>
        /// <param name="coordinateSystem">The coordinate system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="geometryFactory"/> or <paramref name="layerFactory"/>
        /// is <c>null</c>.
        /// </exception>
        public GeometryPreviewMapConfigurator(IWaveBoundaryGeometryFactory geometryFactory,
                                              IWaveLayerFactory layerFactory,
                                              ICoordinateSystem coordinateSystem)
        {
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));
            Ensure.NotNull(layerFactory, nameof(layerFactory));

            this.geometryFactory = geometryFactory;
            this.layerFactory = layerFactory;
            this.coordinateSystem = coordinateSystem;
        }

        public void ConfigureMap(IMap map, 
                                 IBoundaryProvider boundaryProvider, 
                                 SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                 IRefreshGeometryView refreshGeometryView)
        {
            Ensure.NotNull(map, nameof(map));
            Ensure.NotNull(boundaryProvider, nameof(boundaryProvider));
            Ensure.NotNull(supportPointDataComponentViewModel, nameof(supportPointDataComponentViewModel));
            Ensure.NotNull(refreshGeometryView, nameof(refreshGeometryView));

            IBoundaryMapFeaturesContainer featuresContainer = 
                BoundaryMapFeaturesContainerFactory.ConstructReadOnlyBoundaryMapFeaturesContainer(boundaryProvider,
                                                                                                  geometryFactory,
                                                                                                  coordinateSystem);
            ConstructBoundaryMapLayers(featuresContainer, map);
            ConstructActiveSupportPointLayers(boundaryProvider, supportPointDataComponentViewModel, map);
            ConstructInactiveSupportPointLayers(boundaryProvider, supportPointDataComponentViewModel, map);
            ConstructSelectedSupportPointLayers(boundaryProvider, supportPointDataComponentViewModel, map);

            featuresContainer.SupportPointMapFeatureProvider.FeaturesChanged +=
                (sender, args) => refreshGeometryView.RefreshGeometryView();

            map.ZoomToExtents();
        }

        private void ConstructBoundaryMapLayers(IBoundaryMapFeaturesContainer featuresContainer,
                                                IMap map)
        {
            ILayer supportPointsLayer =
                layerFactory.CreateSupportPointsLayer(featuresContainer.SupportPointMapFeatureProvider);
            ILayer startPointsLayer =
                layerFactory.CreateBoundaryStartPointLayer(featuresContainer.BoundaryStartPointMapFeatureProvider);
            ILayer endPointsLayer =
                layerFactory.CreateBoundaryEndPointLayer(featuresContainer.BoundaryEndPointMapFeatureProvider);
            ILayer lineLayer =
                layerFactory.CreateBoundaryLineLayer(featuresContainer.BoundaryLineMapFeatureProvider);

            map.Layers.AddRange(new[]
            {
                supportPointsLayer,
                startPointsLayer,
                endPointsLayer,
                lineLayer,
            });

            startPointsLayer.RenderOrder = 1;
            endPointsLayer.RenderOrder = 1;
            supportPointsLayer.RenderOrder = 2;
            lineLayer.RenderOrder = 5;
        }

        private void ConstructActiveSupportPointLayers(IBoundaryProvider boundaryProvider, 
                                                       SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                                       IMap map)
        {
            var activeSupportPointBehaviour =
                new ToggledSupportPointsFromBoundaryBehaviour(true, 
                                                              supportPointDataComponentViewModel, 
                                                              geometryFactory);
            var activeSupportPointProvider =
                new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                       coordinateSystem,
                                                       activeSupportPointBehaviour);

            ILayer activeSupportPointLayer =
                layerFactory.CreateActiveSupportPointsLayer(activeSupportPointProvider);

            map.Layers.Add(activeSupportPointLayer);

            activeSupportPointLayer.RenderOrder = 3;
        }

        private void ConstructInactiveSupportPointLayers(IBoundaryProvider boundaryProvider, 
                                                         SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                                         IMap map)
        {
            var inactiveSupportPointBehaviour =
                new ToggledSupportPointsFromBoundaryBehaviour(false, 
                                                              supportPointDataComponentViewModel, 
                                                              geometryFactory);
            var inactiveSupportPointProvider =
                new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                                                       coordinateSystem,
                                                       inactiveSupportPointBehaviour);

            ILayer inactiveSupportPointLayer =
                layerFactory.CreateInactiveSupportPointsLayer(inactiveSupportPointProvider);

            map.Layers.Add(inactiveSupportPointLayer);

            inactiveSupportPointLayer.RenderOrder = 4;
        }

        private void ConstructSelectedSupportPointLayers(IBoundaryProvider boundaryProvider, 
                                                         SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                                         IMap map)
        {
            var selectedSupportPointBehaviour =
                new SelectedSupportPointFromBoundaryBehaviour(supportPointDataComponentViewModel,
                                                              geometryFactory);

            var selectedSupportPointProvider =
                new BoundaryReadOnlyMapFeatureProvider(boundaryProvider,
                    coordinateSystem,
                    selectedSupportPointBehaviour);

            ILayer selectedSupportPointLayer =
                layerFactory.CreateSelectedSupportPointLayer(selectedSupportPointProvider);

            map.Layers.Add(selectedSupportPointLayer);

            selectedSupportPointLayer.RenderOrder = 6;
        }
    }
}