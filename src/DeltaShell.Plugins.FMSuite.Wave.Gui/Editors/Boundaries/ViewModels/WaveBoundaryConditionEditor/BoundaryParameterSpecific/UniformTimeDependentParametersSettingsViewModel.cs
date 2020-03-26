using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformConstantParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ParametersSettingsView given uniform time dependent data.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    /// <seealso cref="TimeDependentParametersSettingsViewModel" />
    public sealed class UniformTimeDependentParametersSettingsViewModel<TSpreading> : TimeDependentParametersSettingsViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        public UniformTimeDependentParametersSettingsViewModel(TimeDependentParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ActiveParametersViewModel = new TimeDependentUniformParametersViewModel<TSpreading>(parameters);
        }

        public override TimeDependentParametersViewModel ActiveParametersViewModel { get; protected set; }

        public override string GroupBoxTitle { get; protected set; } = "Uniform Time Dependent Parameters";
    }
}