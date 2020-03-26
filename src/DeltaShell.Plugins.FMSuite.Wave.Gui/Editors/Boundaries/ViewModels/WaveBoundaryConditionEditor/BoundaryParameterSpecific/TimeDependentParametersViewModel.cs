using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="TimeDependentParametersViewModel"/> defines the abstract view model for the TimeDependentParametersView.
    /// The actual values are set in the generic child class.
    /// </summary>
    public abstract class TimeDependentParametersViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        protected TimeDependentParametersViewModel()
        {
            GenerateTimeSeriesCommand = new RelayCommand(view => GenerateSeries((IWin32Window) view));
        }

        /// <summary>
        /// Gets the time dependent parameters function.
        /// </summary>
        public abstract IEnumerable<IFunction> TimeDependentParametersFunctions { get; }

        /// <summary>
        /// Gets the<see cref="ICommand"/> to generate the series for this
        /// <see cref="Wave.Boundaries.ConditionDefinitions.Parameters.TimeDependentParameters{TSpreading}"/>.
        /// </summary>
        public ICommand GenerateTimeSeriesCommand { get; }

        /// <summary>
        /// Generates the time series for the contained time dependent functions.
        /// </summary>
        /// <param name="owner">The owning window required for user prompts.</param>
        protected abstract void GenerateSeries(IWin32Window owner);
    }

}