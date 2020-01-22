using System;
using DeltaShell.NGHS.Common;
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> or
        /// <paramref name="geometryFactory"/> is <c> null </c>.
        /// </exception>
        public BoundaryGeometryViewModel(IWaveBoundary waveBoundary,
                                         IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));

            SupportPointEditorViewModel = new SupportPointEditorViewModel(waveBoundary);
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