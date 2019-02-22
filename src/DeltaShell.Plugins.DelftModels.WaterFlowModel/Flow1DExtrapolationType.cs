using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// Flow1D Extrapolation options as defined within the D-Flow1D Technical Reference Manual.
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum Flow1DExtrapolationType
    {
        [ResourcesDescription(typeof(Resources), "Flow1DExtrapolationType_Linear")]
        Linear,
        [ResourcesDescription(typeof(Resources), "Flow1DExtrapolationType_Constant")]
        Constant
    }
}
