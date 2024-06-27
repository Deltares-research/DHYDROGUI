using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators
{
    /// <summary>
    /// <see cref="DataComponentChangeMediator"/> implements the interface with which
    /// can be communicated that the data associated with a support point has changed.
    /// </summary>
    /// <seealso cref="IAnnounceSupportPointDataChanged"/>
    public class DataComponentChangeMediator : IAnnounceSupportPointDataChanged
    {
        private readonly IRefreshDataComponentViewModel selectedSupportPointDependentViewModel;

        /// <summary>
        /// Creates a new instance of the <see cref="DataComponentChangeMediator"/>.
        /// </summary>
        /// <param name="selectedSupportPointDependentViewModel">The selected support point dependent view model.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="selectedSupportPointDependentViewModel"/> is <c> null</c>.
        /// </exception>
        public DataComponentChangeMediator(IRefreshDataComponentViewModel selectedSupportPointDependentViewModel)
        {
            Ensure.NotNull(selectedSupportPointDependentViewModel, nameof(selectedSupportPointDependentViewModel));
            this.selectedSupportPointDependentViewModel = selectedSupportPointDependentViewModel;
        }

        /// <summary>
        /// Gets or sets the <see cref="IRefreshGeometryView"/>.
        /// </summary>
        public IRefreshGeometryView RefreshGeometryView { get; set; }

        public void AnnounceSelectedSupportPointDataChanged(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));
            selectedSupportPointDependentViewModel.UpdateSelectedActiveParameters(supportPoint);
        }

        public void AnnounceSupportPointsChanged()
        {
            RefreshGeometryView?.RefreshGeometryView();
        }
    }
}