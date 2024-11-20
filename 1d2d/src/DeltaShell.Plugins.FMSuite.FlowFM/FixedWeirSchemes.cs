using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public enum FixedWeirSchemes
    {
        [Description("No scheme selected")]
        None = 0,
        [Description("Fixed Weir Scheme 6")]
        Scheme6 = 6,
        [Description("Fixed Weir Scheme 8")]
        Scheme8 = 8,
        [Description("Fixed Weir Scheme 9")]
        Scheme9 = 9,
    }
}