using System;
using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [Entity]
    public class WasteWaterTreatmentPlant : Feature, IHydroObject, IComparable, ILongNameable
    {
        public WasteWaterTreatmentPlant()
        {
            Name = "WWTP";
            Attributes = new DictionaryFeatureAttributeCollection();
            Links = new EventedList<HydroLink>();
        }

        [FeatureAttribute]
        public virtual string Description { get; set; }

        [Aggregation]
        public virtual DrainageBasin Basin { get; set; }

        // override: this feature needs to bubble geometry changes
        public override IGeometry Geometry { get; set; }

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

        public static WasteWaterTreatmentPlant CreateDefault()
        {
            return new WasteWaterTreatmentPlant {Geometry = new Point(0, 0)};
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual int CompareTo(object obj)
        {
            var other = obj as WasteWaterTreatmentPlant;
            if (other != null)
            {
                if (Equals(this, other))
                {
                    return 0;
                }

                foreach (WasteWaterTreatmentPlant c in Basin.WasteWaterTreatmentPlants)
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
            else if (obj is RunoffBoundary)
            {
                return -1;
            }

            throw new InvalidOperationException();
        }

        public virtual object Clone()
        {
            var wwtp = new WasteWaterTreatmentPlant();
            wwtp.Geometry = Geometry;
            wwtp.Description = Description;
            wwtp.LongName = LongName;
            wwtp.Name = Name;
            wwtp.Basin = Basin;
            wwtp.Attributes = (IFeatureAttributeCollection) Attributes.Clone();
            wwtp.Links = new EventedList<HydroLink>(Links);

            return wwtp;
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