using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
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

            var factory = new ViewShapeFactory(new BoundaryConditionShapeFactory());
            BoundaryWideParametersViewModel = new BoundaryWideParametersViewModel(observedBoundary.ConditionDefinition, factory);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name => observedBoundary.Name;

        /// <summary>
        /// Gets the <see cref="BoundaryDescriptionViewModel"/>.
        /// </summary>
        public BoundaryDescriptionViewModel DescriptionViewModel { get; }

        /// <summary>
        /// Gets the <see cref="BoundaryWideParametersViewModel"/>.
        /// </summary>
        public BoundaryWideParametersViewModel BoundaryWideParametersViewModel { get; }
    }
}