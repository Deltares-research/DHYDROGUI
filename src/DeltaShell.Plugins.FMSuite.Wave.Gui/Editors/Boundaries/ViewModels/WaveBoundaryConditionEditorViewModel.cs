using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels
{
    public class WaveBoundaryConditionEditorViewModel
    {
        private readonly IWaveBoundary observedBoundary;

        /// <summary>
        /// Creates a new <see cref="WaveBoundaryConditionEditorViewModel"/>.
        /// </summary>
        /// <param name="observedBoundary"> The observed boundary. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="observedBoundary"/> is <c>null</c>.
        /// </exception>
        public WaveBoundaryConditionEditorViewModel(IWaveBoundary observedBoundary)
        { 
            this.observedBoundary = observedBoundary ?? throw new ArgumentNullException(nameof(observedBoundary));

            DescriptionViewModel = new BoundaryDescriptionViewModel(observedBoundary);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => observedBoundary.Name;

        /// <summary>
        /// Gets the <see cref="BoundaryDescriptionViewModel"/>.
        /// </summary>
        public BoundaryDescriptionViewModel DescriptionViewModel { get; }
    }
}