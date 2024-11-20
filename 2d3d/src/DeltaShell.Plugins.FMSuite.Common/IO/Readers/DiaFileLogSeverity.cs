using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Readers
{
    /// <summary>
    /// Log severity levels in diagnostics files (*.dia).
    /// </summary>
    public enum DiaFileLogSeverity
    {
        [Description("DEBUG")]
        Debug = 1,

        [Description("INFO")]
        Info = 2,

        [Description("WARNING")]
        Warning = 3,

        [Description("ERROR")]
        Error = 4,

        [Description("FATAL")]
        Fatal = 5
    }
}