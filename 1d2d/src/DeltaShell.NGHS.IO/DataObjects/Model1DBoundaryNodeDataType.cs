using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.NGHS.IO.DataObjects
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum Model1DBoundaryNodeDataType
    {
        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        None,

        /// <summary>
        /// H(t) function
        /// </summary>
        [Description("H(t) : Water level time series")]
        WaterLevelTimeSeries,

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
        FlowConstant,

        /// <summary>
        /// H
        /// </summary>
        [Description("H : Constant water level")]
        WaterLevelConstant
   }
}