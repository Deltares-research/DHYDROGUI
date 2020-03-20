using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="TimeDependentParametersViewModel"/> defines the abstract view model for the TimeDependentParametersView.
    /// The actual values are set in the generic child class.
    /// </summary>
    public abstract class TimeDependentParametersViewModel
    {
        /// <summary>
        /// Gets the time dependent parameters function.
        /// </summary>
        public abstract IFunction TimeDependentParametersFunction { get; }
    }

    /// <summary>
    /// <see cref="TimeDependentParametersViewModel"/> defines the view model for the TimeDependentParametersView.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    public class TimeDependentParametersViewModel<TSpreading> : TimeDependentParametersViewModel
        where TSpreading : IBoundaryConditionSpreading, new()
    {
        public TimeDependentParametersViewModel(TimeDependentParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ObservedParameters = parameters;
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public TimeDependentParameters<TSpreading> ObservedParameters { get; }

        public override IFunction TimeDependentParametersFunction =>
            ObservedParameters.WaveEnergyFunction.UnderlyingFunction;
    }
}