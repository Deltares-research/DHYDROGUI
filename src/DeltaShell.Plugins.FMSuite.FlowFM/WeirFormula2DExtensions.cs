using System;
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>
        /// The name of the specified formula.
        /// </returns>
        /// <exception cref="NotImplementedException"> formula is not of a type supported within FM. </exception>
        public static string GetName2D(this IWeirFormula formula)
        {
            if (formula == null) 
                throw new ArgumentNullException("Formula cannot be null.");

            if (formula is GatedWeirFormula)
                return "Simple gate";
            if (formula is GeneralStructureWeirFormula)
                return "General structure";
            if (formula is SimpleWeirFormula)
                return "Simple weir";

            throw new NotImplementedException($"Formula of type <{formula.GetType().FullName}> is not supported within FM.");
        }
    }
}
