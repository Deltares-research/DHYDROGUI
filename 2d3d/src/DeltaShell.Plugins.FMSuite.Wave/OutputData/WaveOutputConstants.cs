namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputConstants"/> defines the commonly used constants
    /// within the <see cref="WaveOutputData"/> and related classes.
    /// </summary>
    public static class WaveOutputConstants
    {
        /// <summary>
        /// The name of the log file produced by the SWAN kernel.
        /// </summary>
        public const string SwanLogFileName = "swan_bat.log";

        /// <summary>
        /// The prefix of the diagnostic file produced by the SWAN kernel.
        /// </summary>
        public const string SwanDiagnosticFilePrefix = "swn-diag.";

        /// <summary>
        /// The prefix of the SWAN input file produced by the D-Waves kernel.
        /// </summary>
        public const string SwanInputFilePrefix = "INPUT_";

        /// <summary>
        /// The .sp1 extension, used by spectra files produced by the SWAN kernel.
        /// </summary>
        public const string sp1Extension = ".sp1";

        /// <summary>
        /// The .sp2 extension, used by spectra files produced by the SWAN kernel.
        /// </summary>
        public const string sp2Extension = ".sp2";

        /// <summary>
        /// The map file prefix
        /// </summary>
        public const string MapFilePrefix = "wavm-";

        /// <summary>
        /// The history file prefix
        /// </summary>
        public const string HisFilePrefix = "wavh-";

        /// <summary>
        /// The .nc extension, used by Wave map and history files.
        /// </summary>
        public const string ncExtension = ".nc";
    }
}