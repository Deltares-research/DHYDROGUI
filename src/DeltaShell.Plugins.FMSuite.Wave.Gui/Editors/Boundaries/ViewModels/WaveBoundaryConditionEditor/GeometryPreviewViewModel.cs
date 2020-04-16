using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.Gui.MapView;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;

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
        /// <param name="supportPointDataComponentViewModel">The support point data component view model.</param>
        /// <param name="configurator">The map configurator used to configure this instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public GeometryPreviewViewModel(IWaveBoundary waveBoundary,
                                        SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                        IGeometryPreviewMapConfigurator configurator)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            Ensure.NotNull(supportPointDataComponentViewModel, nameof(supportPointDataComponentViewModel));
            Ensure.NotNull(configurator, nameof(configurator));

            var singleBoundaryProvider = new SimpleBoundaryProvider(waveBoundary);
            configurator.ConfigureMap(MapViewModel.Map, 
                                      singleBoundaryProvider, 
                                      supportPointDataComponentViewModel, 
                                      this);

            MapViewModel.RefreshView();
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