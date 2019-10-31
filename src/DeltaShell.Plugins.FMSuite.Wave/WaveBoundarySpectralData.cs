using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave
{


    public enum WavePeriodType
    {
        [Description("Peak")]
        Peak,

        [Description("Mean")]
        Mean
    }

    public enum WaveDirectionalSpreadingType
    {
        [Description("Power")]
        Power,

        [Description("Degrees")]
        Degrees
    }
}