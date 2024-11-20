using System;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public class RunoffBoundary : Feature, IHydroObject, IComparable
    {
        public RunoffBoundary()
        {
            Name = "RunoffBoundary";
            Links = new EventedList<HydroLink>();
            Attributes = new DictionaryFeatureAttributeCollection();
        }
        
        [DisplayName("Name")]
        [FeatureAttribute]
        public virtual string Name { get; set; }

        [Aggregation]
        public virtual IDrainageBasin Basin { get; set; }

        public virtual IHydroRegion Region { get { return Basin; } }
        
        [Aggregation]
        public virtual IEventedList<HydroLink> Links { get; set; }

        public virtual bool CanBeLinkSource
        {
            get { return false; }
        }

        public virtual bool CanBeLinkTarget
        {
            get { return true; }
        }

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

        public virtual int CompareTo(object obj)
        {
            var other = obj as RunoffBoundary;
            if (other != null)
            {
                if (Equals(this, other))
                    return 0;

                foreach (var c in Basin.Boundaries)
                {
                    if (Equals(c, this))
                    {
                        return -1;
                    }
                    if (Equals(c, other))
                    {
                        return 1;
                    }
                }
            }
            else if (obj is Catchment)
            {
                return 1;
            }
            else if (obj is WasteWaterTreatmentPlant)
            {
                return 1;
            }
            throw new InvalidOperationException();
        }
        
        [DisplayName("Long name")]
        [FeatureAttribute]
        public virtual string LongName { get; set; }

        [FeatureAttribute]
        public virtual string Description { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override object Clone()
        {
            var boundary = new RunoffBoundary();
            boundary.Geometry = Geometry;
            boundary.Name = Name;
            boundary.Description = Description;
            boundary.LongName = LongName;
            boundary.Basin = Basin;
            boundary.Attributes = (IFeatureAttributeCollection)Attributes.Clone();
            boundary.Links = new EventedList<HydroLink>(Links);
            return boundary;
        }
    }
}