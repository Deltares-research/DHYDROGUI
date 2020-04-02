using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="FileBasedParametersSettingsViewModel"/> defines the interface of any view
    /// model that wishes to back the FileBasedParametersView.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel" />
    /// <seealso cref="INotifyPropertyChanged" />
    public abstract class FileBasedParametersSettingsViewModel : IParametersSettingsViewModel,
                                                                 INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the currently displayed <see cref="FileBasedParametersViewModel"/>.
        /// </summary>
        public abstract FileBasedParametersViewModel ActiveParametersViewModel { get; protected set; }

        /// <summary>
        /// Gets or sets the group box title.
        /// </summary>
        public abstract string GroupBoxTitle { get; protected set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}