using System;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

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
        /// <param name="observedGeometricDefinition"> The observed geometric definition. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="observedGeometricDefinition" /> is <c> null </c>.
        /// </exception>
        public BoundaryGeometryViewModel(IWaveBoundaryGeometricDefinition observedGeometricDefinition)
        {
            Ensure.NotNull(observedGeometricDefinition, nameof(observedGeometricDefinition));
            SupportPointEditorViewModel = new SupportPointEditorViewModel(observedGeometricDefinition);
        }

        /// <summary>
        /// Gets the <see cref="SupportPointEditorViewModel" />.
        /// </summary>
        public SupportPointEditorViewModel SupportPointEditorViewModel { get; }
    }
}