using System;
using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public class Catchment : Feature, ICopyFrom, IHydroObject, IComparable, INameable
    {
        public virtual IEventedList<Catchment> SubCatchments { get; set; }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 3)]
        public virtual string LongName { get; set; }

        [Aggregation]
        [DisplayName("Type")]
        [FeatureAttribute]
        [ReadOnly(true)]
        public virtual CatchmentType CatchmentType { get; set; }

        [FeatureAttribute(Order = 2)]
        public virtual string Description { get; set; }

        public virtual bool IsGeometryDerivedFromAreaSize { get; set; }

        public virtual IPoint InteriorPoint => throw new NotImplementedException();
        public virtual double AreaSize => throw new NotImplementedException();

        [Aggregation]
        public virtual IDrainageBasin Basin { get; set; }

        [DisplayName("Name")]
        [FeatureAttribute(Order = 1)]
        public virtual string Name { get; set; }

        [Aggregation]
        public virtual IHydroRegion Region => throw new NotImplementedException();

        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource => throw new NotImplementedException();

        public virtual bool CanBeLinkTarget => throw new NotImplementedException();

        public virtual void SetAreaSize(double area) {}

        public static Catchment CreateDefault()
        {
            throw new NotImplementedException();
        }

        public virtual void AddSubCatchment(CatchmentType catchmentType) {}

        public virtual int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public virtual void CopyFrom(object source) {}

        public virtual HydroLink LinkTo(IHydroObject target)
        {
            throw new NotImplementedException();
        }

        public virtual void UnlinkFrom(IHydroObject target) {}

        public virtual bool CanLinkTo(IHydroObject target)
        {
            throw new NotImplementedException();
        }
    }
}