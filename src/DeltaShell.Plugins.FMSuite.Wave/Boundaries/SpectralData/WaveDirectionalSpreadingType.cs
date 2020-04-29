using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData
{
    /// TODO: See if it is possible to add non-default values for enum.
    /// <summary>
    /// <see cref="WaveDirectionalSpreadingType"/> defines the possible options of
    /// directional spreading types.
    /// </summary>
    public enum WaveDirectionalSpreadingType
    {
        [Description("Power")]
        Power,

        [Description("Degrees")]
        Degrees
    }
}