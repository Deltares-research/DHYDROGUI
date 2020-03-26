using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{

    /// <summary>
    /// <see cref="TimeDependentUniformParametersViewModel{TSpreading}"/> defines the view model for the TimeDependentParametersView.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    public class TimeDependentUniformParametersViewModel<TSpreading> : TimeDependentParametersViewModel
        where TSpreading : IBoundaryConditionSpreading, new()
    {
        private readonly IGenerateSeries generateSeries;

        /// <summary>
        /// Creates a new <see cref="TimeDependentUniformParametersViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="generateSeries">The generate series.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public TimeDependentUniformParametersViewModel(IGenerateSeries generateSeries,
                                                       TimeDependentParameters<TSpreading> parameters)
        {
            Ensure.NotNull(generateSeries, nameof(generateSeries));
            Ensure.NotNull(parameters, nameof(parameters));

            this.generateSeries = generateSeries;
            ObservedParameters = parameters;
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public TimeDependentParameters<TSpreading> ObservedParameters { get; }

        public override IEnumerable<IFunction> TimeDependentParametersFunctions =>
            new[] {ObservedParameters.WaveEnergyFunction.UnderlyingFunction};

        protected override void GenerateSeries(IWin32Window owner) => 
            generateSeries.Execute(owner, ObservedParameters.WaveEnergyFunction);
    }
}