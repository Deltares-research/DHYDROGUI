using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators
{
    /// <summary>
    /// <see cref="IAnnounceSupportPointDataChanged"/> defines the
    /// methods used to signal that support points have changed.
    /// </summary>
    public interface IAnnounceSupportPointDataChanged
    {
        /// <summary>
        /// Announces the data associated with <paramref name="supportPoint"/>
        /// has changed.
        /// </summary>
        /// <param name="supportPoint">The support of which the data has changed point.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPoint"/> is <c>null</c>.
        /// </exception>
        void AnnounceSelectedSupportPointDataChanged(SupportPoint supportPoint);

        /// <summary>
        /// Announces the support points have changed.
        /// </summary>
        void AnnounceSupportPointsChanged();
    }
}