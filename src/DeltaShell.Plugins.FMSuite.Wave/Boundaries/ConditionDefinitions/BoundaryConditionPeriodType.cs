using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="BoundaryConditionPeriodType"/> defines the possible options of
    /// wave period types.
    /// </summary>
    public enum BoundaryConditionPeriodType
    {
        [Description("Peak")]
        Peak = 1,
        
        [Description("Mean")]
        Mean = 2,
    }
}