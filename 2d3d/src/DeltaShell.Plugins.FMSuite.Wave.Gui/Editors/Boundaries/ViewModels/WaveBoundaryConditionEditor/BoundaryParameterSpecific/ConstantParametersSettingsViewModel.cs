using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="ConstantParametersSettingsViewModel"/> defines the interface of any view
    /// model that wishes to back the ConstantParametersView.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel"/>
    public abstract class ConstantParametersSettingsViewModel : IParametersSettingsViewModel
    {
        private ConstantParametersViewModel activeParametersViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the currently displayed <see cref="ConstantParametersSettingsViewModel"/>.
        /// </summary>
        public ConstantParametersViewModel ActiveParametersViewModel
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

        public string GroupBoxTitle { get; protected set; }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}