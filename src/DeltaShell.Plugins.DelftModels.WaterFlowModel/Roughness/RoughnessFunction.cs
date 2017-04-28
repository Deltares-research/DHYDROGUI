using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness
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