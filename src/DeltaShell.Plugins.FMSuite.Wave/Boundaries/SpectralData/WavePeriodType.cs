using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData
{
    /// TODO: See if it is possible to add non-default values for enum.
    /// <summary>
    /// <see cref="WavePeriodType"/> defines the possible options of
    /// wave period types.
    /// </summary>
    public enum WavePeriodType
    {
        [Description("Peak")]
        Peak,

        [Description("Mean")]
        Mean
    }
}