using System;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public class WasteWaterTreatmentPlant : Feature, IHydroObject, IComparable, ILongNameable
    {
        [FeatureAttribute]
        public virtual string Description { get; set; }

        [Aggregation]
        public virtual DrainageBasin Basin { get; set; }

        [DisplayName("Name")]
        [FeatureAttribute]
        public virtual string Name { get; set; }

        public virtual IHydroRegion Region => Basin;

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource => true;

        public virtual bool CanBeLinkTarget => true;

        [DisplayName("LongName")]
        [FeatureAttribute]
        public virtual string LongName { get; set; }

        public virtual int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public virtual object Clone()
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