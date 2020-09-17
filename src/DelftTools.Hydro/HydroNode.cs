using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro
{
    [Entity]
    public class HydroNode : Node, IHydroNode
    {
        public HydroNode() : this("hydro node") {}

        public HydroNode(string name) : base(name) {}

        public virtual IHydroNetwork HydroNetwork => throw new NotImplementedException();

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        [Aggregation]
        public override IEventedList<IBranch> IncomingBranches { get; set; }

        [Aggregation]
        public override IEventedList<IBranch> OutgoingBranches { get; set; }

        public virtual IHydroRegion Region { get; }

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource => throw new NotImplementedException();

        public virtual bool CanBeLinkTarget => throw new NotImplementedException();

        /// <summary>
        /// Returns the features of the node (as objects)
        /// </summary>
        public virtual IEnumerable<object> GetDirectChildren()
        {
            throw new NotImplementedException();
        }

        public virtual HydroLink LinkTo(IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual void UnlinkFrom(IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanLinkTo(IHydroObject target)
        {
            throw new NotImplementedException();
        }
    }
}