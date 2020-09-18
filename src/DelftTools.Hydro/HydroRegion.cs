using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils;
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
        private bool isCloning;

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

        public static IHydroRegion GetCommonRegion(IHydroObject source, IHydroObject target)
        {
            if (Equals(source.Region, target.Region))
            {
                return source.Region;
            }

            var sourceParent = source.Region.Parent as IHydroRegion;
            var targetParent = target.Region.Parent as IHydroRegion;

            return Equals(sourceParent, targetParent) ? sourceParent : null;
        }

        /// <summary>
        /// Adds links to the <paramref name="clone"/> region based on the links of <paramref name="original"/> region.
        /// </summary>
        public static void CloneAndAddLinks(IHydroRegion original, IHydroRegion clone)
        {
            List<IHydroObject> originalObjects = original.AllHydroObjects.ToList();
            List<IHydroObject> clonedObjects = clone.AllHydroObjects.ToList();

            //TODO D3DFMIQ-2083
            //foreach (HydroLink link in original.Links)
            //{
            //    var linkClone = (HydroLink) link.Clone();

            //    // repair link
            //    linkClone.Source = clonedObjects[originalObjects.IndexOf(link.Source)];
            //    linkClone.Target = clonedObjects[originalObjects.IndexOf(link.Target)];

            //    // replace link in source and target objects
            //    int linkInSourceIndex = linkClone.Source.Links.IndexOf(link);
            //    linkClone.Source.Links[linkInSourceIndex] = linkClone;

            //    int linkInTargetIndex = linkClone.Target.Links.IndexOf(link);
            //    linkClone.Target.Links[linkInTargetIndex] = linkClone;

            //    clone.Links.Add(linkClone);
            //}
        }

        public static IEnumerable<IHydroRegion> GetAllRegions(IHydroRegion parentRegion)
        {
            yield return parentRegion;
            foreach (IHydroRegion subRegion in parentRegion.SubRegions.OfType<IHydroRegion>().SelectMany(GetAllRegions))
            {
                yield return subRegion;
            }
        }

        public override object Clone()
        {
            var clone = (HydroRegion) base.Clone();
            clone.Name = Name;
            clone.Geometry = (IGeometry) Geometry?.Clone();
            clone.Attributes = (IFeatureAttributeCollection) Attributes?.Clone();

            clone.isCloning = true;
            CloneAndAddLinks(this, clone);
            clone.isCloning = false;
            return clone;
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            return SubRegions;
        }

        [EditAction]
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