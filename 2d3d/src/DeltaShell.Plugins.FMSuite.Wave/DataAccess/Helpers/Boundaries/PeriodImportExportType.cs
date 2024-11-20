using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Period import and export type of the wave boundary.
    /// </summary>
    public enum PeriodImportExportType
    {
        [Description(KnownWaveBoundariesFileConstants.MeanPeriodType)]
        Mean,

        [Description(KnownWaveBoundariesFileConstants.PeakPeriodType)]
        Peak
    }
}