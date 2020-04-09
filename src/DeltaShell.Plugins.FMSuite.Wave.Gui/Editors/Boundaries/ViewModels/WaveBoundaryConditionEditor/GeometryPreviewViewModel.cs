using System;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.Gui.MapView;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Containers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="GeometryPreviewViewModel"/> implements the view model for the geometry preview view.
    /// </summary>
    public sealed class GeometryPreviewViewModel : IDisposable
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

            var waveLayerFactory = new WaveLayerFactory();

            MapViewModel.Map.Layers = new EventedList<ILayer> {waveLayerFactory.CreateBoundaryLayer(featuresContainer)};
            MapViewModel.Map.ZoomToExtents();
            MapViewModel.RefreshView();

            featuresContainer.SupportPointMapFeatureProvider.FeaturesChanged +=
                (sender, Args) => MapViewModel.RefreshView();
        }

        /// <summary>
        /// Gets the <see cref="MapViewModel"/> used to render the preview.
        /// </summary>
        public MapViewModel MapViewModel { get; } = new MapViewModel();

        public void Dispose()
        {
            MapViewModel?.Dispose();
            GC.SuppressFinalize(this);

        }
    }
}