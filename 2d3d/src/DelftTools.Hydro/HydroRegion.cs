using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [DisplayName("Region")]
    [Entity]
    public class HydroRegion : RegionBase, IHydroRegion
    {
        public override IEventedList<IRegion> SubRegions
        {
            get => base.SubRegions;
            set
            {
                if (base.SubRegions != null)
                {
                    base.SubRegions.CollectionChanging -= OnSubRegionsCollectionChanging;
                }

                base.SubRegions = value;

                if (base.SubRegions != null)
                {
                    foreach (IRegion subregion in base.SubRegions)
                    {
                        subregion.Parent = this;
                    }

                    base.SubRegions.CollectionChanging += OnSubRegionsCollectionChanging;
                }
            }
        }

        public virtual IEnumerable<IHydroObject> AllHydroObjects
        {
            get
            {
                return SubRegions.OfType<IHydroRegion>().SelectMany(r => r.AllHydroObjects);
            }
        }

        public override object Clone()
        {
            var clone = (HydroRegion) base.Clone();
            clone.Name = Name;
            clone.Geometry = (IGeometry) Geometry?.Clone();
            clone.Attributes = (IFeatureAttributeCollection) Attributes?.Clone();

            return clone;
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            return SubRegions;
        }

        private void OnSubRegionsCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (!Equals(sender, SubRegions))
            {
                return;
            }

            var subRegion = (IHydroRegion) e.Item;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    subRegion.Parent = this;
                    break;

                case NotifyCollectionChangeAction.Remove:
                    break;
            }
        }
    }
}