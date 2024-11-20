using BasicModelInterface;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// Specifies the DIMR logging levels.
    /// </summary>
    public static class DimrLogging
    {
        /// <summary>
        /// The feedback level key
        /// </summary>
        public const string FeedbackLevelKey = "feedbackLevel";

        /// <summary>
        /// The logfile level key
        /// </summary>
        public const string LogFileLevelKey = "debugLevel";

        /// <summary>
        /// The log file level
        /// </summary>
        public static Level LogFileLevel { get; set; } = Level.None;

        /// <summary>
        /// The feedback level
        /// </summary>
        public static Level FeedbackLevel { get; set; } = Level.None;
    }
}