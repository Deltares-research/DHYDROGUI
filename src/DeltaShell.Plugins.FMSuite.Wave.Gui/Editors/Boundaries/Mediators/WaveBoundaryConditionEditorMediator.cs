using DelftTools.Utils.Guards;

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
        private readonly IRefreshIsEnabledOnDataComponentChanged dataComponentIsEnabledDependentViewModel;
        private readonly IRefreshDataComponentViewModel dataComponentViewModelDependentViewModel;
        private readonly IRefreshViewModel refreshViewModel;

        /// <summary>
        /// Creates a new <see cref="WaveBoundaryConditionEditorMediator"/>.
        /// </summary>
        /// <param name="dataComponentIsEnabledDependentViewModel">The support point editor view model.</param>
        /// <param name="dataComponentViewModelDependentViewModel">The data component view model dependent view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public WaveBoundaryConditionEditorMediator(IRefreshIsEnabledOnDataComponentChanged dataComponentIsEnabledDependentViewModel,
                                                   IRefreshDataComponentViewModel dataComponentViewModelDependentViewModel,
                                                   IRefreshViewModel refreshViewModel)

        {
            Ensure.NotNull(dataComponentIsEnabledDependentViewModel, nameof(dataComponentIsEnabledDependentViewModel));
            Ensure.NotNull(dataComponentViewModelDependentViewModel, nameof(dataComponentViewModelDependentViewModel));
            Ensure.NotNull(refreshViewModel, nameof(refreshViewModel));

            this.dataComponentIsEnabledDependentViewModel = dataComponentIsEnabledDependentViewModel;
            this.dataComponentViewModelDependentViewModel = dataComponentViewModelDependentViewModel;
            this.refreshViewModel = refreshViewModel;
        }

        public void AnnounceDataComponentChanged()
        {
            dataComponentViewModelDependentViewModel.RefreshDataComponentViewModel();
            dataComponentIsEnabledDependentViewModel.RefreshIsEnabled();
            refreshViewModel.RefreshViewModel();
        }
    }
}