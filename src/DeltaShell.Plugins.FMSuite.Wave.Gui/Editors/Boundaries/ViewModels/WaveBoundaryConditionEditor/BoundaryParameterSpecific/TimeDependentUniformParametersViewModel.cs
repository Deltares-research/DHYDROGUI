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
        private readonly GenerateSeries generateSeries = 
            new GenerateSeries(new GenerateSeriesDialogHelper());

        public TimeDependentUniformParametersViewModel(TimeDependentParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
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