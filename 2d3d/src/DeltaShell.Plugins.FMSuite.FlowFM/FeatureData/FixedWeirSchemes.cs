using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public enum FixedWeirSchemes
    {
        [Description("None")]
        None = 0,

        [Description("Numerical")]
        Scheme6 = 6,

        [Description("Tabellenboek")]
        Scheme8 = 8,

        [Description("Villemonte")]
        Scheme9 = 9
    }
}