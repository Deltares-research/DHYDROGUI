using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    /// <summary>
    /// Flow1D Interpolation options as defined within the D-Flow1D Technical Reference Manual.
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum Flow1DInterpolationType
    {
        [ResourcesDescription(typeof(Resources), "Flow1DInterpolationType_Linear")]
        Linear,
        [ResourcesDescription(typeof(Resources), "Flow1DInterpolationType_BlockTo")]
        BlockTo,
        [ResourcesDescription(typeof(Resources), "Flow1DInterpolationType_BlockFrom")]
        BlockFrom
    }
}
