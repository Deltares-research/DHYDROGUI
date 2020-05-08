using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Entity]
    public abstract class BranchFeatureHydroObject : BranchFeature, IHydroObject
    {
        public virtual IHydroRegion Region => (IHydroRegion) Network;

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource => false;

        public virtual bool CanBeLinkTarget => false;

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);

            var hydroObject = (BranchFeatureHydroObject) source;
            Links = new EventedList<HydroLink>(hydroObject.Links);
        }

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
    }
}