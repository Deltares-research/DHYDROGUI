using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="BoundaryGeometryViewModel" /> defines the view model for the boundary geometry view.
    /// </summary>
    public class BoundaryGeometryViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BoundaryGeometryViewModel" /> class.
        /// </summary>
        /// <param name="waveBoundary"> The observed <see cref="IWaveBoundary"/>.</param>
        /// <param name="geometryFactory"> The geometry factory. </param>
        /// <param name="supportPointDataComponentViewModel">The <see cref="SupportPointEditorViewModel"/> to view.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> or
        /// <paramref name="geometryFactory"/> is <c> null </c>.
        /// </exception>
        public BoundaryGeometryViewModel(IWaveBoundary waveBoundary,
                                         IWaveBoundaryGeometryFactory geometryFactory,
                                         SupportPointDataComponentViewModel supportPointDataComponentViewModel)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));
            Ensure.NotNull(supportPointDataComponentViewModel, 
                           nameof(supportPointDataComponentViewModel));

            SupportPointEditorViewModel = new SupportPointEditorViewModel(waveBoundary.GeometricDefinition,
                                                                          supportPointDataComponentViewModel);
            GeometryPreviewViewModel = new GeometryPreviewViewModel(waveBoundary, geometryFactory);
        }

        /// <summary>
        /// Gets the <see cref="SupportPointEditorViewModel" />.
        /// </summary>
        public SupportPointEditorViewModel SupportPointEditorViewModel { get; }

        /// <summary>
        /// Gets the <see cref="GeometryPreviewViewModel"/>.
        /// </summary>
        public GeometryPreviewViewModel GeometryPreviewViewModel { get; }
    }
}