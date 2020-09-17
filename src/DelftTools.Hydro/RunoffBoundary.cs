using System;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public class RunoffBoundary : Feature, IHydroObject, IComparable, ILongNameable
    {
        [Aggregation]
        public virtual DrainageBasin Basin { get; set; }

        [FeatureAttribute]
        public virtual string Description { get; set; }

        [DisplayName("Name")]
        [FeatureAttribute]
        public virtual string Name { get; set; }

        public virtual IHydroRegion Region => throw new NotImplementedException();

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource => false;

        public virtual bool CanBeLinkTarget => true;

        [DisplayName("Long name")]
        [FeatureAttribute]
        public virtual string LongName { get; set; }

        public virtual int CompareTo(object obj)
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