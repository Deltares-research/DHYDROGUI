using DelftTools.Functions;

namespace DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas
{
    /// <summary>
    /// <see cref="IGatedStructureFormula"/> defines the common properties of
    /// gated structure, e.g. Simple Gates and General Structures
    /// </summary>
    public interface IGatedStructureFormula : IStructureFormula
    {
        /// Gate opening
        /// <summary>
        /// The Gate opening value
        /// </summary>
        double GateOpening { get; set; }

        /// Door height
        /// <summary>
        /// The Door height value
        /// </summary>
        double DoorHeight { get; set; }

        /// Gate.Door opening width 
        /// <summary>
        /// The direction in which the door will open.
        /// Left and right are defined by the flow direction of the gate,
        /// indicated in the gui by a small arrow.
        /// </summary>
        GateOpeningDirection HorizontalDoorOpeningDirection { get; set; }

        // Horizontal opening width
        /// <summary>
        /// The Door Horizontal opening width 
        /// </summary>
        double HorizontalDoorOpeningWidth { get; set; }

        // Use Horizontal opening width
        /// <summary>
        /// Flag to enable the use of the Horizontal door opening 
        /// </summary>
        bool UseHorizontalDoorOpeningWidthTimeSeries { get; set; }

        // Horizontal door opening width Time Series (T)
        /// <summary>
        /// The Time series for the Horizontal door opening width
        /// </summary>
        TimeSeries HorizontalDoorOpeningWidthTimeSeries { get; set; }

        /// Lower edge level
        /// <summary>
        /// The lower edge level value
        /// </summary>
        double LowerEdgeLevel { get; set; }

        /// Use lower edge level time series
        /// <summary>
        /// Flag to set the use of the Time series for the lower edge level
        /// </summary>
        bool UseLowerEdgeLevelTimeSeries { get; set; }

        /// Lower edge level Time series
        /// <summary>
        /// Value of the time series for the lower level edge
        /// </summary>
        TimeSeries LowerEdgeLevelTimeSeries { get; set; }
    }
}