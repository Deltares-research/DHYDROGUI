namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="IConstantParametersSettingsViewModel"/> defines the interface of any view
    /// model that wishes to back the ConstantParametersView.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel" />
    public interface IConstantParametersSettingsViewModel : IParametersSettingsViewModel
    {
        /// <summary>
        /// Get the currently displayed <see cref="ConstantParametersViewModel"/>.
        /// </summary>
        ConstantParametersViewModel ActiveParametersViewModel { get; }
    }
}