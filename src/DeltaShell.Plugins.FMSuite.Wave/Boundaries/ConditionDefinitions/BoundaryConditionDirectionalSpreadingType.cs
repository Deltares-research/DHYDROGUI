using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="BoundaryConditionDirectionalSpreadingType"/> defines the
    /// possible options of directional spreading types.
    /// </summary>
    public enum BoundaryConditionDirectionalSpreadingType
    {
        [Description("Power")]
        Power = 1,

        [Description("Degrees")]
        Degrees = 2,
    }
}