using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class FlowBoundaryConditionFactory: BoundaryConditionFactory
    {
        public override bool SupportsMultipleConditionsPerSet
        {
            get { return true; }
        }

        public WaterFlowFMModel Model { set; private get; }

        public override IBoundaryCondition CreateBoundaryCondition(Feature2D feature, string variable, BoundaryConditionDataType dataType)
        {
            FlowBoundaryQuantityType flowBoundaryQuantityType;

            if (variable != FlowBoundaryQuantityType.Tracer.ToString() && Enum.TryParse(variable, out flowBoundaryQuantityType))
            {
                return CreateBoundaryCondition(feature, flowBoundaryQuantityType, dataType, null);
            }
            // parse the tracer name and the quantity type of the boundary condition
            string tracerName = null;
            if (Model != null && Model.TracerDefinitions.Contains(variable))
            {
                tracerName = variable;
            }

            if(tracerName != null)
            {
                return CreateBoundaryCondition(feature, FlowBoundaryQuantityType.Tracer, dataType, tracerName);
            }
            return null;
        }

        private static IBoundaryCondition CreateBoundaryCondition(Feature2D feature, 
                                                                  FlowBoundaryQuantityType flowBoundaryQuantityType,
                                                                  BoundaryConditionDataType dataType,
                                                                  string tracerName)
        {
            var result = new FlowBoundaryCondition(flowBoundaryQuantityType, dataType) {Feature = feature, TracerName = tracerName};

            result.Name = feature.Name + "-" + result.VariableDescription;

            if (result.IsHorizontallyUniform)
            {
                result.AddPoint(0);
            }

            return result;
        }

        public static IBoundaryCondition CreateBoundaryCondition(Feature2D feature2D)
        {
            return CreateBoundaryCondition(feature2D, FlowBoundaryQuantityType.WaterLevel,
                                           BoundaryConditionDataType.TimeSeries, null);
        }
    }
}
