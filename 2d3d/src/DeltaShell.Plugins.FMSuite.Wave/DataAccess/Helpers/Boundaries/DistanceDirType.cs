using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// <see cref="DistanceDirType"/> describes the DistanceDir values.
    /// </summary>
    [Description(KnownWaveProperties.DistanceDir)]
    public enum DistanceDirType
    {
        [Description(KnownWaveBoundariesFileConstants.CounterClockwiseDistanceDirType)]
        CounterClockwise,

        [Description(KnownWaveBoundariesFileConstants.ClockwiseDistanceDirType)]
        Clockwise
    }
}