using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects
{
    public enum WaterFlowModel1DLateralDataType
    {
        /// <summary>
        /// Q(t) function
        /// </summary>
        [Description("Q(t) : Flow time series")]
        FlowTimeSeries,

        /// <summary>
        /// Q(h) 
        /// </summary>
        [Description("Q(h) : Flow water level table")]
        FlowWaterLevelTable,

        /// <summary>
        /// Q 
        /// </summary>
        [Description("Q : Constant flow")]
        FlowConstant
    }
}