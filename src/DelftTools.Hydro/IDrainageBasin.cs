using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;

namespace DelftTools.Hydro
{
    public interface IDrainageBasin : IHydroRegion
    {
        IEventedList<Catchment> Catchments { get; }
        IEventedList<WasteWaterTreatmentPlant> WasteWaterTreatmentPlants { get; }
        IEnumerable<Catchment> AllCatchments { get; }
        IEventedList<RunoffBoundary> Boundaries { get; }
        IEventedList<CatchmentType> CatchmentTypes { get; }
    }
}