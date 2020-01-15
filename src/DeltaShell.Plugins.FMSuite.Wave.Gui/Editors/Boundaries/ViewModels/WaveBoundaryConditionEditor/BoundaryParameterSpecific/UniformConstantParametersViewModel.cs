using System;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformConstantParametersViewModel"/> defines the view model for the UniformConstantParametersView.
    /// </summary>
    public class UniformConstantParametersViewModel
    {
        /// <summary>
        /// Creates a new <see cref="UniformConstantParametersViewModel"/>.
        /// </summary>
        /// <param name="activeParametersViewModel"> The vie data to be displayed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="activeParametersViewModel"/> is <c>null</c>.
        /// </exception>
        public UniformConstantParametersViewModel(ConstantParametersViewModel activeParametersViewModel)
        {
            Ensure.NotNull(activeParametersViewModel, nameof(activeParametersViewModel));
            ActiveParametersViewModel = activeParametersViewModel;
        }

        /// <summary>
        /// Get the currently displayed <see cref="ConstantParametersViewModel"/>.
        /// </summary>
        public ConstantParametersViewModel ActiveParametersViewModel { get; }
    }
}