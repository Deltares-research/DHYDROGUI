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
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [DisplayName("Region")]
    [Entity]
    public class HydroRegion : RegionBase, IHydroRegion
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroRegion));
        private IEventedList<HydroLink> links;

        private bool isCloning;

        public HydroRegion()
        {
            Initialize();
        }

        private void Initialize()
        {
            Links = new EventedList<HydroLink>();
        }

        public override IEventedList<IRegion> SubRegions
        {
            get { return base.SubRegions; } 
            set
            {
                if (base.SubRegions != null)
                {
                    base.SubRegions.CollectionChanging -= OnSubRegionsCollectionChanging;
                }
                
                base.SubRegions = value;

                if (base.SubRegions != null)
                {
                    foreach (var subregion in base.SubRegions)
                    {
                        subregion.Parent = this;
                    }
                    base.SubRegions.CollectionChanging += OnSubRegionsCollectionChanging;
                }
            }
        }

        public virtual IEnumerable<IHydroObject> AllHydroObjects { get { return SubRegions.OfType<IHydroRegion>().SelectMany(r => r.AllHydroObjects); } }

        public virtual IEventedList<HydroLink> Links
        {
            get { return links; }
            set
            {
                if (links != null)
                {
                    links.CollectionChanged -= OnLinksCollectionChanged;
                }

                links = value;

                if (links != null)
                {
                    links.CollectionChanged += OnLinksCollectionChanged;
                }
            }
        }

        public override object Clone()
        {
            var clone = (HydroRegion) base.Clone();
            clone.Initialize();
            clone.Name = Name;
            clone.Geometry = (IGeometry) (Geometry != null ? Geometry.Clone() : null);
            clone.Attributes = (IFeatureAttributeCollection) (Attributes != null ? Attributes.Clone() : null);
            
            clone.isCloning = true;
            CloneAndAddLinks(this, clone);
            clone.isCloning = false;
            return clone;
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            return SubRegions.Cast<object>().Union(Links.Cast<object>());
        }

        /// <summary>
        /// Adds a new <see cref="HydroLink"/> between <paramref name="source"/> and <paramref name="target"/>
        /// in their shared <see cref="HydroRegion"/>.
        /// </summary>
        /// <param name="source"> The source hydro object to link from. </param>
        /// <param name="target"> The target hydro object to link to. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static HydroLink AddNewLink(IHydroObject source, IHydroObject target)
        {
            var link = new HydroLink(source, target)
            {
                Geometry = new LineString(new[]
                {
                    source.LinkingCoordinate,
                    target.LinkingCoordinate
                })
            };

            var commonRegion = GetCommonRegion(source, target);
            if (commonRegion != null)
            {
                lock (commonRegion.Links)
                {
                    commonRegion.Links.Add(link);
                }
            }
            else
            {
                log.Warn($"could not find common region for {source?.Name} and {target?.Name}, linking is probably wrong");
            }
            return link;
        }

        public static void RemoveLink(IHydroObject source, IHydroObject target)
        {
            var link = source.Links.First(l => Equals(l.Source, source) && Equals(l.Target, target));

            var commonRegion = GetCommonRegion(source, target);
            
            commonRegion.Links.Remove(link);
        }

        public static bool CanLinkTo(IHydroObject source, IHydroObject target)
        {
            if (!source.CanBeLinkSource)
            {
                return false;
            }

            if (!target.CanBeLinkTarget)
            {
                return false;
            }

            if (Equals(source, target))
            {
                return false;
            }

            // source and target have common parent region
            if(GetCommonRegion(source, target) == null)
            {
                return false;
            }

            // allowed links
            if ((source is Catchment || source is WasteWaterTreatmentPlant)
                && (target is Catchment || target is WasteWaterTreatmentPlant || target is RunoffBoundary || target is LateralSource))
            {
                var catchmentSource = source as Catchment;
                if (catchmentSource != null)
                {
                    return catchmentSource.CatchmentType != null;
                }

                return true;
            }

            if (source is Embankment && target is LateralSource)
            {
                return true;
            }

            return false;
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
            
            var originalObjects = original.AllHydroObjects.ToList();
            var clonedObjects = clone.AllHydroObjects.ToList();

            foreach (var link in original.Links)
            {
                var linkClone = (HydroLink) link.Clone();

                // repair link
                linkClone.Source = clonedObjects[originalObjects.IndexOf(link.Source)];
                linkClone.Target = clonedObjects[originalObjects.IndexOf(link.Target)];

                // replace link in source and target objects
                var linkInSourceIndex = linkClone.Source.Links.IndexOf(link);
                linkClone.Source.Links[linkInSourceIndex] = linkClone;

                var linkInTargetIndex = linkClone.Target.Links.IndexOf(link);
                linkClone.Target.Links[linkInTargetIndex] = linkClone;

                clone.Links.Add(linkClone);
            }
        }

        public static IEnumerable<IHydroRegion> GetAllRegions(IHydroRegion parentRegion)
        {
            yield return parentRegion;
            foreach (var subRegion in parentRegion.SubRegions.OfType<IHydroRegion>().SelectMany(GetAllRegions))
            {
                yield return subRegion;
            }
        }

        public static void RemoveLink(HydroLink link)
        {
            RemoveLink(link.Source, link.Target);
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
                    var links = subRegion.GetAllItemsRecursive().OfType<IHydroObject>().Where(o => o.Links != null)
                        .SelectMany(o => o.Links).Where(l => Links.Contains(l)).ToArray();
                    foreach (var link in links)
                    {
                        RemoveLink(link);
                    }
                    break;
            }
        }

        private void OnLinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (isCloning)
            {
                return;
            }

            if (e.GetRemovedOrAddedItem() is HydroLink link)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    link.Source.UnlinkFrom(link.Target);
                    link.Source.Links.Remove(link);
                    link.Target.Links.Remove(link);
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    lock (link.Source.Links)
                        link.Source.Links.Add(link);
                    
                    lock (link.Target.Links)
                        link.Target.Links.Add(link);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        void IHydroRegion.RemoveLink(IHydroObject source, IHydroObject target)
        {
            RemoveLink(source, target);
        }

        bool IHydroRegion.CanLinkTo(IHydroObject source, IHydroObject target)
        {
            return CanLinkTo(source, target);
        }

        HydroLink IHydroRegion.AddNewLink(IHydroObject source, IHydroObject target)
        {
            return AddNewLink(source, target);
        }
    }
}