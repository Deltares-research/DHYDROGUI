using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Helpers
{
    public interface IConveyanceCalculator
    {
        /// <summary>
        /// Gives a function containing the Conveyance data from CrossSection geometry and the friction
        /// </summary>
        /// <param name="crossSection">crossSection to use as base</param>
        IFunction GetConveyance(ICrossSection crossSection);
    }
}