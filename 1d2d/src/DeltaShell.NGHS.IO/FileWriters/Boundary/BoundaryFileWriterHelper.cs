using DeltaShell.NGHS.IO.DataObjects;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public static class BoundaryFileWriterHelper
    {
        public static string GetFunctionString(Model1DBoundaryNodeDataType boundaryNodeDataType)
        {
            switch (boundaryNodeDataType)
            {
                case Model1DBoundaryNodeDataType.FlowConstant:
                case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    return BoundaryRegion.FunctionStrings.Constant;
                case Model1DBoundaryNodeDataType.FlowTimeSeries:
                case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    return BoundaryRegion.FunctionStrings.QhTable;
                default:
                    return string.Empty;
            }
        }

        public static string GetFunctionString(Model1DLateralDataType lateralSourceDataType)
        {
            switch (lateralSourceDataType)
            {
                case Model1DLateralDataType.FlowConstant:
                    return BoundaryRegion.FunctionStrings.Constant;
                case Model1DLateralDataType.FlowTimeSeries:
                    return BoundaryRegion.FunctionStrings.TimeSeries;
                case Model1DLateralDataType.FlowWaterLevelTable:
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
