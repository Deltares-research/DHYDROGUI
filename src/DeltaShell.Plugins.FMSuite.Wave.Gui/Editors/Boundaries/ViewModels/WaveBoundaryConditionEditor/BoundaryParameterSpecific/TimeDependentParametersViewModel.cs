using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Functions;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;

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
        public abstract IEnumerable<IFunction> TimeDependentParametersFunctions { get; }
    }

    /// <summary>
    /// <see cref="TimeDependentParametersViewModel"/> defines the view model for the TimeDependentParametersView.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    public class TimeDependentParametersViewModel<TSpreading> : TimeDependentParametersViewModel
        where TSpreading : IBoundaryConditionSpreading, new()
    {
        private readonly GenerateSeries generateSeries = new GenerateSeries(new GenerateSeriesDialogHelper());

        public TimeDependentParametersViewModel(TimeDependentParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ObservedParameters = parameters;

            GenerateTimeSeriesCommand = new RelayCommand(view => GenerateSeries((IWin32Window) view));
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public TimeDependentParameters<TSpreading> ObservedParameters { get; }

        public override IEnumerable<IFunction> TimeDependentParametersFunctions =>
            new[] {ObservedParameters.WaveEnergyFunction.UnderlyingFunction};

        public ICommand GenerateTimeSeriesCommand { get; }

        private void GenerateSeries(IWin32Window owner) => generateSeries.Execute(owner, ObservedParameters.WaveEnergyFunction);
    }
}