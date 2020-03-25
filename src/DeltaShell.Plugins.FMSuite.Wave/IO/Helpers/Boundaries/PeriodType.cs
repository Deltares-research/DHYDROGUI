using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Period type of the wave boundary
    /// </summary>
    public enum PeriodType
    {
        [Description("mean")]
        Mean,

        [Description("peak")]
        Peak
    }
}