using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public enum LayerType
    {
        [Description("Undefined")]
        Undefined,

        [Description("Sigma")]
        Sigma,

        [Description("Z-layer")]
        ZLayer
    }
}