using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public class WaveBoundaryConditionFactory : BoundaryConditionFactory
    {
        public override bool SupportsMultipleConditionsPerSet => false;

        public override IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D, string quantity,
                                                                   BoundaryConditionDataType dataType,
                                                                   string quantityType = null)
        {
            var bc = new WaveBoundaryCondition(dataType)
            {
                Feature = feature2D,
                Name = feature2D.Name,
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform
            };

            switch (dataType)
            {
                case BoundaryConditionDataType.SpectrumFromFile:
                case BoundaryConditionDataType.ParameterizedSpectrumConstant:
                    bc.AddPoint(0);
                    break;
            }

            return bc;
        }
    }
}