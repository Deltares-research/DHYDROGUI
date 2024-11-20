using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Provides constants for the statistical analysis configuration file (*.fou).
    /// </summary>
    public static class FouFileConstants
    {
        /// <summary>
        /// Gets the default fou file name.
        /// </summary>
        public const string DefaultFileName = "Maxima.fou";

        /// <summary>
        /// Gets the delimiter used to indicate comments in the fou file.
        /// </summary>
        public const char CommentDelimiter = '*';

        /// <summary>
        /// Gets the width of each column in the fou file.
        /// </summary>
        public const int ColumnWidth = 10;

        /// <summary>
        /// Gets the default fou file header.
        /// </summary>
        public static readonly string FileHeader = Resources.DefaultFouFileHeader;
    }
}