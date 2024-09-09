using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro
{
    public interface IDrainageBasin : IHydroRegion
    {
        /// <summary>
        /// Gets all catchments.
        /// </summary>
        IEventedList<Catchment> Catchments { get; }

        /// <summary>
        /// Gets all waste water treatment plants.
        /// </summary>
        IEventedList<WasteWaterTreatmentPlant> WasteWaterTreatmentPlants { get; }
        
        /// <summary>
        /// Gets all catchments.
        /// </summary>
        IEnumerable<Catchment> AllCatchments { get; }
        
        /// <summary>
        /// Gets all runoff boundaries.
        /// </summary>
        IEventedList<RunoffBoundary> Boundaries { get; }

        /// <summary>
        /// Gets all catchment types.
        /// </summary>
        IEventedList<CatchmentType> CatchmentTypes { get; }
    }
}