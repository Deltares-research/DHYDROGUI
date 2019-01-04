using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public class WaveBoundaryConditionFactory : BoundaryConditionFactory
    {
        public override bool SupportsMultipleConditionsPerSet
        {
            get { return false; }
        }

        public override IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D, string quantity, BoundaryConditionDataType dataType, string quantityType = null)
        {
            var bc = new WaveBoundaryCondition(dataType)
                {
                    Feature = feature2D,
                    Name = feature2D.Name,
                    SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.Uniform
                };

            if (dataType == BoundaryConditionDataType.SpectrumFromFile)
            {
                bc.AddPoint(0);
                bc.SpectrumFiles[0] = "";
            }
            if (dataType == BoundaryConditionDataType.ParameterizedSpectrumConstant)
            {
                bc.AddPoint(0);
                bc.SpectrumParameters[0] = new WaveBoundaryParameters();
            }
            return bc;
        }
    }
}
