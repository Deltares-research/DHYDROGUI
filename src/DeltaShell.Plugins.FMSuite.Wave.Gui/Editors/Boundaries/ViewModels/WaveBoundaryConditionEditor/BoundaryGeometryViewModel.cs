using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="BoundaryGeometryViewModel"/> defines the view model for the boundary geometry view.
    /// </summary>
    public class BoundaryGeometryViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BoundaryGeometryViewModel"/> class.
        /// </summary>
        /// <param name="waveBoundary"> The observed <see cref="IWaveBoundary"/>.</param>
        /// <param name="previewMapConfigurator">The preview map configurator.</param>
        /// <param name="supportPointDataComponentViewModel">The <see cref="SupportPointEditorViewModel"/> to view.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public BoundaryGeometryViewModel(IWaveBoundary waveBoundary,
                                         IGeometryPreviewMapConfigurator previewMapConfigurator,
                                         SupportPointDataComponentViewModel supportPointDataComponentViewModel)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            Ensure.NotNull(previewMapConfigurator, nameof(previewMapConfigurator));
            Ensure.NotNull(supportPointDataComponentViewModel, nameof(supportPointDataComponentViewModel));

            SupportPointEditorViewModel = new SupportPointEditorViewModel(waveBoundary.GeometricDefinition,
                                                                          supportPointDataComponentViewModel);
            GeometryPreviewViewModel = new GeometryPreviewViewModel(waveBoundary,
                                                                    supportPointDataComponentViewModel,
                                                                    previewMapConfigurator);
        }

        /// <summary>
        /// Gets the <see cref="SupportPointEditorViewModel"/>.
        /// </summary>
        public SupportPointEditorViewModel SupportPointEditorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="GeometryPreviewViewModel"/>.
        /// </summary>
        public GeometryPreviewViewModel GeometryPreviewViewModel { get; }
    }
}