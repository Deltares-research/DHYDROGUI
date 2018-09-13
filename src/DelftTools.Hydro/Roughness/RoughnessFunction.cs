using System.ComponentModel;

namespace DelftTools.Hydro.Roughness
{
    public enum RoughnessFunction
    {
        [Description("Constant")]
        Constant,
        [Description("Discharge")]
        FunctionOfQ,
        [Description("Waterlevel")]
        FunctionOfH
    }
}