using System;
using DelftTools.Hydro.Area.Objects.StructuresObjects.StructureFormulas;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formula"/> is null.</exception>
        /// <exception cref="NotImplementedException">Thrown when <paramref name="formula"/> is not of a
        /// type supported within FM.</exception>
        public static string GetName2D(this IStructureFormula formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException(nameof(formula));
            }

            if (formula is SimpleGateFormula)
            {
                return "Simple gate";
            }

            if (formula is GeneralStructureFormula)
            {
                return "General structure";
            }

            if (formula is SimpleWeirFormula)
            {
                return "Simple weir";
            }

            throw new NotImplementedException(
                $"Formula of type <{formula.GetType().FullName}> is not supported within FM.");
        }
    }
}