using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public class WaveBoundarySpectralData
    {
        public WaveSpectrumShapeType ShapeType { get; set; }
        public WavePeriodType PeriodType { get; set; }
        public WaveDirSpreadType DirectionalSpreadingType { get; set; }
        public double PeakEnhancementFactor { get; set; }
        public double GaussianSpreadingValue { get; set; }
    }

    public class WaveBoundaryParameters
    {
        public double Height { get; set; }
        public double Period { get; set; } = 1.0;
        public double Direction { get; set; }
        public double Spreading { get; set; }
    }

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

    public enum WaveDirSpreadType
    {
        [Description("Power")]
        Power,
        [Description("Degrees")]
        Degrees
    }
}