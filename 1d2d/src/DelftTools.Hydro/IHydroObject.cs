using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Hydro object is any object contained in the <see cref="IHydroRegion"/>.
    /// Some hydro object may be linked to each other using <see cref="HydroLink"/>. 
    /// The meaning of the link between two hydro objects in most cases is that it allows flow of water or other substance througth them.
    /// Hydro objects can be linked to the hydro object of the same region or to the hydro objects of another region (only in the case if these hydro regions have common parent).
    /// Note, that links are always stored (composed) in the parent hydro region, a copy of the links connected to the current hydro object are also stored locally (aggregation).
    /// </summary>
    public interface IHydroObject : IFeature, INameable
    {
        IHydroRegion Region { get; }

        [Aggregation]
        IEventedList<HydroLink> Links { get; set; }

        bool CanBeLinkSource { get; }

        bool CanBeLinkTarget { get; }
        
        /// <summary>
        /// Gets the coordinate for linking a <see cref="HydroLink"/>.
        /// Can return <c>null</c> if the <see cref="IHydroObject.Geometry"/> of this instance is <c>null</c>.
        /// </summary>
        Coordinate LinkingCoordinate { get; }

        HydroLink LinkTo(IHydroObject target);

        void UnlinkFrom(IHydroObject target);
        
        bool CanLinkTo(IHydroObject target);
    }
}