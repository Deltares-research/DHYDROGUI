using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators
{
    /// <summary>
    /// <see cref="IAnnounceSelectedSupportPointHasChanged"/> defines the interface
    /// used to signal that the data defined by the selected support point has changed.
    /// Note this can either be the case when a different support point is selected, or
    /// when the data associated with the support point is changed.
    /// </summary>
    public interface IAnnounceSelectedSupportPointHasChanged
    {
        /// <summary>
        /// Announces the selected support point data component has changed.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPoint"/> is <c>null</c>.
        /// </exception>
        void AnnounceSelectedSupportPointDataComponentHasChanged(SupportPoint supportPoint);
    }
}