using System;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformConstantParametersSettingsViewModel"/> defines the view model for the ConstantParametersSettingsView.
    /// </summary>
    public class UniformConstantParametersSettingsViewModel : IConstantParametersSettingsViewModel
    {
        /// <summary>
        /// Creates a new <see cref="UniformConstantParametersSettingsViewModel"/>.
        /// </summary>
        /// <param name="activeParametersViewModel"> The vie data to be displayed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="activeParametersViewModel"/> is <c>null</c>.
        /// </exception>
        public UniformConstantParametersSettingsViewModel(ConstantParameters parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ActiveParametersViewModel = new ConstantParametersViewModel(parameters);
        }

        /// <summary>
        /// Get the currently displayed <see cref="ConstantParametersViewModel"/>.
        /// </summary>
        public ConstantParametersViewModel ActiveParametersViewModel { get; }
    }
}