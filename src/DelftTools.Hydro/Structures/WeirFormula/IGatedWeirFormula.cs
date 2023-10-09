namespace DelftTools.Hydro.Structures.WeirFormula
{
    /// <summary>
    /// Weir formula supporting a gate.
    /// </summary>
    public interface IGatedWeirFormula : IWeirFormula
    {
        /// <summary>
        /// Gets or sets the gate opening width between gate doors.
        /// </summary>
        double GateOpening { get; set; }

        /// <summary>
        /// Gets or sets the height of the gate door.
        /// </summary>
        double GateHeight { get; set; }

        /// <summary>
        /// Gets or sets the position of the gate door’s lower edge.
        /// </summary>
        double LowerEdgeLevel { get; set; }
    }
}