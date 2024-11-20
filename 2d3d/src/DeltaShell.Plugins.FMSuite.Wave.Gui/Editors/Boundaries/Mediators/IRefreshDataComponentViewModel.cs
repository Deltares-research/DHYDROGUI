using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators
{
    /// <summary>
    /// <see cref="IRefreshDataComponentViewModel"/> defines the interface for
    /// refreshing the DataComponentViewModel.
    /// </summary>
    public interface IRefreshDataComponentViewModel
    {
        /// <summary>
        /// Refreshes the data component view model.
        /// </summary>
        void RefreshDataComponentViewModel();

        /// <summary>
        /// Updates the selected active parameters parameters.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPoint"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the data is not spatially variant.
        /// </exception>
        void UpdateSelectedActiveParameters(SupportPoint supportPoint);
    }
}