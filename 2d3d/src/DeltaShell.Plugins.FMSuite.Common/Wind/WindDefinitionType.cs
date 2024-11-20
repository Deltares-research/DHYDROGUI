using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Common.Wind
{
    public enum WindDefinitionType
    {
        [Description("X/Y-components")]
        WindXWindY,

        [Description("Wind vector")]
        WindXY,

        [Description("Wind vector and air pressure")]
        WindXYP,

        [Description("Spider web grid")]
        SpiderWebGrid
    }
}