using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public abstract class BoundaryConditionFactory
    {
        public abstract bool SupportsMultipleConditionsPerSet { get; }

        public abstract IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D,
                                                                   string quantity,
                                                                   BoundaryConditionDataType dataType,
                                                                   string quantityType = null);
    }
}