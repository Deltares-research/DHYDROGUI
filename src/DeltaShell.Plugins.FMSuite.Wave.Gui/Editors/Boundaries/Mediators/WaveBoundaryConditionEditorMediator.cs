using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators
{
    /// <summary>
    /// <see cref="WaveBoundaryConditionEditorMediator"/> acts as a mediator
    /// between the different view models of the wave boundary condition
    /// editor. It ensures the different view model dependencies do not need
    /// to depend upon each other directly. Instead the actual coupling, and
    /// wiring of different calls is encapsulated here.
    /// </summary>
    /// <seealso cref="IAnnounceDataComponentChanged" />
    public class WaveBoundaryConditionEditorMediator : IAnnounceDataComponentChanged
    {
        private readonly SupportPointEditorViewModel supportPointEditorViewModel;
        private readonly BoundarySpecificParametersSettingsViewModel specificParametersSettingsViewModel;

        /// <summary>
        /// Creates a new <see cref="WaveBoundaryConditionEditorMediator"/>.
        /// </summary>
        /// <param name="supportPointEditorViewModel">The support point editor view model.</param>
        /// <param name="specificParametersSettingsViewModel">The specific parameters settings view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public WaveBoundaryConditionEditorMediator(SupportPointEditorViewModel supportPointEditorViewModel,
                                                   BoundarySpecificParametersSettingsViewModel specificParametersSettingsViewModel)

        {
            Ensure.NotNull(supportPointEditorViewModel, nameof(supportPointEditorViewModel));
            Ensure.NotNull(specificParametersSettingsViewModel, nameof(specificParametersSettingsViewModel));

            this.supportPointEditorViewModel = supportPointEditorViewModel;
            this.specificParametersSettingsViewModel = specificParametersSettingsViewModel;
        }

        public void AnnounceDataComponentChanged()
        {
            supportPointEditorViewModel.ReceiveDataComponentChanged();
        }
    }
}