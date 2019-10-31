using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData
{
    /// TODO: See if it is possible to add non-default values for enum.
    /// <summary>
    /// <see cref="WaveSpectrumShapeType"/> defines the possible options of
    /// wave spectrum shape types.
    /// </summary>
    public enum WaveSpectrumShapeType
    {
        [Description("Jonswap")]
        Jonswap,

        [Description("Pierson-Moskowitz")]
        PiersonMoskowitz,

        [Description("Gauss")]
        Gauss,
    }
}