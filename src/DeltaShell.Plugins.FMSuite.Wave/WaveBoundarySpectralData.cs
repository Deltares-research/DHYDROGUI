using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave
{

    public enum WaveSpectrumShapeType
    {
        [Description("Jonswap")]
        Jonswap,

        [Description("Pierson-Moskowitz")]
        PiersonMoskowitz,

        [Description("Gauss")]
        Gauss
    }

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