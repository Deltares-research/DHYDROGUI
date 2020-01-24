using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformConstantParametersSettingsViewModel"/> defines the view model for the
    /// ConstantParametersSettingsView given uniform data.
    /// </summary>
    /// <seealso cref="ConstantParametersSettingsViewModel" />
    public sealed class UniformConstantParametersSettingsViewModel : ConstantParametersSettingsViewModel
    {
        /// <summary>
        /// Creates a new <see cref="UniformConstantParametersSettingsViewModel"/>.
        /// </summary>
        /// <param name="parameters"> The vie data to be displayed.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        public UniformConstantParametersSettingsViewModel(ConstantParameters parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ActiveParametersViewModel = new ConstantParametersViewModel(parameters);
        }

        /// <summary>
        /// Get the currently displayed <see cref="ConstantParametersViewModel"/>.
        /// </summary>
        public override ConstantParametersViewModel ActiveParametersViewModel { get; protected set; }

        public override string GroupBoxTitle { get; protected set; } = "Uniform Constant Parameters";
    }
}