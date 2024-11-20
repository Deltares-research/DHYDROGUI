using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Utils;

namespace DelftTools.Hydro.Structures
{
    [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
    public enum GateOpeningDirection
    {
        [Description("Symmetric")]
        Symmetric,
        [Description("From left")]
        FromLeft,
        [Description("From right")]
        FromRight,
    }

    public interface IGate : IStructure1D
    {
        /// <summary>
        /// The base level of the gate. Same as crest level, but it cannot have a time series.
        /// </summary>
        double SillLevel { get; set; }
        bool UseSillLevelTimeSeries { get; set; }
        TimeSeries SillLevelTimeSeries { get; set; }
        /// <summary>
        /// The opening height of the gate.
        /// LowerEdgeLevel - SillLevel = OpeningHeight
        /// </summary>
        double LowerEdgeLevel { get; set; }
        bool UseLowerEdgeLevelTimeSeries { get; set; }
        TimeSeries LowerEdgeLevelTimeSeries { get; set; }

        /// <summary>
        /// Vertical opening of the gate.
        /// </summary>
        double OpeningWidth { get; set; }
        bool UseOpeningWidthTimeSeries { get; set; }
        TimeSeries OpeningWidthTimeSeries { get; set; }

        /// <summary>
        /// The actual height of the gate door.
        /// </summary>
        double DoorHeight { get; set; }

        /// <summary>
        /// The direction in which the door will open.
        /// Left and right are defined by the flow direction of the gate, 
        /// indicated in the gui by a small arrow.
        /// </summary>
        GateOpeningDirection HorizontalOpeningDirection { get; set; }

        /// <summary>
        /// Actual width of the gate.
        /// </summary>
        double SillWidth { get; set; }
    }
}