using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum MorphologyBoundaryConditionQuantityType
    {
        [Category("Morphology")]
        [Description("Bed level unconstrained")]
        NoBedLevelConstraint = 0,
        [Category("Morphology")]
        [Description("Bed level fixed")]
        BedLevelFixed = 1,
        [Category("Morphology")]
        [Description("Bed load transport (incl. pores)")]
        BedLoadTransportRatePrescribed = 4,
        [Category("Morphology")]
        [Description("Bed level prescribed")]
        BedLevelSpecifiedAsFunctionOfTime = 6,
        [Category("Morphology")]
        [Description("Bed level change prescribed")]
        BedLevelChangeSpecifiedAsFunctionOfTime = 7
    }
}