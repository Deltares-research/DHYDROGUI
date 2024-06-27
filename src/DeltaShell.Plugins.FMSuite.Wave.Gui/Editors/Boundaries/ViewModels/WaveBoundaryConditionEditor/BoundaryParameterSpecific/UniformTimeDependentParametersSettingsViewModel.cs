using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="UniformConstantParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ParametersSettingsView given uniform time dependent data.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    /// <seealso cref="TimeDependentParametersSettingsViewModel"/>
    public sealed class UniformTimeDependentParametersSettingsViewModel<TSpreading> : TimeDependentParametersSettingsViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Creates a new <see cref="UniformTimeDependentParametersSettingsViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="generateSeries">The <see cref="IGenerateSeries"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> or <paramref name="generateSeries"/>
        /// is <c>null</c>.
        /// </exception>
        public UniformTimeDependentParametersSettingsViewModel(TimeDependentParameters<TSpreading> parameters, IGenerateSeries generateSeries) :
            base(generateSeries, Resources.UniformTimeDependentParametersSettingsViewModel_GroupBoxTitle)
        {
            Ensure.NotNull(parameters, nameof(parameters));

            ActiveParametersViewModel = new TimeDependentUniformParametersViewModel<TSpreading>(GenerateSeries, parameters);
        }
    }
}