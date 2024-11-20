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

        /// Gate height
        /// <summary>
        /// The gate height value
        /// </summary>
        double GateHeight { get; set; }

        /// Gate opening horizontal direction 
        /// <summary>
        /// The direction in which the gate will open.
        /// Left and right are defined by the flow direction of the gate,
        /// indicated in the gui by a small arrow.
        /// </summary>
        GateOpeningDirection GateOpeningHorizontalDirection { get; set; }

        // Horizontal gate opening width
        /// <summary>
        /// The gate horizontal opening width 
        /// </summary>
        double HorizontalGateOpeningWidth { get; set; }

        // Use horizontal gate opening width
        /// <summary>
        /// Flag to enable the use of the horizontal gate opening 
        /// </summary>
        bool UseHorizontalGateOpeningWidthTimeSeries { get; set; }

        // Horizontal gate opening width Time Series (T)
        /// <summary>
        /// The Time series for the Horizontal gate opening width
        /// </summary>
        TimeSeries HorizontalGateOpeningWidthTimeSeries { get; set; }

        /// Gate lower edge level
        /// <summary>
        /// The gate lower edge level value
        /// </summary>
        double GateLowerEdgeLevel { get; set; }

        /// Use gate lower edge level time series
        /// <summary>
        /// Flag to set the use of the Time series for the gate lower edge level
        /// </summary>
        bool UseGateLowerEdgeLevelTimeSeries { get; set; }

        /// Gate lower edge level Time series
        /// <summary>
        /// Value of the time series for the gate lower level edge
        /// </summary>
        TimeSeries GateLowerEdgeLevelTimeSeries { get; set; }
    }
}