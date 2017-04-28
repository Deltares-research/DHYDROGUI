using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class BoundaryFileWriterHelper
    {
        public static string GetFunctionString(WaterFlowModel1DBoundaryNodeDataType boundaryNodeDataType)
        {
            switch (boundaryNodeDataType)
            {
                case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    return BoundaryRegion.FunctionStrings.Constant;
                case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                    return BoundaryRegion.FunctionStrings.QhTable;
                default:
                    return string.Empty;
            }
        }

        public static string GetFunctionString(WaterFlowModel1DLateralDataType lateralSourceDataType)
        {
            switch (lateralSourceDataType)
            {
                case WaterFlowModel1DLateralDataType.FlowConstant:
                    return BoundaryRegion.FunctionStrings.Constant;
                case WaterFlowModel1DLateralDataType.FlowTimeSeries:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                case WaterFlowModel1DLateralDataType.FlowWaterLevelTable:
                    return BoundaryRegion.FunctionStrings.QhTable;
                default:
                    return string.Empty;
            }
        }

        public static string GetFunctionString(SaltBoundaryConditionType saltBoundaryConditionType)
        {
            switch (saltBoundaryConditionType)
            {
                case SaltBoundaryConditionType.Constant:
                    return BoundaryRegion.FunctionStrings.Constant;
                case SaltBoundaryConditionType.TimeDependent:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                default:
                    return string.Empty;
            }
        }

        public static string GetFunctionString(SaltLateralDischargeType saltLateralDischargeType)
        {
            switch (saltLateralDischargeType)
            {
                case SaltLateralDischargeType.ConcentrationConstant:
                case SaltLateralDischargeType.MassConstant:
                case SaltLateralDischargeType.Default:
                    return BoundaryRegion.FunctionStrings.Constant;
                case SaltLateralDischargeType.ConcentrationTimeSeries:
                case SaltLateralDischargeType.MassTimeSeries:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                default:
                    return string.Empty;
            }
        }

        public static string GetFunctionString(TemperatureBoundaryConditionType temperatureBoundaryConditionType)
        {
            switch (temperatureBoundaryConditionType)
            {
                case TemperatureBoundaryConditionType.Constant:
                    return BoundaryRegion.FunctionStrings.Constant;
                case TemperatureBoundaryConditionType.TimeDependent:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                default:
                    return string.Empty;
            }
        }

        public static string GetFunctionString(TemperatureLateralDischargeType temperatureLateralDischargeType)
        {
            switch (temperatureLateralDischargeType)
            {
                case TemperatureLateralDischargeType.Constant:
                    return BoundaryRegion.FunctionStrings.Constant;
                case TemperatureLateralDischargeType.TimeDependent:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                default:
                    return string.Empty;
            }
        }

    }
}
