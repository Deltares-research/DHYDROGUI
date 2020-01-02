using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels
{
    public class WaveBoundaryConditionEditorViewModel
    {
        private readonly IWaveBoundary observedBoundary;

        public WaveBoundaryConditionEditorViewModel(IWaveBoundary observedBoundary)
        { 
            this.observedBoundary = observedBoundary ?? throw new ArgumentNullException(nameof(observedBoundary));
        }

        public string Name => observedBoundary.Name;
    }
}