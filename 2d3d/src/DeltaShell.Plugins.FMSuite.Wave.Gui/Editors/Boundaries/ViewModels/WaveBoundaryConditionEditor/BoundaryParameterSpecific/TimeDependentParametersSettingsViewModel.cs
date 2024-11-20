using System.ComponentModel;
using System.Runtime.CompilerServices;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="TimeDependentParametersSettingsViewModel"/> defines the interface of any view
    /// model that wishes to back the TimeDependentParametersView.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel"/>
    public abstract class TimeDependentParametersSettingsViewModel : IParametersSettingsViewModel
    {
        protected readonly IGenerateSeries GenerateSeries;
        private TimeDependentParametersViewModel activeParametersViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="TimeDependentParametersSettingsViewModel"/>.
        /// </summary>
        /// <param name="generateSeries">The generate series.</param>
        /// <param name="groupBoxTitle">The title of this settings view group box.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="generateSeries"/> or
        /// <paramref name="groupBoxTitle"/> is <c>null</c>.
        /// </exception>
        protected TimeDependentParametersSettingsViewModel(IGenerateSeries generateSeries,
                                                           string groupBoxTitle)
        {
            Ensure.NotNull(generateSeries, nameof(generateSeries));
            Ensure.NotNull(groupBoxTitle, nameof(groupBoxTitle));

            GenerateSeries = generateSeries;
            GroupBoxTitle = groupBoxTitle;
        }

        /// <summary>
        /// Gets or sets the active parameters view model.
        /// </summary>
        /// <value>
        /// The active parameters view model.
        /// </value>
        public TimeDependentParametersViewModel ActiveParametersViewModel
        {
            get => activeParametersViewModel;
            protected set
            {
                if (value == ActiveParametersViewModel)
                {
                    return;
                }

                activeParametersViewModel = value;
                OnPropertyChanged();
            }
        }

        public string GroupBoxTitle { get; }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}