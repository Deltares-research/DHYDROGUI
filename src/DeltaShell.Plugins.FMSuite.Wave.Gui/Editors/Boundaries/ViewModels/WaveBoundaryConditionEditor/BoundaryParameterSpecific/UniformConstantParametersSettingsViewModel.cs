using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformConstantParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ParametersSettingsView given uniform constant data.
    /// </summary>
    /// <seealso cref="ConstantParametersSettingsViewModel" />
    public sealed class UniformConstantParametersSettingsViewModel<TSpreading> : ConstantParametersSettingsViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Creates a new <see cref="UniformConstantParametersSettingsViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="parameters"> The vie data to be displayed.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        public UniformConstantParametersSettingsViewModel(ConstantParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ActiveParametersViewModel = new ConstantParametersViewModel<TSpreading>(parameters);
        }

        public override ConstantParametersViewModel ActiveParametersViewModel { get; protected set; }

        public override string GroupBoxTitle { get; protected set; } = "Uniform Constant Parameters";
    }
}