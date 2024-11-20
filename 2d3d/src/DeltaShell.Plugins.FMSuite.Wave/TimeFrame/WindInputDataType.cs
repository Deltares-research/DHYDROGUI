using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.TimeFrame
{
    /// <summary>
    /// <see cref="WindInputDataType"/> defines the possible options for wind
    /// input in the time frame editor.
    /// </summary>
    public enum WindInputDataType
    {
        [Description("Constant")]
        Constant = 1,
        [Description("Per Timepoint")]
        TimeVarying = 2,
        [Description("From File")]
        FileBased = 3,
    }
}