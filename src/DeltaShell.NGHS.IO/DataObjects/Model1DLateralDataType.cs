using System.ComponentModel;
using DelftTools.Utils;

namespace DeltaShell.NGHS.IO.DataObjects
{

    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum Model1DLateralDataType
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