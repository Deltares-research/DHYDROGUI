using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
{
    /// <summary>
    /// <see cref="HydrodynamicsInputDataType"/> defines the possible options
    /// for hydrodynamics input in the time frame editor.
    /// </summary>
    public enum HydrodynamicsInputDataType
    {
        [Description("Constant")]
        Constant = 1,
        [Description("Per Timepoint")]
        TimeVarying = 2,
    }
}