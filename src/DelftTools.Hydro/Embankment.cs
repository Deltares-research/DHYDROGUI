using DelftTools.Utils.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    public class Embankment : Feature2D, IHydroObject
    {
        public Embankment()
        {
            Links = new EventedList<HydroLink>();
        }

        #region IHydroObject

        public virtual IHydroRegion Region
        {
            get;
            set;
        }
        
        public virtual IEventedList<HydroLink> Links { get; set; }
        
        public virtual bool CanBeLinkSource { get { return true; } }
        
        public virtual bool CanBeLinkTarget { get { return false; } }
        public virtual Coordinate LinkingCoordinate => Geometry?.Coordinate;

        public virtual HydroLink LinkTo(IHydroObject target)
        {
            return Region.AddNewLink(this, target);
        }

        public virtual void UnlinkFrom(IHydroObject target)
        {
            Region.RemoveLink(this, target);
        }

        public virtual bool CanLinkTo(IHydroObject target)
        {
            return Region.CanLinkTo(this, target);
        }

        #endregion
    }
}