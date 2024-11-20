using System.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Defines any high-level hydrographic region object. For example hydro network, drainage basin, 2D / 3D waterbody,
    /// groundwater region.
    /// </summary>
    public interface IHydroRegion : IRegion
    {
        /// <summary>
        /// Returns all hydro objects contained in the region.
        /// </summary>
        IEnumerable<IHydroObject> AllHydroObjects { get; }
    }
}