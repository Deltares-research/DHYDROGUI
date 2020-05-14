namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// <see cref="BndExtForceFileConstants"/> contains constant values for the boundary external forcing file.
    /// </summary>
    public static class BndExtForceFileConstants
    {
        /// <summary>
        /// The boundary category key.
        /// </summary>
        public const string BoundaryBlockKey = "[boundary]";

        /// <summary>
        /// The quantity property key.
        /// </summary>
        public const string QuantityKey = "quantity";

        /// <summary>
        /// The location file property key.
        /// </summary>
        public const string LocationFileKey = "locationfile";

        /// <summary>
        /// The forcing file property key.
        /// </summary>
        public const string ForcingFileKey = "forcingfile";

        /// <summary>
        /// The thatcher harleman time lag property key.
        /// </summary>
        public const string ThatcherHarlemanTimeLagKey = "return_time";

        /// <summary>
        /// The open boundary tolerance property key.
        /// </summary>
        public const string OpenBoundaryToleranceKey = "OpenBoundaryTolerance";
    }
}