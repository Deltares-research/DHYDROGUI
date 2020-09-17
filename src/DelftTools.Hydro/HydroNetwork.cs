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
    /// <summary>
    /// Hydrographic network (channels, pipes)
    /// </summary>
    [DisplayName("Hydro Network")]
    [Entity]
    public partial class HydroNetwork : Network, IHydroNetwork
    {
        public override IEventedList<IBranch> Branches { get; set; }

        public override IEventedList<INode> Nodes { get; set; }

        public virtual IEnumerable<IChannel> Channels { get; protected set; }

        public virtual IEventedList<IRegion> SubRegions { get; set; }

        public virtual IEnumerable<IRegion> AllRegions => HydroRegion.GetAllRegions(this);

        [Aggregation]
        public virtual IRegion Parent { get; set; }

        public virtual IEnumerable<IHydroObject> AllHydroObjects { get; }

        public virtual IEventedList<HydroLink> Links { get; set; }

        public new virtual bool EditWasCancelled { get; }

        public override string ToString()
        {
            return Name;
        }

        public virtual IEnumerable<object> GetDirectChildren()
        {
            yield break;
        }

        public override object Clone()
        {
            return (HydroNetwork) base.Clone();
        }

        public virtual HydroLink AddNewLink(IHydroObject source, IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveLink(IHydroObject source, IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanLinkTo(IHydroObject source, IHydroObject target)
        {
            return false;
        }
    }
}