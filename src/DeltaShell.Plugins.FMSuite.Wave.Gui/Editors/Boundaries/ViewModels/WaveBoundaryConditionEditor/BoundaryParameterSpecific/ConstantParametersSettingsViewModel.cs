using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="ConstantParametersSettingsViewModel"/> defines the interface of any view
    /// model that wishes to back the ConstantParametersView.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel" />
    public abstract class ConstantParametersSettingsViewModel : IParametersSettingsViewModel
    {
        /// <summary>
        /// Gets or sets the currently displayed <see cref="ConstantParametersSettingsViewModel"/>.
        /// </summary>
        public abstract ConstantParametersViewModel ActiveParametersViewModel { get; protected set; }

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