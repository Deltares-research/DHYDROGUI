using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum VerticalInterpolationType
    {
        Uniform,
        Step,
        Linear,
        Logarithmic,
    }
}