using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="TimeDependentParametersSettingsViewModel"/> defines the interface of any view
    /// model that wishes to back the TimeDependentParametersView.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel" />
    /// <seealso cref="INotifyPropertyChanged" />
    public abstract class TimeDependentParametersSettingsViewModel : IParametersSettingsViewModel,
                                                                     INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the active parameters view model.
        /// </summary>
        /// <value>
        /// The active parameters view model.
        /// </value>
        public abstract TimeDependentParametersViewModel ActiveParametersViewModel { get; protected set; }

        /// <summary>
        /// Gets or sets the group box title.
        /// </summary>
        public abstract string GroupBoxTitle { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}