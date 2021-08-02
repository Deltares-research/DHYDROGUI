namespace DelftTools.Hydro.Structures.WeirFormula
{
    /// <summary>
    /// Weirformula supporting gate..can be gated or general structure for now..
    /// </summary>
    public interface IGatedWeirFormula : IWeirFormula
    {
        double GateOpening { get; set; }

        double GateHeight { get; set; }

        double LowerEdgeLevel { get; set; }
    }
}