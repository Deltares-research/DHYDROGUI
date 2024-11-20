using System.ComponentModel;

namespace DelftTools.Hydro.Roughness
{
    public enum RoughnessFunction
    {
        [Description("Constant")]
        Constant,
        [Description("absDischarge")]
        FunctionOfQ,
        [Description("Waterlevel")]
        FunctionOfH
    }
}