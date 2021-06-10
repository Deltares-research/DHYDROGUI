namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame
{
    /// <summary>
    /// <see cref="TimeFrameFileFilters"/> defines the file filters used in a
    /// <see cref="Helpers.ITimeFrameEditorFileImportHelper"/>.
    /// </summary>
    public static class TimeFrameFileFilters
    {
        /// <summary>
        /// The wind velocity file filter, defined as *wnd files.
        /// </summary>
        public const string WindVelocity =
            "Wind Velocity (*.wnd)|*.wnd|All files (*.*)|*.*";

        /// <summary>
        /// The uniform x series file filter, defined as *.wnd and *.amu files.
        /// </summary>
        public const string UniformXSeries =
            "Uniform X series (*.wnd;*.amu)|*.wnd;*.amu|All files (*.*)|*.*";

        /// <summary>
        /// The uniform y series file filter, defined as *.wnd and *.amv files.
        /// </summary>
        public const string UniformYSeries =
            "Uniform Y series (*.wnd;*.amv)|*.wnd;*.amv|All files (*.*)|*.*";

        /// <summary>
        /// The spider web file filter, defined as *.spw files.
        /// </summary>
        public const string SpiderWeb =
            "Spider Web (*.spw)|*.spw|All files (*.*)|*.*";
    }
}