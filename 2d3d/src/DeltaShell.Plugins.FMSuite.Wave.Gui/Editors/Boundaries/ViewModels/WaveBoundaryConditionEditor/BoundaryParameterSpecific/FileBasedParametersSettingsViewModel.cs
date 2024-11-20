using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="FileBasedParametersSettingsViewModel"/> defines the interface of any view
    /// model that wishes to back the FileBasedParametersView.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel"/>
    public abstract class FileBasedParametersSettingsViewModel : IParametersSettingsViewModel
    {
        private FileBasedParametersViewModel activeParametersViewModel;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the currently displayed <see cref="FileBasedParametersViewModel"/>.
        /// </summary>
        public FileBasedParametersViewModel ActiveParametersViewModel
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