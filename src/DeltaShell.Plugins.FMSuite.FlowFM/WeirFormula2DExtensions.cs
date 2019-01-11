using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;


namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    /// <summary>
    /// WeirFormula2DExtensions contains extension for WeirFormulas that relate
    /// to FM.
    /// </summary>
    public static class WeirFormula2DExtensions
    {
        /// <summary>
        /// Get the name of this Formula within a D-FlowFM context.
        /// </summary>
        /// <param name="formula">The formula.</param>
        /// <returns>
        /// The name of the specified formula.
        /// </returns>
        /// <remarks>
        /// Currently supports GatedWeirFormula, GeneralStructure and SimpleWeir,
        /// for any other formula, Simple Weir will be returned.
        /// </remarks>
        public static string GetName2D(this IWeirFormula formula)
        {
            if (formula is GatedWeirFormula)
                return "Simple gate";
            if (formula is GeneralStructureWeirFormula)
                return "General structure";
            return "Simple weir";
        }
    }
}
