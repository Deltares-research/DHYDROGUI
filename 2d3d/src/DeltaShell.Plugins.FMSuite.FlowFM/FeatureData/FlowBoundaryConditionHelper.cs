using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public static class FlowBoundaryConditionHelper
    {
        internal static bool IsBoundaryCondition(IBoundaryCondition boundaryCondition)
        {
            var flowBC = boundaryCondition as FlowBoundaryCondition;
            if (flowBC == null)
            {
                return false;
            }

            return IsBoundaryConditionFlowQuantityType(flowBC.FlowQuantity);
        }

        private static bool IsBoundaryConditionFlowQuantityType(FlowBoundaryQuantityType flowQuantity)
        {
            return flowQuantity == FlowBoundaryQuantityType.WaterLevel ||
                   flowQuantity == FlowBoundaryQuantityType.Discharge ||
                   flowQuantity == FlowBoundaryQuantityType.Neumann ||
                   flowQuantity == FlowBoundaryQuantityType.Riemann ||
                   flowQuantity == FlowBoundaryQuantityType.RiemannVelocity ||
                   flowQuantity == FlowBoundaryQuantityType.NormalVelocity ||
                   flowQuantity == FlowBoundaryQuantityType.Salinity ||
                   flowQuantity == FlowBoundaryQuantityType.TangentVelocity ||
                   flowQuantity == FlowBoundaryQuantityType.Velocity;
        }
    }
}