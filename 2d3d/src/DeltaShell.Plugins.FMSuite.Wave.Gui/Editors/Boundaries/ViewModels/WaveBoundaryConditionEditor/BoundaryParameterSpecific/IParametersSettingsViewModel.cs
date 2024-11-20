using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="IParametersSettingsViewModel"/> defines the different parameter
    /// settings view models used within the Boundary Parameters Specific Settings view.
    /// Currently, the following are defined:
    /// * <see cref="ConstantParametersSettingsViewModel"/>
    /// * <see cref="TimeDependentParametersSettingsViewModel"/>
    /// * <see cref="FileBasedParametersSettingsViewModel"/>
    /// </summary>
    /// <remarks>
    /// This interface acts as a discriminated union for its implementing types.
    /// As such, this interface is empty. Other classes will use it to type
    /// cast to the implementing types.
    /// </remarks>
    /// <seealso cref="INotifyPropertyChanged"/>
    public interface IParametersSettingsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the group box title.
        /// </summary>
        string GroupBoxTitle { get; }
    }
}