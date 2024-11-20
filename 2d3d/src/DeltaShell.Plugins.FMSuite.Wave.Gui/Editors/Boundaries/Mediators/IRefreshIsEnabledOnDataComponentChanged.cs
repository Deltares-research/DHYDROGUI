namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators
{
    /// <summary>
    /// <see cref="IRefreshIsEnabledOnDataComponentChanged"/> defines the interface
    /// to trigger a Refresh of the is enabled property when the DataComponent has
    /// changed.
    /// </summary>
    public interface IRefreshIsEnabledOnDataComponentChanged
    {
        /// <summary>
        /// Reevaluates whether this component is enabled.
        /// </summary>
        void RefreshIsEnabled();
    }
}