namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility
{
    /// <summary>
    /// Class that contains known legacy properties.
    /// </summary>
    public static class KnownLegacyProperties
    {
        public const string TStart = "tstart"; // Replaced by StartDateTime
        public const string TStop = "tstop";   // Replaced by StopDateTime
    }
}