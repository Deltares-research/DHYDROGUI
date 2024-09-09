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
    /// <summary>
    /// Drainage basin is defined as a set of catchments (sub-basins) covering some drainage area,
    /// including a set of related hydrographic features such as waste-water treatment plants.
    /// </summary>
    [Entity]
    [DisplayName("Drainage Basin")]
    public class DrainageBasin : RegionBase, IDrainageBasin
    {
        public DrainageBasin()
        {
            Name = "drainage basin";
            Catchments = new EventedList<Catchment>();
            WasteWaterTreatmentPlants = new EventedList<WasteWaterTreatmentPlant>();
            Boundaries = new EventedList<RunoffBoundary>();
            Links = new EventedList<HydroLink>();
            
            CatchmentTypes = new EventedList<CatchmentType>();
            
            CatchmentTypes.Add((CatchmentType)CatchmentType.Paved.Clone());
            CatchmentTypes.Add((CatchmentType)CatchmentType.Unpaved.Clone());
            CatchmentTypes.Add((CatchmentType)CatchmentType.GreenHouse.Clone());
            CatchmentTypes.Add((CatchmentType)CatchmentType.OpenWater.Clone());
            CatchmentTypes.Add((CatchmentType)CatchmentType.Sacramento.Clone());
            CatchmentTypes.Add((CatchmentType)CatchmentType.Hbv.Clone());
        }

        public virtual IEventedList<CatchmentType> CatchmentTypes { get; set; }

        private IEventedList<Catchment> catchments;

        private IEventedList<WasteWaterTreatmentPlant> wasteWaterTreatmentPlants;

        private IEventedList<HydroLink> links;

        public virtual IEventedList<Catchment> Catchments
        {
            get { return catchments; }
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
            get { return wasteWaterTreatmentPlants; } 
            protected set
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

        public virtual IEventedList<RunoffBoundary> Boundaries
        {
            get { return boundaries; }
            protected set
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

        public virtual IEnumerable<IHydroObject> AllHydroObjects
        {
            get
            {
                return AllCatchments.Cast<IHydroObject>().Concat(WasteWaterTreatmentPlants).Concat(Boundaries);
            }
        }

        private bool allCatchmentsDirty = true;
        private IList<Catchment> cachedAllCatchments = new List<Catchment>();
        private IEventedList<RunoffBoundary> boundaries;

        public virtual IEnumerable<Catchment> AllCatchments
        {
            get
            {
                //for performance.. (used in Catchment CompareTo, among others)
                if (allCatchmentsDirty)
                {
                    cachedAllCatchments = catchments.ToList();
                    allCatchmentsDirty = false;
                }
                return cachedAllCatchments;
            }
        }

        public virtual IEventedList<HydroLink> Links
        {
            get { return links; }
            protected set
            {
                if(links != null)
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

        private bool isCloning;

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
                    link.Source.Links.Remove(link);
                    link.Target.Links.Remove(link);
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    lock(link.Source.Links)
                        link.Source.Links.Add(link);
                    lock(link.Target.Links)
                        link.Target.Links.Add(link);
                }
                else
                {
                    throw new NotSupportedException();
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
                    Geometry = Geometry != null ? (IGeometry)Geometry.Clone() : null,
                    Attributes = Attributes != null ? (IFeatureAttributeCollection)Attributes.Clone() : null,
                };

            foreach (var plant in WasteWaterTreatmentPlants)
            {
                clone.WasteWaterTreatmentPlants.Add((WasteWaterTreatmentPlant)plant.Clone());
            }

            foreach (var boundary in Boundaries)
            {
                clone.Boundaries.Add((RunoffBoundary)boundary.Clone());
            }

            foreach (var subRegion in SubRegions)
            {
                clone.SubRegions.Add((IHydroRegion)subRegion.Clone());
            }

            foreach (var catchment in Catchments)
            {
                clone.Catchments.Add((Catchment)catchment.Clone());
            }
            clone.isCloning = true;
            HydroRegion.CloneAndAddLinks(this, clone);
            clone.isCloning = false;
            return clone;
        }

        void OnCatchmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is IEventedList<Catchment>))
                return;

            allCatchmentsDirty = true;

            HandleCatchmentsCollectionChanged(sender, e);
        }

        void HandleCatchmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var catchment = (Catchment)e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    catchment.Basin = this;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    catchment.Links.ToArray().ForEach(HydroRegion.RemoveLink);
                    catchment.Basin = null;
                    break;
            }
        }

        private void OnBoundariesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != Boundaries)
                return;

            var boundaryNode = (RunoffBoundary)e.GetRemovedOrAddedItem();

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

        private void OnWasteWaterTreatmentPlantsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != WasteWaterTreatmentPlants)
                return;

            var wasteWaterTreatmentPlant = (WasteWaterTreatmentPlant)e.GetRemovedOrAddedItem();

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

        public override IEnumerable<object> GetDirectChildren()
        {
            return Catchments.Cast<object>()
                             .Union(WasteWaterTreatmentPlants)
                             .Union(Boundaries)
                             .Union(Links);
        }
    }
}