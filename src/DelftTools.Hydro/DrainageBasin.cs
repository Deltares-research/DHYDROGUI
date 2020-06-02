using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public interface IDrainageBasin : IHydroRegion
    {
        IEventedList<Catchment> Catchments { get; }
        IEventedList<WasteWaterTreatmentPlant> WasteWaterTreatmentPlants { get; set; }
        IEnumerable<Catchment> AllCatchments { get; }
    }

    /// <summary>
    /// Drainage basin is defined as a set of catchments (sub-basins) covering some drainage area, including a set of
    /// related hydgraphic features such as waste-water treatment plants.
    /// </summary>
    [Entity]
    [DisplayName("Drainage Basin")]
    public class DrainageBasin : RegionBase, IDrainageBasin
    {
        private IEventedList<Catchment> catchments;

        private IEventedList<WasteWaterTreatmentPlant> wasteWaterTreatmentPlants;

        private IEventedList<HydroLink> links;

        private bool allCatchmentsDirty = true;
        private IList<Catchment> cachedAllCatchments = new List<Catchment>();
        private IEventedList<RunoffBoundary> boundaries;

        private bool isCloning;

        public DrainageBasin()
        {
            Name = "drainage basin";
            Catchments = new EventedList<Catchment>();
            WasteWaterTreatmentPlants = new EventedList<WasteWaterTreatmentPlant>();
            Boundaries = new EventedList<RunoffBoundary>();
            Links = new EventedList<HydroLink>();

            CatchmentTypes = new EventedList<CatchmentType>();

            // TODO: inject by a specific model plugin and not here!
            CatchmentTypes.Add((CatchmentType) CatchmentType.Polder.Clone());
            CatchmentTypes.Add((CatchmentType) CatchmentType.Paved.Clone());
            CatchmentTypes.Add((CatchmentType) CatchmentType.Unpaved.Clone());
            CatchmentTypes.Add((CatchmentType) CatchmentType.GreenHouse.Clone());
            CatchmentTypes.Add((CatchmentType) CatchmentType.OpenWater.Clone());
            CatchmentTypes.Add((CatchmentType) CatchmentType.Sacramento.Clone());
            CatchmentTypes.Add((CatchmentType) CatchmentType.Hbv.Clone());
        }

        public virtual IEventedList<CatchmentType> CatchmentTypes { get; set; }

        public virtual IEventedList<RunoffBoundary> Boundaries
        {
            get => boundaries;
            set
            {
                if (boundaries != null)
                {
                    boundaries.CollectionChanged -= OnBoundariesCollectionChanged;
                }

                boundaries = value;
                if (boundaries != null)
                {
                    boundaries.CollectionChanged += OnBoundariesCollectionChanged;
                }
            }
        }

        public virtual IEventedList<Catchment> Catchments
        {
            get => catchments;
            protected set
            {
                if (catchments != null)
                {
                    catchments.CollectionChanged -= OnCatchmentsCollectionChanged;
                }

                catchments = value;
                if (catchments != null)
                {
                    catchments.CollectionChanged += OnCatchmentsCollectionChanged;
                }
            }
        }

        public virtual IEventedList<WasteWaterTreatmentPlant> WasteWaterTreatmentPlants
        {
            get => wasteWaterTreatmentPlants;
            set
            {
                if (wasteWaterTreatmentPlants != null)
                {
                    wasteWaterTreatmentPlants.CollectionChanged -= OnWasteWaterTreatmentPlantsCollectionChanged;
                }

                wasteWaterTreatmentPlants = value;
                if (wasteWaterTreatmentPlants != null)
                {
                    wasteWaterTreatmentPlants.CollectionChanged += OnWasteWaterTreatmentPlantsCollectionChanged;
                }
            }
        }

        public virtual IEnumerable<IHydroObject> AllHydroObjects =>
            AllCatchments.Cast<IHydroObject>().Concat(WasteWaterTreatmentPlants).Concat(Boundaries);

        public virtual IEnumerable<Catchment> AllCatchments
        {
            get
            {
                //for performance.. (used in Catchment CompareTo, among others)
                if (allCatchmentsDirty)
                {
                    cachedAllCatchments = GetAllCatchments(catchments).ToList();
                    allCatchmentsDirty = false;
                }

                return cachedAllCatchments;
            }
        }

        public virtual IEventedList<HydroLink> Links
        {
            get => links;
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

        public virtual HydroLink AddNewLink(IHydroObject source, IHydroObject target)
        {
            return HydroRegion.AddNewLink(source, target);
        }

        public virtual void RemoveLink(IHydroObject source, IHydroObject target)
        {
            HydroRegion.RemoveLink(source, target);
        }

        public virtual bool CanLinkTo(IHydroObject source, IHydroObject target)
        {
            return HydroRegion.CanLinkTo(source, target);
        }

        public override object Clone()
        {
            var clone = new DrainageBasin
            {
                Name = Name,
                Geometry = Geometry != null ? (IGeometry) Geometry.Clone() : null,
                Attributes = Attributes != null ? (IFeatureAttributeCollection) Attributes.Clone() : null,
            };

            foreach (WasteWaterTreatmentPlant plant in WasteWaterTreatmentPlants)
            {
                clone.WasteWaterTreatmentPlants.Add((WasteWaterTreatmentPlant) plant.Clone());
            }

            foreach (RunoffBoundary boundary in Boundaries)
            {
                clone.Boundaries.Add((RunoffBoundary) boundary.Clone());
            }

            foreach (IRegion subRegion in SubRegions)
            {
                clone.SubRegions.Add((IHydroRegion) subRegion.Clone());
            }

            foreach (Catchment catchment in Catchments)
            {
                clone.Catchments.Add((Catchment) catchment.Clone());
            }

            clone.isCloning = true;
            HydroRegion.CloneAndAddLinks(this, clone);
            clone.isCloning = false;
            return clone;
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            return Catchments.Cast<object>()
                             .Union(WasteWaterTreatmentPlants.Cast<object>())
                             .Union(Boundaries.Cast<object>())
                             .Union(Links.Cast<object>());
        }

        private IEnumerable<Catchment> GetAllCatchments(IEnumerable<Catchment> catchments)
        {
            foreach (Catchment catchment in catchments)
            {
                yield return catchment;

                foreach (Catchment subCatchment in GetAllCatchments(catchment.SubCatchments))
                {
                    yield return subCatchment;
                }
            }
        }

        [EditAction]
        private void OnLinksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (isCloning)
            {
                return;
            }

            var link = e.GetRemovedOrAddedItem() as HydroLink;

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                link.Source.Links.Remove(link);
                link.Target.Links.Remove(link);
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                link.Source.Links.Add(link);
                link.Target.Links.Add(link);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void OnCatchmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is IEventedList<Catchment>)) //catchments, or subcatchments list
            {
                return;
            }

            allCatchmentsDirty = true;

            HandleCatchmentsCollectionChanged(sender, e);
        }

        [EditAction]
        private void HandleCatchmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var catchment = (Catchment) e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    catchment.Basin = this;
                    foreach (Catchment subcatchment in catchment.SubCatchments)
                    {
                        subcatchment.Basin = this;
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    catchment.Links.ToArray().ForEach(HydroRegion.RemoveLink);
                    catchment.Basin = null;
                    foreach (Catchment subcatchment in catchment.SubCatchments)
                    {
                        subcatchment.Links.ToArray().ForEach(HydroRegion.RemoveLink);
                        subcatchment.Basin = null;
                    }

                    break;
            }
        }

        [EditAction]
        private void OnBoundariesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != Boundaries)
            {
                return;
            }

            var boundaryNode = (RunoffBoundary) e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    boundaryNode.Basin = this;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    boundaryNode.Links.ToArray().ForEach(HydroRegion.RemoveLink);
                    boundaryNode.Basin = null;
                    break;
            }
        }

        [EditAction]
        private void OnWasteWaterTreatmentPlantsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != WasteWaterTreatmentPlants)
            {
                return;
            }

            var wasteWaterTreatmentPlant = (WasteWaterTreatmentPlant) e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    wasteWaterTreatmentPlant.Basin = this;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    wasteWaterTreatmentPlant.Links.ToArray().ForEach(HydroRegion.RemoveLink);
                    wasteWaterTreatmentPlant.Basin = null;
                    break;
            }
        }
    }
}