using DeltaShell.Plugins.FMSuite.FlowFM.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public static class BoundaryConditionQuantityTypeConverter
    {
        public static MorphologyBoundaryConditionQuantityType
            ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(FlowBoundaryQuantityType type)
        {
            switch (type)
            {

                case FlowBoundaryQuantityType.MorphologyBedLevelPrescribed:
                    return MorphologyBoundaryConditionQuantityType.BedLevelSpecifiedAsFunctionOfTime;
                case FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed:
                    return MorphologyBoundaryConditionQuantityType.BedLevelChangeSpecifiedAsFunctionOfTime;
                case FlowBoundaryQuantityType.MorphologyBedLoadTransport:
                    return MorphologyBoundaryConditionQuantityType.BedLoadTransportRatePrescribed;
                case FlowBoundaryQuantityType.MorphologyBedLevelFixed:
                    return MorphologyBoundaryConditionQuantityType.BedLevelFixed;
                default:
                    return MorphologyBoundaryConditionQuantityType.NoBedLevelConstraint;
            }
        }

        public static FlowBoundaryQuantityType ConvertMorphologyBoundaryConditionQuantityTypeToFlowBoundaryConditionQuantityType(MorphologyBoundaryConditionQuantityType type)
        {
            switch (type)
            {
                case MorphologyBoundaryConditionQuantityType.BedLevelFixed:
                    return FlowBoundaryQuantityType.MorphologyBedLevelFixed;
                case MorphologyBoundaryConditionQuantityType.BedLoadTransportRatePrescribed:
                    return FlowBoundaryQuantityType.MorphologyBedLoadTransport;
                case MorphologyBoundaryConditionQuantityType.BedLevelSpecifiedAsFunctionOfTime:
                    return FlowBoundaryQuantityType.MorphologyBedLevelPrescribed;
                case MorphologyBoundaryConditionQuantityType.BedLevelChangeSpecifiedAsFunctionOfTime:
                    return FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed;
                default:
                    return FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint;
            }
        }
    }
}