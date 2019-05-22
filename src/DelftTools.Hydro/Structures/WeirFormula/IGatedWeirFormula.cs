using DelftTools.Functions;

namespace DelftTools.Hydro.Structures.WeirFormula
{
    /// <summary>
    /// Weirformula supporting gate..can be gated or general structure for now..
    /// </summary>
    public interface IGatedWeirFormula : IWeirFormula
    {
        double GateOpening { get; set; }

        // Gate.Door height (T)
        double DoorHeight { get; set; }

        // Gate.Door opening width (T)
        /// <summary>
        /// The direction in which the door will open.
        /// Left and right are defined by the flow direction of the gate, 
        /// indicated in the gui by a small arrow.
        /// </summary>
        GateOpeningDirection HorizontalDoorOpeningDirection { get; set; }

        double HorizontalDoorOpeningWidth { get; set; }
        bool UseHorizontalDoorOpeningWidthTimeSeries { get; set; }
        TimeSeries HorizontalDoorOpeningWidthTimeSeries { get; set; }

        // Gate.Lower Edge Level(T)
        double LowerEdgeLevel { get; set; }
        bool UseLowerEdgeLevelTimeSeries { get; set; }
        TimeSeries LowerEdgeLevelTimeSeries { get; set; }
    }
}