using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Period import type of the wave boundary
    /// </summary>
    public enum PeriodImportType
    {
        [Description("mean")]
        Mean,

        [Description("peak")]
        Peak
    }
}