using DeltaShell.NGHS.Common;
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

        /// <summary>
        /// Creates a new <see cref="WaveBoundaryConditionEditorMediator"/>.
        /// </summary>
        /// <param name="supportPointEditorViewModel">The support point editor view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPointEditorViewModel"/> is <c>null</c>.
        /// </exception>
        public WaveBoundaryConditionEditorMediator(SupportPointEditorViewModel supportPointEditorViewModel)
        {
            Ensure.NotNull(supportPointEditorViewModel, nameof(supportPointEditorViewModel));
            this.supportPointEditorViewModel = supportPointEditorViewModel;
        }

        public void AnnounceDataComponentChanged()
        {
            supportPointEditorViewModel.ReceiveDataComponentChanged();
        }
    }
}