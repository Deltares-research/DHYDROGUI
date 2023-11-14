using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Defines any high-level hydrographic region object. For example hydro network, drainage basin, 2D / 3D waterbody, groundwater region.
    /// </summary>
    public interface IHydroRegion : IRegion
    {
        /// <summary>
        /// Returns all hydro objects contained in the region.
        /// </summary>
        IEnumerable<IHydroObject> AllHydroObjects { get; }

        /// <summary>
        /// All links between hydro objects in this region, or its sub-regions.
        /// </summary>
        IEventedList<HydroLink> Links { get; }

        /// <summary>
        /// Add a new link between between <paramref name="source"/> and <paramref name="target"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        HydroLink AddNewLink(IHydroObject source, IHydroObject target);

        /// <summary>
        /// Remove link from this hydro region and from the <paramref name="source"/> and <paramref name="target"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void RemoveLink(IHydroObject source, IHydroObject target);

        /// <summary>
        /// Checks if two hydro objects can be linked.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool CanLinkTo(IHydroObject source, IHydroObject target);
    }
}