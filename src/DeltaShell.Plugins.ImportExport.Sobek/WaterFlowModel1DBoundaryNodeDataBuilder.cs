using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Class responsible for translating SobekBoundary conditions to 'Flow' BoundaryConditions.
    /// </summary>
    public class WaterFlowModel1DBoundaryNodeDataBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DBoundaryNodeDataBuilder));

        public static Model1DBoundaryNodeData ToFlowBoundaryNodeData(SobekFlowBoundaryCondition sobekFlowBoundaryCondition)
        {
            var flowBoundaryCondition = new Model1DBoundaryNodeData();
            if (sobekFlowBoundaryCondition.StorageType == SobekFlowBoundaryStorageType.Constant)
            {
                if (sobekFlowBoundaryCondition.BoundaryType == SobekFlowBoundaryConditionType.Level)
                {
                    flowBoundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                    flowBoundaryCondition.WaterLevel = sobekFlowBoundaryCondition.LevelConstant;
                }
                else
                {
                    flowBoundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowConstant;
                    flowBoundaryCondition.Flow = sobekFlowBoundaryCondition.FlowConstant;
                }
            }
            else if (sobekFlowBoundaryCondition.StorageType == SobekFlowBoundaryStorageType.Variable)
            {
                if (sobekFlowBoundaryCondition.BoundaryType == SobekFlowBoundaryConditionType.Level)
                {
                    flowBoundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
                    DataTableHelper.SetTableToFunction(sobekFlowBoundaryCondition.LevelTimeTable, flowBoundaryCondition.Data);
                }
                else
                {
                    flowBoundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
                    DataTableHelper.SetTableToFunction(sobekFlowBoundaryCondition.FlowTimeTable, flowBoundaryCondition.Data);
                }
            }
            else if (sobekFlowBoundaryCondition.StorageType == SobekFlowBoundaryStorageType.Qh)
            {
                if (SobekFlowBoundaryConditionType.Level == sobekFlowBoundaryCondition.BoundaryType)
                {
                    // Hq boundaries are not supported; TvM just swap columns
                    flowBoundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
                    DataTableHelper.SetTableToFunction(DataTableHelper.SwapColumns(sobekFlowBoundaryCondition.LevelQhTable), flowBoundaryCondition.Data);
                    //set to BoundaryType from level to flow so it will be treated as flow
                    sobekFlowBoundaryCondition.BoundaryType = SobekFlowBoundaryConditionType.Flow;
                }
                else
                {
                    //SobekFlowBoundaryConditionType.Flow == sobekFlowBoundaryCondition.BoundaryType)
                    flowBoundaryCondition.DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
                    DataTableHelper.SetTableToFunction(sobekFlowBoundaryCondition.FlowHqTable, flowBoundaryCondition.Data);
                }
                //Log.WarnFormat("Boundary conditions of QH type not yet supported (id = {0}).", sobekFlowBoundaryCondition.ID);
               // ConvertTableToTimeFunction(sobekFlowBoundaryCondition.FlowTimeTable, flowBoundaryCondition);
            }

            if (flowBoundaryCondition.DataType == Model1DBoundaryNodeDataType.FlowWaterLevelTable ||
                flowBoundaryCondition.DataType == Model1DBoundaryNodeDataType.FlowTimeSeries ||
                flowBoundaryCondition.DataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries)
            {
                flowBoundaryCondition.Data.Arguments[0].InterpolationType = sobekFlowBoundaryCondition.InterpolationType;
            }

            if(sobekFlowBoundaryCondition.ExtrapolationType == ExtrapolationType.Periodic)
            {
                if (sobekFlowBoundaryCondition.StorageType == SobekFlowBoundaryStorageType.Constant)
                {
                    Log.WarnFormat("Cannot apply periodic extrapolation to a constant value (boundary condition {0})", flowBoundaryCondition.Name);
                }
                else
                {
                    TimeSeriesHelper.SetPeriodicExtrapolationSobek(flowBoundaryCondition.Data, sobekFlowBoundaryCondition.ExtrapolationPeriod);
                }
            }

            return flowBoundaryCondition;
        }
    }
}