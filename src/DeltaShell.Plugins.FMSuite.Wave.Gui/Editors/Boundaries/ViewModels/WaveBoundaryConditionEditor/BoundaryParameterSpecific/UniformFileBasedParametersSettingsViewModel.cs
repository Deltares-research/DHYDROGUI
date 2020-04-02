using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformFileBasedParametersSettingsViewModel"/> defines the view model for the
    /// ParametersSettingsView given uniform constant data.
    /// </summary>
    /// <seealso cref="FileBasedParametersSettingsViewModel" />
    public sealed class UniformFileBasedParametersSettingsViewModel : FileBasedParametersSettingsViewModel
    {
        /// <summary>
        /// Creates a new <see cref="UniformFileBasedParametersSettingsViewModel"/>.
        /// </summary>
        /// <param name="parameters">The view data to be displayed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        public UniformFileBasedParametersSettingsViewModel(FileBasedParameters parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ActiveParametersViewModel = new FileBasedParametersViewModel(parameters);
        }

        public override FileBasedParametersViewModel ActiveParametersViewModel { get; protected set; }

        public override string GroupBoxTitle { get; protected set; } = "Uniform File Based Parameters";
    }
}