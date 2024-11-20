using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformConstantParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ParametersSettingsView given uniform constant data.
    /// </summary>
    /// <seealso cref="ConstantParametersSettingsViewModel"/>
    public sealed class UniformConstantParametersSettingsViewModel<TSpreading> : ConstantParametersSettingsViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Creates a new <see cref="UniformConstantParametersSettingsViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="parameters">The view data to be displayed.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        public UniformConstantParametersSettingsViewModel(ConstantParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ActiveParametersViewModel = new ConstantParametersViewModelGeneric<TSpreading>(parameters);

            GroupBoxTitle = Resources.UniformConstantParametersSettingsViewModel_GroupBoxTitle;
        }
    }
}