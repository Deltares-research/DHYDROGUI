namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators
{
    /// <summary>
    /// <see cref="IAnnounceDataComponentChanged"/> defines the interface
    /// with which can be announced that the description, i.e. the forcing
    /// type and spatial definition type, have been changed.
    /// </summary>
    public interface IAnnounceDataComponentChanged
    {
        /// <summary>
        /// Announces that the boundary data component has changed.
        /// </summary>
        void AnnounceDataComponentChanged();
    }
}