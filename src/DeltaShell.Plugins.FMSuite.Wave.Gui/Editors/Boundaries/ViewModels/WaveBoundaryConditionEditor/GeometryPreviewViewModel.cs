using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.Gui.MapView;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using SharpMap.Api;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="GeometryPreviewViewModel"/> implements the view model for the geometry preview view.
    /// </summary>
    public sealed class GeometryPreviewViewModel : IRefreshGeometryView, IDisposable
    {
        /// <summary>
        /// Creates a new <see cref="GeometryPreviewViewModel"/>.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public GeometryPreviewViewModel(IWaveBoundary waveBoundary,
                                        IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));

            var singleBoundaryProvider = new SimpleBoundaryProvider(waveBoundary);

            IBoundaryMapFeaturesContainer featuresContainer = 
                BoundaryMapFeaturesContainerFactory.ConstructReadOnlyBoundaryMapFeaturesContainer(singleBoundaryProvider,
                                                                                                  geometryFactory,
                                                                                                  null);

            InitialiseLayers(MapViewModel.Map, featuresContainer);

            MapViewModel.Map.ZoomToExtents();
            MapViewModel.RefreshView();

            featuresContainer.SupportPointMapFeatureProvider.FeaturesChanged +=
                (sender, Args) => MapViewModel.RefreshView();
        }
        private static void InitialiseLayers(IMap map,
                                             IBoundaryMapFeaturesContainer featuresContainer)
        {
            var waveLayerFactory = new WaveLayerFactory();

            ILayer supportPointsLayer =
                waveLayerFactory.CreateSupportPointsLayer(featuresContainer.SupportPointMapFeatureProvider);
            ILayer startPointsLayer =
                waveLayerFactory.CreateBoundaryStartPointLayer(featuresContainer.BoundaryStartPointMapFeatureProvider);
            ILayer endPointsLayer =
                waveLayerFactory.CreateBoundaryEndPointLayer(featuresContainer.BoundaryEndPointMapFeatureProvider);
            ILayer lineLayer =
                waveLayerFactory.CreateBoundaryLineLayer(featuresContainer.BoundaryLineMapFeatureProvider);

            // For some reason the order matters due to the dispose of the Map.
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

        /// <summary>
        /// Gets the <see cref="MapViewModel"/> used to render the preview.
        /// </summary>
        public MapViewModel MapViewModel { get; } = new MapViewModel();

        public void RefreshGeometryView() => MapViewModel.RefreshView();

        public void Dispose()
        {
            MapViewModel?.Dispose();
            GC.SuppressFinalize(this);

        }
    }
}