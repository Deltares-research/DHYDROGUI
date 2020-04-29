using System.Collections.Generic;
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
        /// Creates a new <see cref="TimeDependentParametersViewModel"/>.
        /// </summary>
        protected TimeDependentParametersViewModel()
        {
            GenerateTimeSeriesCommand = new RelayCommand(_ => GenerateSeries());
        }

        /// <summary>
        /// Gets the time dependent parameters functions.
        /// </summary>
        public abstract IEnumerable<IFunction> TimeDependentParametersFunctions { get; }

        /// <summary>
        /// Gets the<see cref="ICommand"/> to generate the series for this
        /// <see cref="TimeDependentParametersViewModel"/>.
        /// </summary>
        public ICommand GenerateTimeSeriesCommand { get; }

        /// <summary>
        /// Generates the time series for the contained time dependent functions.
        /// </summary>
        protected abstract void GenerateSeries();
    }

}